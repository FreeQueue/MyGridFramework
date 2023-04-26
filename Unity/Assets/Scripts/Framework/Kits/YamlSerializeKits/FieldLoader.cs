#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Framework.Extensions;
using Framework.Kits.BitSetKits;
using Framework.Kits.ExpressionKits;
using Framework.Kits.MiniYamlKits;
using UnityEngine;

namespace Framework.Kits.YamlSerializeKits
{
	public static partial class FieldLoader
	{
		public static Func<string, Type, string, object> InvalidValueAction = (s, t, f) =>
			throw new YamlException($"FieldLoader: Cannot parse `{s}` into `{f}.{t}` ");

		public static Action<string, Type> UnknownFieldAction = (s, f) =>
			throw new NotImplementedException($"FieldLoader: Missing field `{s}` on `{f.Name}`");

		private static readonly ConcurrentCache<Type, FieldLoadInfo[]> _typeLoadInfo =
			new ConcurrentCache<Type, FieldLoadInfo[]>(BuildTypeLoadInfo);
		private static readonly ConcurrentCache<string, BooleanExpression> _booleanExpressionCache =
			new ConcurrentCache<string, BooleanExpression>(expression => new BooleanExpression(expression));
		private static readonly ConcurrentCache<string, IntegerExpression> _integerExpressionCache =
			new ConcurrentCache<string, IntegerExpression>(expression => new IntegerExpression(expression));

		private static readonly Dictionary<Type, Func<string, Type, string, MemberInfo, object>> _typeParsers =
			new Dictionary<Type, Func<string, Type, string, MemberInfo, object>> {
				{ typeof(bool), ParseBool },
				{ typeof(int), ParseInt32 },
				{ typeof(ushort), ParseUint16 },
				{ typeof(long), ParseInt64 },
				{ typeof(float), ParseFloat },
				{ typeof(decimal), ParseDecimal },
				{ typeof(string), ParseString },
				{ typeof(DateTime), ParseDateTime },
				{ typeof(Enum), ParseEnum },
				{ typeof(Color), ParseColor32 },
				{ typeof(Vector2), ParseVector2 },
				{ typeof(Vector3), ParseVector3 },
				{ typeof(Vector2Int), ParseVector2Int },
				{ typeof(Vector2Int[]), ParseVector2IntArray },
				{ typeof(BooleanExpression), ParseBooleanExpression },
				{ typeof(IntegerExpression), ParseIntegerExpression },
			};

		private static readonly Dictionary<Type, Func<string, Type, string, MiniYaml, MemberInfo, object>>
			_genericTypeParsers =
				new Dictionary<Type, Func<string, Type, string, MiniYaml, MemberInfo, object>> {
					{ typeof(HashSet<>), ParseHashSetOrList },
					{ typeof(List<>), ParseHashSetOrList },
					{ typeof(Dictionary<,>), ParseDictionary },
					{ typeof(BitSet<>), ParseBitSet },
					{ typeof(Nullable<>), ParseNullable },
				};

		public static void Load(object self, MiniYaml yaml) {
			FieldLoadInfo[] loadInfo = _typeLoadInfo[self.GetType()];
			List<string> missing = new List<string>();

			Dictionary<string, MiniYaml> yamls = null;

			foreach (FieldLoadInfo fieldLoadInfo in loadInfo) {
				object val;

				yamls ??= yaml.ToDictionary();
				if (fieldLoadInfo.Loader != null) {
					if (!fieldLoadInfo.Attribute.Required || yamls.ContainsKey(fieldLoadInfo.YamlName)) {
						val = fieldLoadInfo.Loader(yaml);
					}
					else {
						missing.Add(fieldLoadInfo.YamlName);
						continue;
					}
				}
				else {
					if (!TryGetValueFromYaml(fieldLoadInfo.YamlName, fieldLoadInfo.Field, yamls, out val)) {
						if (fieldLoadInfo.Attribute.Required) {
							missing.Add(fieldLoadInfo.YamlName);
						}
						continue;
					}
				}

				fieldLoadInfo.Field.SetValue(self, val);
			}

			if (missing.Count > 0) {
				throw new MissingFieldsException(missing.ToArray());
			}
		}

		private static bool TryGetValueFromYaml(
			string yamlName, FieldInfo field, Dictionary<string, MiniYaml> md, out object ret
		) {
			ret = null;

			if (!md.TryGetValue(yamlName, out MiniYaml yaml)) {
				return false;
			}

			ret = GetValue(field.Name, field.FieldType, yaml, field);
			return true;
		}

		public static T Load<T>(MiniYaml y) where T : new() {
			var t = new T();
			Load(t, y);
			return t;
		}

