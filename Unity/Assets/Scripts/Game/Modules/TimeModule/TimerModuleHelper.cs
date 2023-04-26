using Framework;
using UnityEngine;

namespace S100.Modules
{
	public class TimerModuleHelper:ModuleHelper<TimerModuleHelper>
	{
		[Tooltip("时间槽数量")] public int slotCount = 100;
		[Tooltip("时间槽大小")] public int tickSpan = 100;
	}
}