/****************************************************************************
 * Copyright (c) 2015 - 2022 liangxiegame UNDER MIT License
 * 
 * http://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework.Extensions
{

	public static class SystemReflectionExts
	{
		public static IEnumerable<string> GetNamespaces(this Assembly a) {
			return a.GetTypes().Select(t => t.Namespace).Distinct().Where(n => n != null);
		}
		public static object ReflectionCallPrivateMethod<T>(this T self, string methodName, params object[] args) {
			Type type = typeof(T);
			MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

			return methodInfo?.Invoke(self, args);
		}

		public static TReturnType ReflectionCallPrivateMethod<T, TReturnType>(
			this T self, string methodName,
			params object[] args
		) {
			return (TReturnType)self.ReflectionCallPrivateMethod(methodName, args);
		}

		public static bool HasAttribute<T>(this MemberInfo @this, bool inherit = false) where T : Attribute {
			return Attribute.IsDefined(@this, typeof(T), inherit);
		}

		public static bool HasAttribute(this MemberInfo @this, Type attributeType, bool inherit = false) {
			return Attribute.IsDefined(@this, attributeType, inherit);
		}
		
		public static bool HasAttribute<T>(this ParameterInfo @this, bool inherit = false) where T : Attribute {
			return Attribute.IsDefined(@this, typeof(T), inherit);
		}

		public static bool HasAttribute(this ParameterInfo @this, Type attributeType, bool inherit = false) {
			return Attribute.IsDefined(@this, attributeType, inherit);
		}
		
		public static bool HasAttribute<T>(this Module @this, bool inherit = false) where T : Attribute {
			return Attribute.IsDefined(@this, typeof(T), inherit);
		}

		public static bool HasAttribute(this Module @this, Type attributeType, bool inherit = false) {
			return Attribute.IsDefined(@this, attributeType, inherit);
		}
		
		public static bool HasAttribute<T>(this Assembly @this, bool inherit = false) where T : Attribute {
			return Attribute.IsDefined(@this, typeof(T), inherit);
		}

		public static bool HasAttribute(this Assembly @this, Type attributeType, bool inherit = false) {
			return Attribute.IsDefined(@this, attributeType, inherit);
		}

		public static object Create(this ConstructorInfo @this, Dictionary<string, object> args) {
			var p = @this.GetParameters();
			var a = new object[p.Length];
			for (var i = 0; i < p.Length; i++) {
				var key = p[i].Name;
				if (!args.TryGetValue(key, out var value))
					throw new InvalidOperationException($"ObjectCreator: key `{key}' not found");
				a[i] = value;
			}

			return @this.Invoke(a);
		}
		
		public static IEnumerable<Type> BaseTypes(this Type @this)
		{
			while (@this != null)
			{
				yield return @this;
				@this = @this.BaseType;
			}
		}
	}
}