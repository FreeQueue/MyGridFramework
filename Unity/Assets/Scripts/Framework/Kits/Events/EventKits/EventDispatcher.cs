using System;
using System.Collections.Generic;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.EventKits
{
	public class EventDispatcher<TEventArgs> : IReceiveEvent<TEventArgs>, ISendEvent<TEventArgs>,IReference
	{
		private readonly List<IReceiveEvent<TEventArgs>> _receivers;
		private IDisposable _unsubscriber;
		public EventDispatcher() {
			_receivers = new List<IReceiveEvent<TEventArgs>>();
		}

		void IReceiveEvent<TEventArgs>.Receive(int id,TEventArgs eventArgs) {
			foreach (var receiver in _receivers) {
				receiver.Receive(id,eventArgs);
			}
		}
		void IReceiveEvent<TEventArgs>.ReceiveNow(int id,TEventArgs eventArgs) {
			foreach (var receiver in _receivers) {
				receiver.Receive(id,eventArgs);
			}
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
		
		public void SubscribePublisher(ISendEvent<TEventArgs> sender) {
			UnsubscribePublisher();
			_unsubscriber = sender.StartNotify(this);
		}
		public void UnsubscribePublisher() {
			_unsubscriber?.Dispose();
			_unsubscriber = null;
		}
		public IDisposable StartNotify(IReceiveEvent<TEventArgs> receiver) {
			_receivers.Add(receiver);
			return Unsubscriber.Create(_receivers, receiver);
		}
		public void Clear() {
			UnsubscribePublisher();
			_receivers.Clear();
		}
	}
}