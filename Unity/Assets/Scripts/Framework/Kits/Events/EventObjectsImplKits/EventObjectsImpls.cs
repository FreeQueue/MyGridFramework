using System;
using Framework.Kits.EventKits;

namespace Framework.Kits.EventObjectsImplKits
{
	public class EventSource : EventSource<object[]>
	{
		public new void Fire<TId>(TId id,params object[] eventArgs) where TId : IConvertible {
			base.Fire(id,eventArgs);
		}
		public new void FireNow<TId>(TId id,params object[] eventArgs) where TId : IConvertible {
			base.FireNow(id,eventArgs);
		}
	}

	public class EventDispatcher : EventDispatcher<object[]>
	{
		public new void Fire<TId>(TId id,params object[] eventArgs) where TId : IConvertible {
			base.Fire(id,eventArgs);
		}
		public new void FireNow<TId>(TId id,params object[] eventArgs) where TId : IConvertible {
			base.FireNow(id,eventArgs);
		}
	}
	public class EventPool : EventPool<object[]>
	{
		public new void Fire<TId>(TId id,params object[] eventArgs) where TId : IConvertible {
			base.Fire(id,eventArgs);
		}
		public new void FireNow<TId>(TId id,params object[] eventArgs) where TId : IConvertible {
			base.FireNow(id,eventArgs);
		}
	}
	public class EventPoolEx : EventPoolEx<object[]>
	{
		public new void Fire<TId>(TId id,params object[] eventArgs) where TId : IConvertible {
			base.Fire(id,eventArgs);
		}
		public new void FireNow<TId>(TId id,params object[] eventArgs) where TId : IConvertible {
			base.FireNow(id,eventArgs);
		}
	}

	public class EventHandlerSet : EventHandlerSet<object[]>
	{
		
	}
	
	public class AutoBindEventCache : AutoBindHandlerCache<object[]>
	{
		
	}
}