using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Kits.ReferencePoolKits;
using Framework.Extensions;

namespace Framework.Kits.EventKits
{
	public class EventPool<TEventArgs> : IFireEvent<TEventArgs>, IReference
	{
		public readonly Type handlerType = typeof(Action<TEventArgs>);

		protected readonly Queue<Event<TEventArgs>> events;
		protected readonly ConcurrentDictionary<int, HashSet<Action<TEventArgs>>> handlerSets;

		public EventPool() {
			events = new Queue<Event<TEventArgs>>();
			handlerSets = new ConcurrentDictionary<int, HashSet<Action<TEventArgs>>>();
		}

		#region DelegateEvent
		public int EventCount => events.Count;
		public int HandlerCount => handlerSets.Sum(pair => pair.Value.Count);

		public int EventHandlerCount(IConvertible id) {
			return handlerSets.TryGetValue(id.ToInt32(null), out HashSet<Action<TEventArgs>> handlerSet)
				? handlerSet.Count
				: 0;
		}
		public bool HasHandler(IConvertible id, Action<TEventArgs> handler) {
			return handlerSets.TryGetValue(id.ToInt32(null), out HashSet<Action<TEventArgs>> handlerSet) &&
					handlerSet.Contains(handler);
		}
		public void Update() {
			lock (events) {
				while (events.TryDequeue(out Event<TEventArgs> e)) {
					HandleEvent(e.Id, e.EventArgs);
					ReferencePool.Release(e);
				}
			}
		}
		public bool Subscribe<TId>(TId id, Action<TEventArgs> handler) where TId:IConvertible{
			return handlerSets.GetOrNew(id.ToInt32(null)).Add(handler);
		}
		public bool Unsubscribe<TId>(TId id, Action<TEventArgs> handler)where  TId:IConvertible{
			return handlerSets.TryGetValue(id.ToInt32(null), out HashSet<Action<TEventArgs>> handlerSet) &&
					handlerSet.Remove(handler);
		}
		public BitArray SubscribeMap<TId>((TId, Action<TEventArgs>)[] bindMap) where TId:IConvertible{
			var flags = new BitArray(bindMap.Length);
			int i = 0;
			foreach ((TId, Action<TEventArgs>) bind in bindMap) {
				flags[i++] = Subscribe(bind.Item1, bind.Item2);
			}
			return flags;
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
		protected void HandleEvent(int id, TEventArgs eventArgs) {
			if (!handlerSets.TryGetValue(id, out HashSet<Action<TEventArgs>> handlerSet)) return;
			foreach (Action<TEventArgs> handler in handlerSet) {
				handler(eventArgs);
			}
		}
		public void ClearHandlers() {
			foreach (var handlerSetPair in handlerSets) {
				handlerSetPair.Value.Clear();
			}
		}
		#endregion

		#region IFireEvent<TEventArgs>
		public virtual void Fire<TId>(TId id, TEventArgs eventArgs = default) where TId:IConvertible{
			events.Enqueue(Event<TEventArgs>.Create(id, eventArgs));
		}
		public virtual void FireNow<TId>(TId id, TEventArgs eventArgs = default) where TId:IConvertible{
			HandleEvent(id.ToInt32(null), eventArgs);
		}
		#endregion

		public virtual void Clear() {
			ClearHandlers();
			foreach (Event<TEventArgs> e in events) {
				ReferencePool.Release(e);
			}
			events.Clear();
		}
	}
}