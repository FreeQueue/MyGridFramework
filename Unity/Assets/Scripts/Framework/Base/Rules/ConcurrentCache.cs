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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Framework
{
	public class ConcurrentCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		private readonly ConcurrentDictionary<TKey, TValue> _cache;
		private readonly Func<TKey, TValue> _loader;

		public ConcurrentCache(Func<TKey, TValue> loader, IEqualityComparer<TKey> c)
		{
			_loader = loader ?? throw new ArgumentNullException(nameof(loader));
			_cache = new ConcurrentDictionary<TKey, TValue>(c);
		}

		public ConcurrentCache(Func<TKey, TValue> loader)
			: this(loader, EqualityComparer<TKey>.Default) { }

		public TValue this[TKey key] => _cache.GetOrAdd(key, _loader);
		public IEnumerable<TKey> Keys => _cache.Keys;

		public IEnumerable<TValue> Values => _cache.Values;

		public bool ContainsKey(TKey key) { return _cache.ContainsKey(key); }
		public bool TryGetValue(TKey key, out TValue value) { return _cache.TryGetValue(key, out value); }
		public int Count => _cache.Count;
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return _cache.GetEnumerator(); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
