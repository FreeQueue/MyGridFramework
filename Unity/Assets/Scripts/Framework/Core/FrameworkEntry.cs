using System.Collections.Generic;
using Framework.Kits.IOCKits;

namespace Framework
{
	public static class FrameworkEntry
	{
		private static readonly NameIocContainer<IModule> _modules = new NameIocContainer<IModule>();
		private static bool hasShutdown;
		public static bool Contains<T>() where T : IModule {
			return _modules.Contains<T>();
		}
		public static void RegisterModule<T>(T instance, string name = null) where T : class, IModule {
			_modules.Register(instance, name);
			instance.Init();
		}
		public static T RegisterModule<T>(string name = null) where T : class, IModule, new() {
			_modules.Register(out T instance ,name);
			instance.Init();
			return instance;
		}
		public static T GetModule<T>(string name = null) where T : class, IModule {
			return _modules.Get<T>(name);
		}

		public static void Update(float elapseSeconds, float realElapseSeconds) {
			foreach (IModule module in _modules) {
				module.Update(elapseSeconds, realElapseSeconds);
			}
		}

		public static void Shutdown() {
			if (hasShutdown) return;
			foreach (IModule module in _modules) {
				module.Shutdown();
			}
			hasShutdown = true;
		}
	}
}