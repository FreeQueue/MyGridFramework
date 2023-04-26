using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.EventKits
{
	public class Unsubscriber : IUnRegister, IReference,IDisposable
	{
		private object _observer;
		private IList _observers;
		public static Unsubscriber Create<T>(IList<T> observers, T observer) where T:class{
			var unsubscriber = ReferencePool.Get<Unsubscriber>();
			unsubscriber._observers = (IList)observers;
			unsubscriber._observer = observer;
			return unsubscriber;
		}
		public void UnRegister() {
			if (_observer != null && _observers.Contains(_observer)) {
				_observers.Remove(_observer);
			}
			ReferencePool.Release(this);
		}
		public void Clear() {
			_observers = null;
			_observer = null;
		}

		public void Dispose() {
			UnRegister();
		}
	}
}