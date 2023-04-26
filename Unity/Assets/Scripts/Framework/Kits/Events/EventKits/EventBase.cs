using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.EventKits
{
	public abstract class EventBase:IReference
	{
		public abstract int EventId { get; }
		public virtual void Clear()
		{
			//do nothing
		}
	}
}