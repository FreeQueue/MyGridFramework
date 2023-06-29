using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Framework;
using Framework.Kits.MiniYamlKits;
using UnityEngine;

namespace Framework.Kits.YamlSerializeKits
{
	public static partial class FieldLoader
	{
		private const char SPLIT_COMMA = ',';
		private static object ParseInt32(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (Util.Convert.TryParseInt32(value, out int res)) return res;
			return InvalidValueAction(value, fieldType, fieldName);
		}

		private static object ParseVector2Int(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (Util.Convert.TryParseVector2Int(value, out Vector2Int res)) return res;
			return InvalidValueAction(value, fieldType, fieldName);
		}
		private static object ParseUint16(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (ushort.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out ushort res)) {
				return res;
			}
			return InvalidValueAction(value, fieldType, fieldName);
		}
		private static object ParseInt64(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (long.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out long res)) {
				return res;
			}
			return InvalidValueAction(value, fieldType, fieldName);
		}
		private static object ParseFloat(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (value != null && float.TryParse(value.Replace("%", ""), NumberStyles.Float,
					NumberFormatInfo.InvariantInfo, out float res)) {
				return res * (value.Contains('%') ? 0.01f : 1f);
			}
			return InvalidValueAction(value, fieldType, fieldName);
		}
		private static object ParseDecimal(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (value != null && decimal.TryParse(value.Replace("%", ""), NumberStyles.Float,
					NumberFormatInfo.InvariantInfo, out decimal res)) {
				return res * (value.Contains('%') ? 0.01m : 1m);
			}
			return InvalidValueAction(value, fieldType, fieldName);
		}
		private static object ParseString(string fieldName, Type fieldType, string value, MemberInfo field) {
			return value;
		}
		private static object ParseColor32(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (Util.Convert.TryParseColor32(value, out Color32 color)) {
				return color;
			}
			return InvalidValueAction(value, fieldType, fieldName);
		}

		private static object ParseBooleanExpression(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (value != null) {
				try {
					return _booleanExpressionCache[value];
				}
				catch (InvalidDataException e) {
					throw new YamlException(e.Message);
				}
			}

			return InvalidValueAction(value, fieldType, fieldName);
		}
		private static object ParseIntegerExpression(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (value != null) {
				try {
					return _integerExpressionCache[value];
				}
				catch (InvalidDataException e) {
					throw new YamlException(e.Message);
				}
			}

			return InvalidValueAction(value, fieldType, fieldName);
		}
		private static object ParseEnum(string fieldName, Type fieldType, string value, MemberInfo field) {
			try {
				return Enum.Parse(fieldType, value, true);
			}
			catch (ArgumentException) {
				return InvalidValueAction(value, fieldType, fieldName);
			}
		}
		private static object ParseBool(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (bool.TryParse(value.ToLowerInvariant(), out bool result)) {
				return result;
			}
			return InvalidValueAction(value, fieldType, fieldName);
		}
		private static object ParseVector2IntArray(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (value != null) {
				string[] parts = value.Split(SPLIT_COMMA, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length % 2 != 0) {
					return InvalidValueAction(value, fieldType, fieldName);
				}

				Vector2Int[] ints = new Vector2Int[parts.Length / 2];
				for (int i = 0; i < ints.Length; i++) {
					ints[i] = new Vector2Int(
						int.Parse(parts[2 * i], NumberStyles.Integer, NumberFormatInfo.InvariantInfo),
						int.Parse(parts[2 * i + 1], NumberStyles.Integer, NumberFormatInfo.InvariantInfo));
				}
				return ints;
			}
			return InvalidValueAction(value, fieldType, fieldName);
		}
		private static object ParseVector2(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (value != null) {
				string[] parts = value.Split(SPLIT_COMMA, StringSplitOptions.RemoveEmptyEntries);
				float xx = 0;
				float yy = 0;
				if (float.TryParse(parts[0].Replace("%", ""), NumberStyles.Float, NumberFormatInfo.InvariantInfo,
						out float res)) {
					xx = res * (parts[0].Contains('%') ? 0.01f : 1f);
				}
				if (float.TryParse(parts[1].Replace("%", ""), NumberStyles.Float, NumberFormatInfo.InvariantInfo,
						out res)) {
					yy = res * (parts[1].Contains('%') ? 0.01f : 1f);
				}
				return new Vector2(xx, yy);
			}

			return InvalidValueAction(value, fieldType, fieldName);
		}
		private static object ParseVector3(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (value != null) {
				string[] parts = value.Split(SPLIT_COMMA, StringSplitOptions.RemoveEmptyEntries);
				float.TryParse(parts[0], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float x);
				float.TryParse(parts[1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float y);

				// z component is optional for compatibility with older float2 definitions
				float z = 0;
				if (parts.Length > 2) {
					float.TryParse(parts[2], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out z);
				}
				return new Vector3(x, y, z);
			}
			return InvalidValueAction(value, fieldType, fieldName);
		}
		
		private static object ParseDateTime(string fieldName, Type fieldType, string value, MemberInfo field) {
			if (DateTime.TryParseExact(value, "yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeUniversal, out DateTime dt)) {
				return dt;
			}
			return InvalidValueAction(value, fieldType, fieldName);
		}
		#region GenericTypeParse

		private static object ParseHashSetOrList(
			string fieldName, Type fieldType, string value, MiniYaml yaml, MemberInfo field
		) {
			object set = Activator.CreateInstance(fieldType);
			if (value == null) {
				return set;
			}

			string[] parts = value.Split(SPLIT_COMMA, StringSplitOptions.RemoveEmptyEntries);
			Type[] arguments = fieldType.GetGenericArguments();
			MethodInfo addMethod = fieldType.GetMethod(nameof(List<object>.Add), arguments);
			object[] addArgs = new object[1];
			for (int i = 0; i < parts.Length; i++) {
				addArgs[0] = GetValue(fieldName, arguments[0], parts[i].Trim(), field);
				addMethod.Invoke(set, addArgs);
			}

			return set;
		}
		private static object ParseDictionary(
			string fieldName, Type fieldType, string value, MiniYaml yaml, MemberInfo field
		) {
			object dict = Activator.CreateInstance(fieldType);
			Type[] arguments = fieldType.GetGenericArguments();
			MethodInfo addMethod = fieldType.GetMethod(nameof(Dictionary<object, object>.Add), arguments);
			object[] addArgs = new object[2];
			foreach (MiniYamlNode node in yaml.Nodes) {
				addArgs[0] = GetValue(fieldName, arguments[0], node.Key, field);
				addArgs[1] = GetValue(fieldName, arguments[1], node.Value, field);
				addMethod.Invoke(dict, addArgs);
			}

			return dict;
		}
		private static object ParseBitSet(
			string fieldName, Type fieldType, string value, MiniYaml yaml, MemberInfo field
		) {
			if (value != null) {
				string[] parts = value.Split(SPLIT_COMMA, StringSplitOptions.RemoveEmptyEntries);
				ConstructorInfo ctor = fieldType.GetConstructor(new[] { typeof(string[]) });
				return ctor.Invoke(new object[] { parts.Select(p => p.Trim()).ToArray() });
			}

			return InvalidValueAction(value, fieldType, fieldName);
		}
		private static object ParseNullable(
			string fieldName, Type fieldType, string value, MiniYaml yaml, MemberInfo field
		) {
			if (string.IsNullOrEmpty(value)) {
				return null;
			}

			Type innerType = fieldType.GetGenericArguments().First();
			object innerValue = GetValue("Nullable<T>", innerType, value, field);
			return fieldType.GetConstructor(new[] { innerType }).Invoke(new[] { innerValue });
		}
		#endregion
	}
}