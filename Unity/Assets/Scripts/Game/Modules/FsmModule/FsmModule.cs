using Framework;
using Framework.Kits.FsmKits;

namespace S100.Modules{
	public class FsmModule:FsmManager,IModule
	{
		void IModule.Update(float elapseSeconds, float realElapseSeconds) {
			Update(elapseSeconds,realElapseSeconds);
		}
		void IModule.Shutdown() {
			Shutdown();
		}
	}
}