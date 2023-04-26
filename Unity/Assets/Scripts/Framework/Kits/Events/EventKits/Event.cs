using System;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.EventKits
{
	public class Event<TEventArgs> : IReference
	{
		public int Id { get; private set; }
		public TEventArgs EventArgs { get; private set; }
		public void Clear() {
			Id = default;
			EventArgs = default;
		}

		public static Event<TEventArgs> Create<TId>(TId id, TEventArgs eventArgs = default) where TId : IConvertible {
			Event<TEventArgs> e = ReferencePool.Get<Event<TEventArgs>>();
			e.Id = id.ToInt32(null);
			e.EventArgs = eventArgs;
			return e;
		}
	}

}