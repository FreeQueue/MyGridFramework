using Framework;
using Framework.Kits.ObjectPoolKits;

namespace S100.Modules
{
	public class ObjectPoolModule: ObjectPoolManager,IModule
	{
		void IModule.Update(float elapseSeconds, float realElapseSeconds) {
			Update(realElapseSeconds);
		}
		void IModule.Shutdown() {
			Clear();
		}
	}
}