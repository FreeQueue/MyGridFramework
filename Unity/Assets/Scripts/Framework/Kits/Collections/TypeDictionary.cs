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

		public void Add(object val)
		{
			Type t = val.GetType();

			foreach (Type i in t.GetInterfaces())
				InnerAdd(i, val);
			foreach (Type tt in t.BaseTypes())
				InnerAdd(tt, val);
		}

		private void InnerAdd(Type t, object val)
		{
			_data.GetOrAdd(t, CreateList).Add(val);
		}

		public bool Contains<T>()
		{
			return _data.ContainsKey(typeof(T));
		}

		public bool Contains(Type t)
		{
			return _data.ContainsKey(t);
		}

		public T Get<T>()
		{
			return (T)Get(typeof(T), true);
		}

		public T GetOrDefault<T>()
		{
			var result = Get(typeof(T), false);
			if (result == null)
				return default;
			return (T)result;
		}

		private object Get(Type t, bool throwsIfMissing)
		{
			if (!_data.TryGetValue(t, out var ret))
			{
				if (throwsIfMissing)
					throw new InvalidOperationException($"TypeDictionary does not contain instance of type `{t}`");
				return null;
			}

			if (ret.Count > 1)
				throw new InvalidOperationException($"TypeDictionary contains multiple instances of type `{t}`");
			return ret[0];
		}

		public IEnumerable<T> WithInterface<T>()
		{
			return _data.TryGetValue(typeof(T), out var objects) ? objects.Cast<T>() : Array.Empty<T>();
		}

		public void Remove<T>(T val)
		{
			Type t = val.GetType();

			foreach (Type i in t.GetInterfaces())
				InnerRemove(i, val);
			foreach (Type tt in t.BaseTypes())
				InnerRemove(tt, val);
		}

		private void InnerRemove(Type t, object val)
		{
			if (!_data.TryGetValue(t, out var objects))
				return;
			objects.Remove(val);
			if (objects.Count == 0)
				_data.Remove(t);
		}

		public void TrimExcess()
		{
			foreach (var objects in _data.Values)
				objects.TrimExcess();
		}

		public IEnumerator GetEnumerator()
		{
			return WithInterface<object>().GetEnumerator();
		}
	}

	public static class TypeExts
	{
		public static IEnumerable<Type> BaseTypes(this Type t)
		{
			while (t != null)
			{
				yield return t;
				t = t.BaseType;
			}
		}
	}
}
