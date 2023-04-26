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
using System.Collections.Generic;

namespace Framework.Kits.UGF_CollectionKits
{
	public interface IPriorityQueue<T>
	{
		void Add(T item);
		bool Empty { get; }
		T Peek();
		T Pop();
	}

	/// <summary>
	/// Represents a collection of items that have a priority.
	/// On pop, the item with the lowest priority value is removed.
	/// </summary>
	public sealed class PriorityQueue<T, TComparer> : IPriorityQueue<T> where TComparer : struct, IComparer<T>
	{
		/// <summary>
		/// Compares two items to determine their priority.
		/// PERF: Using a struct allows the calls to be devirtualized.
		/// </summary>
		private readonly TComparer _comparer;

		/// <summary>
		/// A <a href="https://en.wikipedia.org/wiki/Binary_heap">binary min-heap</a> storing the items.
		/// An array divided into sub arrays called levels. At each level the size of a level array doubles.
		/// Elements at deeper levels always have higher priority values than elements nearer to the root.
		/// </summary>
		private T[] _items;

		/// <summary>
		/// Index of deepest level.
		/// </summary>
		private int _level;

		/// <summary>
		/// Number of elements in the deepest level.
		/// </summary>
		private int _index;

		public PriorityQueue(TComparer comparer)
		{
			this._comparer = comparer;
			_items = new T[1];
		}

		public void Add(T item)
		{
			var addLevel = _level;
			var addIndex = _index;

			while (addLevel >= 1)
			{
				T above = _items[AboveIndex(addLevel, addIndex)];
				if (_comparer.Compare(above, item) > 0)
				{
					_items[Index(addLevel, addIndex)] = above;
					--addLevel;
					addIndex >>= 1;
				}
				else
					break;
			}

			_items[Index(addLevel, addIndex)] = item;

			if (++_index >= 1 << _level)
			{
				_index = 0;
				var count = 2 * (1 << ++_level);
				if (count - 1 >= _items.Length)
					Array.Resize(ref _items, count);
			}
		}

		public bool Empty => _level == 0;

		private static int Index(int level, int index) { return (1 << level) - 1 + index; }

		private static int AboveIndex(int level, int index) { return (1 << (level - 1)) - 1 + (index >> 1); }

		private int IndexLast()
		{
			var lastLevel = _level;
			var lastIndex = _index;

			if (--lastIndex < 0)
				lastIndex = (1 << --lastLevel) - 1;

			return Index(lastLevel, lastIndex);
		}

		public T Peek()
		{
			if (_level <= 0 && _index <= 0)
				throw new InvalidOperationException("PriorityQueue empty.");

			return _items[Index(0, 0)];
		}

		public T Pop()
		{
			T ret = Peek();
			BubbleInto(0, 0, _items[IndexLast()]);
			if (--_index < 0)
				_index = (1 << --_level) - 1;
			return ret;
		}

		private void BubbleInto(int intoLevel, int intoIndex, T val)
		{
			while (true)
			{
				var downLevel = intoLevel + 1;
				var downIndex = intoIndex << 1;

				if (downLevel > _level || (downLevel == _level && downIndex >= _index))
				{
					_items[Index(intoLevel, intoIndex)] = val;
					return;
				}

				T down = _items[Index(downLevel, downIndex)];
				if (downLevel < _level || (downLevel == _level && downIndex < _index - 1))
				{
					T downRight = _items[Index(downLevel, downIndex + 1)];
					if (_comparer.Compare(down, downRight) >= 0)
					{
						down = downRight;
						++downIndex;
					}
				}

				if (_comparer.Compare(val, down) <= 0)
				{
					_items[Index(intoLevel, intoIndex)] = val;
					return;
				}

				_items[Index(intoLevel, intoIndex)] = down;
				intoLevel = downLevel;
				intoIndex = downIndex;
			}
		}
	}
}
