using System;
using System.Reflection;
using UnityEngine;

namespace Framework.Kits.SingletonKits
{
	public static class SingletonCreator
	{
		public static bool MonoSingletonAutoCreate { get; set; } = false;
		public static T CreateSingleton<T>() where T : class, ISingleton {
			Type type = typeof(T);
			Type monoBehaviourType = typeof(MonoBehaviour);

			if (monoBehaviourType.IsAssignableFrom(type)) {
				return CreateMonoSingleton<T>();
			}
			var instance = CreateNonPublicConstructorObject<T>();
			instance.OnSingletonInit();
			return instance;
		}
		private static T CreateNonPublicConstructorObject<T>() where T : class {
			Type type = typeof(T);
			// 获取私有构造函数
			var constructorInfos = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
			// 获取无参构造函数
			ConstructorInfo ctor = Array.Find(constructorInfos, c => c.GetParameters().Length == 0);
			return ctor.Invoke(null) as T;
		}
		/// <summary>
		/// 泛型方法：创建MonoBehaviour单例
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T CreateMonoSingleton<T>() where T : class, ISingleton {
			Type type = typeof(T);

			//判断当前场景中是否存在T实例
			if (UnityEngine.Object.FindObjectOfType(type) is not T instance) {
				if (MonoSingletonAutoCreate) {
					//如果还是无法找到instance  则主动去创建同名Obj 并挂载相关脚本 组件
					var obj = new GameObject(typeof(T).Name);
					UnityEngine.Object.DontDestroyOnLoad(obj);
					instance = obj.AddComponent(typeof(T)) as T;
				}
				else {
					throw new FormatException($"MonoSingleton {type} does not exist,please create manually");
				}
			}

			instance.OnSingletonInit();
			return instance;
		}
	}
}