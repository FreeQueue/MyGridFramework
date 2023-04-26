//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Framework.Kits.UGF_CollectionKits
{
    /// <summary>
    ///     游戏框架链表范围。
    /// </summary>
    /// <typeparam name="T">指定链表范围的元素类型。</typeparam>
    [StructLayout(LayoutKind.Auto)]
	public readonly struct LinkedListRange<T> : IEnumerable<T>, IEnumerable
	{
        /// <summary>
        ///     获取链表范围是否有效。
        /// </summary>
        public bool IsValid => First != null && Terminal != null && First != Terminal;

        /// <summary>
        ///     获取链表范围的开始结点。
        /// </summary>
        public LinkedListNode<T> First { get; }

        /// <summary>
        ///     获取链表范围的终结标记结点。
        /// </summary>
        public LinkedListNode<T> Terminal { get; }

        /// <summary>
        ///     获取链表范围的结点数量。
        /// </summary>
        public int Count {
			get {
				if (!IsValid) {
					return 0;
				}
				int count = 0;
				for (LinkedListNode<T> current = First;
					current != null && current != Terminal;
					current = current.Next) {
					count++;
				}
				return count;
			}
		}

        public LinkedListRange(LinkedListNode<T> first, LinkedListNode<T> terminal) {
			First = first;
			Terminal = terminal;
		}

        /// <summary>
        ///     检查是否包含指定值。
        /// </summary>
        /// <param name="value">要检查的值。</param>
        /// <returns>是否包含指定值。</returns>
        public bool Contains(T value) {
			for (LinkedListNode<T> current = First; current != null && current != Terminal; current = current.Next) {
				if (current.Value.Equals(value)) {
					return true;
				}
			}

			return false;
		}

        /// <summary>
        ///     返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        public Enumerator GetEnumerator() {
			return new Enumerator(this);
		}

        /// <summary>
        ///     返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
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
        ///     循环访问集合的枚举数。
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
		public struct Enumerator : IEnumerator<T>, IEnumerator
		{
			private readonly LinkedListRange<T> _LinkedListRange;
			private LinkedListNode<T> _Current;
			private T _CurrentValue;

			internal Enumerator(LinkedListRange<T> range) {
				if (!range.IsValid) {
					throw new InvalidDataException("Range is invalid.");
				}

				_LinkedListRange = range;
				_Current = _LinkedListRange.First;
				_CurrentValue = default;
			}

            /// <summary>
            ///     获取当前结点。
            /// </summary>
            public T Current => _CurrentValue;

            /// <summary>
            ///     获取当前的枚举数。
            /// </summary>
            object IEnumerator.Current => _CurrentValue;

            /// <summary>
            ///     清理枚举数。
            /// </summary>
            public void Dispose() {
			}

            /// <summary>
            ///     获取下一个结点。
            /// </summary>
            /// <returns>返回下一个结点。</returns>
            public bool MoveNext() {
				if (_Current == null || _Current == _LinkedListRange.Terminal) {
					return false;
				}

				_CurrentValue = _Current.Value;
				_Current = _Current.Next;
				return true;
			}

            /// <summary>
            ///     重置枚举数。
            /// </summary>
            void IEnumerator.Reset() {
				_Current = _LinkedListRange.First;
				_CurrentValue = default;
			}
		}
	}
}