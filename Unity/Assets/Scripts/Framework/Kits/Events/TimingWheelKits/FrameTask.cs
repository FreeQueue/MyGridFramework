using System;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.TimerWheelKits
{
	public class FrameTask : IReference
	{
		public int FrameCount { get; set; }
		public Action CallBack { get; set; }

		public void Clear() {
			FrameCount = default;
			CallBack = default;
		}

		public static FrameTask Create(int frameCount, Action callback) {
			var loomTask = ReferencePool.Get<FrameTask>();
			loomTask.FrameCount = frameCount;
			loomTask.CallBack = callback;
			return loomTask;
		}
	}
}