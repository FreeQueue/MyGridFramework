using Framework;
using Framework.Kits.TimerWheelKits;
using Helper=S100.Modules.TimerModuleHelper;
namespace S100.Modules
{
	public class TimerModule:TimingWheelManager,IModule
	{
		void IModule.Init() {
			Init(TimerModuleHelper.Instance.slotCount,TimerModuleHelper.Instance.tickSpan);
			StartTimer();
		}
		void IModule.Update(float elapseSeconds, float realElapseSeconds) {
			Update();
		}
		void IModule.Shutdown() {
			StopTimer();
			Clear();
		}
	}
}