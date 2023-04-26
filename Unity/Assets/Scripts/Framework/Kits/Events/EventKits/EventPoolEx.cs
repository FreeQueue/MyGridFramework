using System;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.EventKits
{
	public class EventPoolEx<TEventArgs> : EventPool<TEventArgs>, IReceiveEvent<TEventArgs>, ISendEvent<TEventArgs>
	{
		protected EventSource<TEventArgs> eventSource;
		protected IDisposable unsubscriber;

		public override void Fire<TId>(TId id, TEventArgs eventArgs = default) {
			base.Fire(id, eventArgs);
			eventSource?.Fire(id, eventArgs);
		}
		public override void FireNow<TId>(TId id, TEventArgs eventArgs = default) {
			base.FireNow(id, eventArgs);
			eventSource?.FireNow(id, eventArgs);
		}

		#region IReceiveEvent<TEventArgs>
		public void SubscribePublisher(ISendEvent<TEventArgs> sender) {
			UnsubscribePublisher();
			unsubscriber = sender.StartNotify(this);
		}
		public void UnsubscribePublisher() {
			unsubscriber?.Dispose();
			unsubscriber = null;
		}

		void IReceiveEvent<TEventArgs>.Receive(int id, TEventArgs eventArgs) {
			Fire(id, eventArgs);
		}
		void IReceiveEvent<TEventArgs>.ReceiveNow(int id, TEventArgs eventArgs) {
			FireNow(id, eventArgs);
		}
		#endregion

		#region IReceiveEvent<TEventArgs>
		public IDisposable StartNotify(IReceiveEvent<TEventArgs> receiver) {
			eventSource ??= new EventSource<TEventArgs>();
			return eventSource.StartNotify(receiver);
		}
		#endregion

		public override void Clear() {
			ReferencePool.Release(eventSource);
			eventSource = null;
			UnsubscribePublisher();
			base.Clear();
		}

	}
}