		public static void LoadField(object target, string key, string value) {
			const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			key = key.Trim();

			FieldInfo field = target.GetType().GetField(key, FLAGS);
			if (field != null) {
				SerializeAttribute sa = field.GetCustomAttributes<SerializeAttribute>(false)
					.DefaultIfEmpty(SerializeAttribute.Default).First();
				if (!sa.FromYamlKey) {
					field.SetValue(target, GetValue(field.Name, field.FieldType, value, field));
				}
				return;
			}

			PropertyInfo prop = target.GetType().GetProperty(key, FLAGS);
			if (prop != null) {
				SerializeAttribute sa = prop.GetCustomAttributes<SerializeAttribute>(false)
					.DefaultIfEmpty(SerializeAttribute.Default).First();
				if (!sa.FromYamlKey) {
					prop.SetValue(target, GetValue(prop.Name, prop.PropertyType, value, prop), null);
				}
				return;
			}

			UnknownFieldAction(key, target.GetType());
		}

		public static T GetValue<T>(string field, string value) {
			return (T)GetValue(field, typeof(T), value);
		}

		public static object GetValue(string fieldName, Type fieldType, string value, MemberInfo field = null) {
			return GetValue(fieldName, fieldType, new MiniYaml(value), field);
		}

		public static object GetValue(string fieldName, Type fieldType, MiniYaml yaml, MemberInfo field) {
			string value = yaml.Value?.Trim();
			if (fieldType.IsGenericType) {
				if (_genericTypeParsers.TryGetValue(fieldType.GetGenericTypeDefinition(),
						out Func<string, Type, string, MiniYaml, MemberInfo, object> parseFuncGeneric)) {
					return parseFuncGeneric(fieldName, fieldType, value, yaml, field);
				}
			}
			else {
				if (_typeParsers.TryGetValue(fieldType, out Func<string, Type, string, MemberInfo, object> parseFunc)) {
					return parseFunc(fieldName, fieldType, value, field);
				}

				if (fieldType.IsArray && fieldType.GetArrayRank() == 1) {
					if (value == null) {
						return Array.CreateInstance(fieldType.GetElementType(), 0);
					}

					StringSplitOptions options
						= field != null && field.GetCustomAttribute<SerializeAttribute>() is {AllowEmptyEntries:true}
							? StringSplitOptions.None
							: StringSplitOptions.RemoveEmptyEntries;
					string[] parts = value.Split(SPLIT_COMMA, options);

					var result = Array.CreateInstance(fieldType.GetElementType(), parts.Length);
					for (int i = 0; i < parts.Length; i++) {
						result.SetValue(GetValue(fieldName, fieldType.GetElementType(), parts[i].Trim(), field), i);
					}
					return result;
				}
			}

			TypeConverter conv = TypeDescriptor.GetConverter(fieldType);
			if (conv.CanConvertFrom(typeof(string))) {
				try {
					return conv.ConvertFromInvariantString(value);
				}
				catch {
					return InvalidValueAction(value, fieldType, fieldName);
				}
			}

			UnknownFieldAction($"[Type] {value}", fieldType);
			return null;
		}

		public static IEnumerable<FieldLoadInfo> GetTypeLoadInfo(Type type, bool includePrivateByDefault = false) {
			return _typeLoadInfo[type].Where(fli =>
				includePrivateByDefault || fli.Field.IsPublic || fli.Attribute.Serialize && !fli.Attribute.IsDefault);
		}

		private static FieldLoadInfo[] BuildTypeLoadInfo(Type type) {
			List<FieldLoadInfo> result = new List<FieldLoadInfo>();

			foreach (FieldInfo fieldInfo in type.GetFields(
						BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {

				SerializeAttribute serializeAttribute = fieldInfo.GetCustomAttributes<SerializeAttribute>(false)
					.DefaultIfEmpty(SerializeAttribute.Default).First();
				if (!serializeAttribute.Serialize) continue;

				string fieldName = string.IsNullOrEmpty(serializeAttribute.FieldName)
					? fieldInfo.Name
					: serializeAttribute.FieldName;

				Func<MiniYaml, object> loader = serializeAttribute.GetLoader(type);
				if (loader == null && serializeAttribute.FromYamlKey) {
					loader = yaml => GetValue(fieldName, fieldInfo.FieldType, yaml, fieldInfo);
				}

				var fieldLoadInfo = new FieldLoadInfo(fieldInfo, serializeAttribute, fieldName, loader);
				result.Add(fieldLoadInfo);
			}

			return result.ToArray();
		}
	}
}