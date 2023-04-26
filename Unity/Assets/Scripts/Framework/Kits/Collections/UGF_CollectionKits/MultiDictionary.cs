//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Framework.Kits.UGF_CollectionKits
{
    /// <summary>
    ///     游戏框架多值字典类。
    /// </summary>
    /// <typeparam name="TKey">指定多值字典的主键类型。</typeparam>
    /// <typeparam name="TValue">指定多值字典的值类型。</typeparam>
    public sealed class MultiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, LinkedListRange<TValue>>>,
		IEnumerable
	{
		private readonly Dictionary<TKey, LinkedListRange<TValue>> _Dictionary;
		private readonly LinkedList<TValue> _LinkedList;

        /// <summary>
        ///     初始化游戏框架多值字典类的新实例。
        /// </summary>
        public MultiDictionary() {
			_LinkedList = new LinkedList<TValue>();
			_Dictionary = new Dictionary<TKey, LinkedListRange<TValue>>();
		}

        /// <summary>
        ///     获取多值字典中实际包含的主键数量。
        /// </summary>
        public int Count => _Dictionary.Count;

        /// <summary>
        ///     获取多值字典中指定主键的范围。
        /// </summary>
        /// <param name="key">指定的主键。</param>
        /// <returns>指定主键的范围。</returns>
        public LinkedListRange<TValue> this[TKey key] {
			get {
				LinkedListRange<TValue> range = default;
				_Dictionary.TryGetValue(key, out range);
				return range;
			}
		}

        /// <summary>
        ///     返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        IEnumerator<KeyValuePair<TKey, LinkedListRange<TValue>>>
			IEnumerable<KeyValuePair<TKey, LinkedListRange<TValue>>>.GetEnumerator() {
			return GetEnumerator();
		}

        /// <summary>
        ///     返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

        /// <summary>
        ///     清理多值字典。
        /// </summary>
        public void Clear() {
			_Dictionary.Clear();
			_LinkedList.Clear();
		}

        /// <summary>
        ///     检查多值字典中是否包含指定主键。
        /// </summary>
        /// <param name="key">要检查的主键。</param>
        /// <returns>多值字典中是否包含指定主键。</returns>
        public bool Contains(TKey key) {
			return _Dictionary.ContainsKey(key);
		}

        /// <summary>
        ///     检查多值字典中是否包含指定值。
        /// </summary>
        /// <param name="key">要检查的主键。</param>
        /// <param name="value">要检查的值。</param>
        /// <returns>多值字典中是否包含指定值。</returns>
        public bool Contains(TKey key, TValue value) {
			LinkedListRange<TValue> range = default;
			if (_Dictionary.TryGetValue(key, out range)) {
				return range.Contains(value);
			}

			return false;
		}

        /// <summary>
        ///     尝试获取多值字典中指定主键的范围。
        /// </summary>
        /// <param name="key">指定的主键。</param>
        /// <param name="range">指定主键的范围。</param>
        /// <returns>是否获取成功。</returns>
        public bool TryGetValue(TKey key, out LinkedListRange<TValue> range) {
			return _Dictionary.TryGetValue(key, out range);
		}

        /// <summary>
        ///     向指定的主键增加指定的值。
        /// </summary>
        /// <param name="key">指定的主键。</param>
        /// <param name="value">指定的值。</param>
        public void Add(TKey key, TValue value) {
			LinkedListRange<TValue> range = default;
			if (_Dictionary.TryGetValue(key, out range)) {
				_LinkedList.AddBefore(range.Terminal, value);
			}
			else {
				LinkedListNode<TValue> first = _LinkedList.AddLast(value);
				LinkedListNode<TValue> terminal = _LinkedList.AddLast(default(TValue));
				_Dictionary.Add(key, new LinkedListRange<TValue>(first, terminal));
			}
		}

        /// <summary>
        ///     从指定的主键中移除指定的值。
        /// </summary>
        /// <param name="key">指定的主键。</param>
        /// <param name="value">指定的值。</param>
        /// <returns>是否移除成功。</returns>
        public bool Remove(TKey key, TValue value) {
			LinkedListRange<TValue> range = default;
			if (_Dictionary.TryGetValue(key, out range)) {
				for (LinkedListNode<TValue> current = range.First;
					current != null && current != range.Terminal;
					current = current.Next) {
					if (current.Value.Equals(value)) {
						if (current == range.First) {
							LinkedListNode<TValue> next = current.Next;
							if (next == range.Terminal) {
								_LinkedList.Remove(next);
								_Dictionary.Remove(key);
							}
							else {
								_Dictionary[key] = new LinkedListRange<TValue>(next, range.Terminal);
							}
						}

						_LinkedList.Remove(current);
						return true;
					}
				}
			}

			return false;
		}

        /// <summary>
        ///     从指定的主键中移除所有的值。
        /// </summary>
        /// <param name="key">指定的主键。</param>
        /// <returns>是否移除成功。</returns>
        public bool RemoveAll(TKey key) {
			LinkedListRange<TValue> range = default;
			if (_Dictionary.TryGetValue(key, out range)) {
				_Dictionary.Remove(key);

				LinkedListNode<TValue> current = range.First;
				while (current != null) {
					LinkedListNode<TValue> next = current != range.Terminal ? current.Next : null;
					_LinkedList.Remove(current);
					current = next;
				}

				return true;
			}

			return false;
		}

        /// <summary>
        ///     返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        public Enumerator GetEnumerator() {
			return new Enumerator(_Dictionary);
		}

        /// <summary>
        ///     循环访问集合的枚举数。
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
		public struct Enumerator : IEnumerator<KeyValuePair<TKey, LinkedListRange<TValue>>>, IEnumerator
		{
			private Dictionary<TKey, LinkedListRange<TValue>>.Enumerator _enumerator;

			internal Enumerator(Dictionary<TKey, LinkedListRange<TValue>> dictionary) {
				_enumerator = dictionary.GetEnumerator();
			}

            /// <summary>
            ///     获取当前结点。
            /// </summary>
            public KeyValuePair<TKey, LinkedListRange<TValue>> Current => _enumerator.Current;

            /// <summary>
            ///     获取当前的枚举数。
            /// </summary>
            object IEnumerator.Current => _enumerator.Current;

            /// <summary>
            ///     清理枚举数。
            /// </summary>
            public void Dispose() {
				_enumerator.Dispose();
			}

            /// <summary>
            ///     获取下一个结点。
            /// </summary>
            /// <returns>返回下一个结点。</returns>
            public bool MoveNext() {
				return _enumerator.MoveNext();
			}

            /// <summary>
            ///     重置枚举数。
            /// </summary>
            void IEnumerator.Reset() {
				((IEnumerator<KeyValuePair<TKey, LinkedListRange<TValue>>>)_enumerator).Reset();
			}
		}
	}
}