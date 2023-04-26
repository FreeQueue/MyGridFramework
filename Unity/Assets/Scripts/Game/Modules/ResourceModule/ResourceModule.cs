using Cysharp.Threading.Tasks;
using Framework;
using Framework.Kits.ResourceKits;

namespace S100.Modules
{
	public class ResourceModule : IModule
	{
		void IModule.Init() {
			ResourceManager.mode = ResourceModuleHelper.Instance.mode;
			ResourceManager.InitWithDefaultPackage(ResourceModuleHelper.Instance.defaultPackageName).Forget();
		}
		void IModule.Shutdown() {
			ResourceManager.Shutdown();
		}
	}
}