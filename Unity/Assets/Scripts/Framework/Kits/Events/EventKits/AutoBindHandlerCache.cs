using System;
using System.Collections.Generic;
using System.Reflection;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.EventKits
{
	public class AutoBindHandlerCache<TEventArgs> : IReference
	{
		public List<(int, Action<TEventArgs>)> HandlerMap { get; } = new List<(int, Action<TEventArgs>)>();
		public Type HandlerType => typeof(Action<TEventArgs>);
		public void Register(Type staticTarget, bool clean = true) {
			if (clean) Clear();
			var methods = staticTarget.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			foreach (MethodInfo method in methods) {
				var attribute = Attribute.GetCustomAttribute(method, typeof(AutoBindEventAttribute));
				if (attribute is not AutoBindEventAttribute autoBindEventAttribute) continue;
				HandlerMap.Add((autoBindEventAttribute.EventId,
					(Action<TEventArgs>)method.CreateDelegate(HandlerType)));
			}
		}
		public void Register(object target, bool clean = true) {
			if (clean) Clear();
			var methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
													BindingFlags.Instance | BindingFlags.Static);
			foreach (MethodInfo method in methods) {
				var attribute = Attribute.GetCustomAttribute(method, typeof(AutoBindEventAttribute));
				if (attribute is not AutoBindEventAttribute autoBindEventAttribute) continue;
				Delegate action = method.IsStatic
					? method.CreateDelegate(HandlerType)
					: method.CreateDelegate(HandlerType, target);
				HandlerMap.Add((autoBindEventAttribute.EventId,
					(Action<TEventArgs>)method.CreateDelegate(HandlerType)));
			}
		}
		public void Clear() {
			HandlerMap.Clear();
		}
	}
}