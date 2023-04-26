using System;
using System.Collections.Generic;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.EventKits
{
	public class EventSource<TEventArgs> : IFireEvent<TEventArgs>,ISendEvent<TEventArgs>,IReference
	{
		private readonly List<IReceiveEvent<TEventArgs>> _receivers;
		public EventSource() {
			_receivers = new List<IReceiveEvent<TEventArgs>>();
		}
		public void Fire<TId>(TId id, TEventArgs eventArgs = default) where TId : IConvertible {
			foreach (IReceiveEvent<TEventArgs> receiver in _receivers) {
				receiver.Receive(id.ToInt32(null), eventArgs);
			}
		}
		public void FireNow<TId>(TId id, TEventArgs eventArgs = default) where TId : IConvertible {
			foreach (IReceiveEvent<TEventArgs> receiver in _receivers) {
				receiver.ReceiveNow(id.ToInt32(null), eventArgs);
			}
		}

		public IDisposable StartNotify(IReceiveEvent<TEventArgs> receiver) {
			_receivers.Add(receiver);
			return Unsubscriber.Create(_receivers, receiver);
		}
		public void Clear() {
			_receivers.Clear();
		}
	}
}