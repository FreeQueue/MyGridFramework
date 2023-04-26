namespace Framework.Kits.SingletonKits
{
	public abstract class Singleton<T> : ISingleton where T : Singleton<T>
	{
		protected static T instance;
		private static readonly object s_lock = new object();
		public static T Instance {
			get {
				lock (s_lock) {
					instance ??= SingletonCreator.CreateSingleton<T>();
				}
				return instance;
			}
		}
		public virtual void OnSingletonInit() {
		}
	}

}