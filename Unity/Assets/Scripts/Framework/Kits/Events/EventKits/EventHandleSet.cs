using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Kits.ReferencePoolKits;
using Framework.Extensions;

namespace Framework.Kits.EventKits
{
	public class EventHandlerSet<TEventArgs> : IReceiveEvent<TEventArgs>, IReference
	{
		public readonly Type handlerType = typeof(Action<TEventArgs>);
		protected readonly Dictionary<int, Action<TEventArgs>> handlers;
		protected IDisposable unsubscriber;
		public int HandlerCount => handlers.Values.Sum(value => value != null ? 1 : 0);

		public EventHandlerSet() {
			handlers = new Dictionary<int, Action<TEventArgs>>();
		}

		public void Subscribe<TId>(TId id, Action<TEventArgs> handler) where TId : IConvertible {
			handlers.Add(id.ToInt32(null), handler);
		}
		public bool Unsubscribe<TId>(TId id) where TId : IConvertible {
			return handlers.Remove(id.ToInt32(null));
		}
		public void SubscribeMap<TId>(IEnumerable<(TId, Action<TEventArgs>)> bindMap) where TId : IConvertible {
			foreach ((TId, Action<TEventArgs>) bind in bindMap) {
				Subscribe(bind.Item1, bind.Item2);
			}
		}
		public void SubscribeAutoBindEvent(Type staticTarget) {
			var methods = staticTarget.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			foreach (MethodInfo method in methods) {
				var attribute
					= (AutoBindEventAttribute)Attribute.GetCustomAttribute(method, typeof(AutoBindEventAttribute));
				if (attribute != null) {
					Subscribe(attribute.EventId, (Action<TEventArgs>)method.CreateDelegate(handlerType));
				}
			}
		}
		public void SubscribeAutoBindEvent(object target) {
			var methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
													BindingFlags.Instance | BindingFlags.Static);
			foreach (MethodInfo method in methods) {
				var attribute
					= (AutoBindEventAttribute)Attribute.GetCustomAttribute(method, typeof(AutoBindEventAttribute));
				if (attribute == null) continue;
				Delegate action = method.IsStatic
					? method.CreateDelegate(handlerType)
					: method.CreateDelegate(handlerType, target);
				Subscribe(attribute.EventId, (Action<TEventArgs>)action);
			}
		}
		protected  void HandleEvent(int id, TEventArgs eventArgs) {
			handlers.Values.ForEach(handler => handler(eventArgs));
		}
		public void ClearHandlers() {
			handlers.Clear();
		}
		public void SubscribePublisher(ISendEvent<TEventArgs> sender) {
			UnsubscribePublisher();
			unsubscriber = sender.StartNotify(this);
		}
		public void UnsubscribePublisher() {
			unsubscriber?.Dispose();
			unsubscriber = null;
		}
		void IReceiveEvent<TEventArgs>.Receive(int id, TEventArgs eventArgs) {
			HandleEvent(id, eventArgs);
		}
		void IReceiveEvent<TEventArgs>.ReceiveNow(int id, TEventArgs eventArgs) {
			HandleEvent(id, eventArgs);
		}
		public void Clear() {
			ClearHandlers();
			UnsubscribePublisher();
		}
	}
}