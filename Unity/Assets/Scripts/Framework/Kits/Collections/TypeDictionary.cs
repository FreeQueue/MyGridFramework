#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Extensions;

namespace Framework.Kits.CollectionsUnmanaged
{
	public class TypeDictionary : IEnumerable
	{
		private static readonly Func<Type, List<object>> CreateList = _ => new List<object>();
		private readonly Dictionary<Type, List<object>> _data = new Dictionary<Type, List<object>>();

		public IEnumerator GetEnumerator() => WithInterface<object>().GetEnumerator();

		public void Add(object val) {
			Type type = val.GetType();

			foreach (Type @interface in type.GetInterfaces()) {
				InnerAdd(@interface, val);
			}
			foreach (Type baseType in type.BaseTypes()) {
				InnerAdd(baseType, val);
			}
		}

		private void InnerAdd(Type t, object val) {
			_data.GetOrAdd(t, CreateList).Add(val);
		}
		public bool Contains<T>() => _data.ContainsKey(typeof(T));
		public bool Contains(Type t) => _data.ContainsKey(t);
		public T Get<T>() => (T)Get(typeof(T), true);

		public T GetOrDefault<T>() {
			object result = Get(typeof(T), false);
			if (result == null) return default(T);
			return (T)result;
		}

		private object Get(Type t, bool throwsIfMissing) {
			if (!_data.TryGetValue(t, out List<object> ret)) {
				if (throwsIfMissing)
					throw new InvalidOperationException($"TypeDictionary does not contain instance of type `{t}`");
				return null;
			}

			if (ret.Count > 1)
				throw new InvalidOperationException($"TypeDictionary contains multiple instances of type `{t}`");
			return ret[0];
		}

		public IEnumerable<T> WithInterface<T>() => _data.TryGetValue(typeof(T), out List<object> objects)
			? objects.Cast<T>()
			: Array.Empty<T>();

		public void Remove<T>(T val) {
			Type t = val.GetType();

			foreach (Type i in t.GetInterfaces()) {
				InnerRemove(i, val);
			}
			foreach (Type tt in t.BaseTypes()) {
				InnerRemove(tt, val);
			}
		}

		private void InnerRemove(Type t, object val) {
			if (!_data.TryGetValue(t, out List<object> objects)) return;
			objects.Remove(val);
			if (objects.Count == 0) _data.Remove(t);
		}

		public void TrimExcess() {
			foreach (List<object> list in _data.Values) {
				list.TrimExcess();
			}
		}
	}
}