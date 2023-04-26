using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Extensions;
using SAssembly = System.Reflection.Assembly;
using SType = System.Type;
using SArray = System.Array;

namespace Framework
{
	partial class Util
	{
		public static class Type
		{
			public static readonly Cache<string, SType> TypeCache;
			public static readonly Cache<SType, ConstructorInfo> CtorCache;

			static Type() {
				TypeCache = new Cache<string, SType>(GetType);
				CtorCache = new Cache<SType, ConstructorInfo>(GetCtor);
			}

			/// 获取已加载的程序集中的指定类型。
			private static SType GetType(string typeName) {
				var type = SType.GetType(typeName);
				if (type != null) return type;

				foreach (var assembly in Assembly.AllAssemblies) {
					type = assembly.GetType(typeName);
					if (type == null) continue;
					return type;
				}
				return null;
			}

			private static ConstructorInfo GetCtor(SType type) {
				const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
				var ctors = type.GetConstructors(FLAGS).Where(x => x.HasAttribute<UseCtorAttribute>()).ToArray();
				if (ctors.Length > 1)
					throw new InvalidOperationException("UseCtor on multiple constructors; invalid.");
				return ctors.FirstOrDefault();
			}

			public static T Create<T>(string className)
			{
				return Create<T>(className, new Dictionary<string, object>());
			}
			
			public static T Create<T>(string className, Dictionary<string, object> args) {
				var type = TypeCache[className];
				if (type == null) throw new InvalidOperationException($"Cannot locate type: {className}");

				ConstructorInfo ctor = CtorCache[type];
				if (ctor == null)
					return (T)type.GetConstructor(SArray.Empty<SType>()).Invoke(SArray.Empty<object>());
				else
					return (T)ctor.Create(args);
			}

			/// 获取类型的所有子类，默认不包含抽象类
			/// <param name="hasAbstract">是否包括抽象类</param>
			public static void GetAllSubclassesOfType(
				SType baseType, IDictionary<string, SType> result, bool hasAbstract = false
			) {
				if (!baseType.IsClass || baseType.IsSealed) throw new InvalidOperationException("Invalid baseType.");
				result.Clear();
				foreach (SAssembly assembly in Assembly.AllAssemblies) {
					var types = hasAbstract
						? from type in assembly.GetTypes()
						where type.IsSubclassOf(baseType)
						select type
						: from type in assembly.GetTypes()
						where type.IsSubclassOf(baseType) && !type.IsAbstract
						select type;

					foreach (SType type in types) {
						result[type.Name] = type;
					}
				}
			}

			/// 获取类型的所有实现类，默认不包含抽象类
			/// <param name="hasAbstract">是否包括抽象类</param>
			public static void GetAllImplementsOfType(
				SType baseType, IDictionary<string, SType> result, bool hasAbstract = false
			) {
				if (!baseType.IsClass || !baseType.IsInterface || baseType.IsSealed)
					throw new InvalidOperationException("Invalid baseType.");
				result.Clear();
				foreach (SAssembly assembly in Assembly.AllAssemblies) {
					var types = hasAbstract
						? from type in assembly.GetTypes()
						where baseType.IsAssignableFrom(type)
						select type
						: from type in assembly.GetTypes()
						where baseType.IsAssignableFrom(type) && !type.IsAbstract
						select type;

					foreach (SType type in types) {
						result[type.Name] = type;
					}
				}
			}

			[AttributeUsage(AttributeTargets.Constructor)]
			public sealed class UseCtorAttribute : Attribute
			{
			}
		}
	}
}