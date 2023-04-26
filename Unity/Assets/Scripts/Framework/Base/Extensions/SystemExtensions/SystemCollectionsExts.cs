using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Extensions
{
	public static class SystemCollectionsExts
	{
		public static T MinBy<T, TValue>(this IEnumerable<T> @this, Func<T, TValue> selector)
		{
			return CompareBy(@this, selector, 1, true);
		}

		public static T MaxBy<T, TValue>(this IEnumerable<T> @this, Func<T, TValue> selector)
		{
			return CompareBy(@this, selector, -1, true);
		}

		public static T MinByOrDefault<T, TValue>(this IEnumerable<T> @this, Func<T, TValue> selector)
		{
			return CompareBy(@this, selector, 1, false);
		}

		public static T MaxByOrDefault<T, TValue>(this IEnumerable<T> @this, Func<T, TValue> selector)
		{
			return CompareBy(@this, selector, -1, false);
		}

		public static T CompareBy<T, TValue>(
			IEnumerable<T> source, Func<T, TValue> selector, int modifier, bool throws
		)
		{
			Comparer<TValue> comparer = Comparer<TValue>.Default;
			using IEnumerator<T> enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext()) {
				if (throws) {
					throw new ArgumentException("Collection must not be empty.", nameof(source));
				}
				return default;
			}
			T element = enumerator.Current;
			TValue value = selector(element);
			while (enumerator.MoveNext()) {
				T nextElement = enumerator.Current;
				TValue nextValue = selector(nextElement);
				if (comparer.Compare(nextValue, value) * modifier >= 0) continue;
				element = nextElement;
				value = nextValue;
			}
			return element;
		}

		public static int IndexOf<T>(this T[] array, T value)
		{
			return Array.IndexOf(array, value);
		}
		public static string JoinWith<T>(this IEnumerable<T> @this, string j)
		{
			return string.Join(j, @this);
		}

		public static IEnumerable<T> Append<T>(this IEnumerable<T> @this, params T[] moreTs)
		{
			return @this.Concat(moreTs);
		}

		public static IEnumerable<T> Exclude<T>(this IEnumerable<T> @this, params T[] exclusions)
		{
			return @this.Except(exclusions);
		}
		public static TValue GetOrNew<TKey, TValue>(
			this IDictionary<TKey, TValue> @this, TKey key
		) where TValue : new()
		{
			return @this.GetOrAdd(key, new TValue());
		}

		public static TValue GetOrAdd<TKey, TValue>(
			this IDictionary<TKey, TValue> @this, TKey key, TValue defaultValue
		)
		{
			return @this.TryGetValue(key, out TValue value) ? value : @this[key] = defaultValue;
		}

		public static TValue GetOrAdd<TKey, TValue>(
			this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> createFn
		)
		{
			if (!@this.TryGetValue(key, out TValue ret)) {
				@this.Add(key, ret = createFn(key));
			}
			return ret;
		}

		public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
		{
			foreach (T item in @this) {
				action(item);
			}
		}

		public static void ForEachReverse<T>(this IEnumerable<T> @this, Action<T> action)
		{
			if (@this is IList<T> list) {
				for (int i = list.Count - 1; i >= 0; i++) {
					action(list[i]);
				}
				return;
			}
			foreach (T element in @this.Reverse()) {
				action(element);
			}
		}
		public static void ForEach<T>(this IEnumerable<T> @this, Action<int, T> action)
		{
			int i = 0;
			foreach (T element in @this) {
				action(i, element);
				i++;
			}
		}
		public static void ForEach<TKey, TValue>(
			this IEnumerable<KeyValuePair<TKey, TValue>> @this, Action<TKey, TValue> action
		)
		{
			IEnumerator<KeyValuePair<TKey, TValue>> enumerator = @this.GetEnumerator();
			while (enumerator.MoveNext()) {
				action(enumerator.Current.Key, enumerator.Current.Value);
			}
			enumerator.Dispose();
		}

		public static void AddRange<TKey, TValue>(
			this IDictionary<TKey, TValue> @this, IEnumerable<KeyValuePair<TKey, TValue>> collection,
			bool isOverride = false
		)
		{
			IEnumerator<KeyValuePair<TKey, TValue>> enumerator = collection.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<TKey, TValue> current = enumerator.Current;
				if (@this.ContainsKey(current.Key)) {
					if (isOverride) {
						@this[current.Key] = current.Value;
					}
					continue;
				}

				@this.Add(current.Key, current.Value);
			}
			enumerator.Dispose();
		}

		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> @this)
		{
			return new HashSet<T>(@this);
		}

		public static Dictionary<TKey, TSource> ToDictionaryWithConflictLog<TSource, TKey>(
			this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector,
			string debugName, Func<TKey, string> logKey, Func<TSource, string> logValue
		)
		{
			return ToDictionaryWithConflictLog(@this, keySelector, x => x, debugName, logKey, logValue);
		}

		public static Dictionary<TKey, TValue> ToDictionaryWithConflictLog<TSource, TKey, TValue>(
			this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector,
			string debugName, Func<TKey, string> logKey = null, Func<TValue, string> logValue = null
		)
		{
			// Fall back on ToString() if null functions are provided:
			logKey ??= s => s.ToString();
			logValue ??= s => s.ToString();

			// Try to build a dictionary and log all duplicates found (if any):
			Dictionary<TKey, List<string>> dupKeys = new Dictionary<TKey, List<string>>();
			int capacity = @this is ICollection<TSource> collection ? collection.Count : 0;
			Dictionary<TKey, TValue> d = new Dictionary<TKey, TValue>(capacity);
			foreach (TSource item in @this) {
				TKey key = keySelector(item);
				TValue element = elementSelector(item);

				// Discard elements with null keys
				if (!typeof(TKey).IsValueType && key == null) {
					continue;
				}

				// Check for a key conflict:
				if (d.TryAdd(key, element)) continue;
				if (!dupKeys.TryGetValue(key, out List<string> dupKeyMessages)) {
					// Log the initial conflicting value already inserted:
					dupKeyMessages = new List<string> {
						logValue(d[key]),
					};
					dupKeys.Add(key, dupKeyMessages);
				}

				// Log this conflicting value:
				dupKeyMessages.Add(logValue(element));
			}

			// If any duplicates were found, throw a descriptive error
			if (dupKeys.Count > 0) {
				string badKeysFormatted = string.Join(", ",
					dupKeys.Select(p => $"{logKey(p.Key)}: [{string.Join(",", p.Value)}]"));
				string msg = $"{debugName}, duplicate values found for the following keys: {badKeysFormatted}";
				throw new ArgumentException(msg);
			}

			// Return the dictionary we built:
			return d;
		}
	}
}