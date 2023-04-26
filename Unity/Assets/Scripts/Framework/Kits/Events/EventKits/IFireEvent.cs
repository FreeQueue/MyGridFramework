using System;

namespace Framework.Kits.EventKits
{
	public interface IFireEvent<in TEventArgs>
	{
		public void Fire<TId>(TId id, TEventArgs eventArgs = default) where TId:IConvertible;
		public void FireNow<TId>(TId id, TEventArgs eventArgs = default)where TId:IConvertible;
	}
}