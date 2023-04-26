using System;
using System.Collections.Generic;
using Framework.Extensions;

namespace Framework.Kits.UGF_CollectionKits
{
	public class SortedMultiDictionary<TKey, TValue> : SortedDictionary<TKey, List<TValue>>
	{
		private readonly List<TValue> _empty = new List<TValue>();

        /// <summary>
        ///     返回内部的list
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public new List<TValue> this[TKey t] {
			get {
				TryGetValue(t, out List<TValue> list);
				return list ?? _empty;
			}
		}

		public void Add(TKey key, TValue value) {
			List<TValue> list = this.GetOrNew(key);
			list.Add(value);
		}

		public bool Remove(TKey t, TValue k) {
			if (!TryGetValue(t, out List<TValue> list)) return false;
			if (!list.Remove(k)) {
				return false;
			}
			if (list.Count == 0) {
				Remove(t);
			}
			return true;
		}

        /// <summary>
        ///     不返回内部的list,copy一份出来
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public TValue[] GetAll(TKey t) {
			TryGetValue(t, out List<TValue> list);
			return list == null ? Array.Empty<TValue>() : list.ToArray();
		}

		public TValue GetOne(TKey t) {
			TryGetValue(t, out List<TValue> list);
			return list is { Count: > 0 } ? list[0] : default;
		}

		public bool Contains(TKey t, TValue k) {
			TryGetValue(t, out List<TValue> list);
			return list != null && list.Contains(k);
		}
	}
}