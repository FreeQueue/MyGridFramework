using UnityEngine;

namespace Framework.Kits.SingletonKits
{
	public class MonoSingleton<T> :MonoBehaviour, ISingleton where T : MonoSingleton<T>
	{
		/// <summary>
		/// 静态实例
		/// </summary>
		protected static T instance;

		/// <summary>
		/// 静态属性：封装相关实例对象
		/// </summary>
		public static T Instance
		{
			get
			{
				if (instance == null && !_onApplicationQuit)
				{
					instance = SingletonCreator.CreateMonoSingleton<T>();
				}

				return instance;
			}
		}
		public virtual void OnSingletonInit() {
			
		}
		
		/// <summary>
		/// 当前应用程序是否结束 标签
		/// </summary>
		protected static bool _onApplicationQuit = false;
		/// <summary>
		/// 判断当前应用程序是否退出
		/// </summary>
		public static bool IsApplicationQuit => _onApplicationQuit;
		/// <summary>
		/// 应用程序退出：释放当前对象并销毁相关GameObject
		/// </summary>
		protected virtual void OnApplicationQuit()
		{
			_onApplicationQuit = true;
			if (instance == null) return;
			Destroy(instance.gameObject);
			instance = null;
		}
	}
}