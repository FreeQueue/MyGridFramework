using Framework;
using Framework.Kits.EventKits;

namespace S100.Modules
{
	public class EventModule : EventPoolEx<object>, IModule
	{
		void IModule.Update(float elapseSeconds, float realElapseSeconds) {
			Update();
		}
		void IModule.Shutdown() {
			Clear();
		}
	}
}