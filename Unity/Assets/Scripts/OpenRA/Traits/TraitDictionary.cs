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
using System.Text;
using Framework.Extensions;
using Framework.Kits.CollectionsUnmanaged;
using UnityEngine;

namespace OpenRA.Traits
{
	internal static class ListExts
	{
		public static int BinarySearchMany(this List<Actor> list, uint searchFor) {
			int start = 0;
			int end = list.Count;
			while (start != end) {
				int mid = (start + end) / 2;
				if (list[mid].ActorID < searchFor) {
					start = mid + 1;
				}
				else {
					end = mid;
				}
			}

			return start;
		}
	}

	/// <summary>
	///     Provides efficient ways to query a set of actors by their traits.
	/// </summary>
	internal class TraitDictionary
	{
		private static readonly Func<Type, ITraitContainer> CreateTraitContainer = t =>
			(ITraitContainer)typeof(TraitContainer<>).MakeGenericType(t).GetConstructor(Type.EmptyTypes).Invoke(null);

		private readonly Dictionary<Type, ITraitContainer> _traits = new Dictionary<Type, ITraitContainer>();

		private ITraitContainer InnerGet(Type t) {
			return _traits.GetOrAdd(t, CreateTraitContainer);
		}

		private TraitContainer<T> InnerGet<T>() {
			return (TraitContainer<T>)InnerGet(typeof(T));
		}

		public void PrintReport() {
			var stringBuilder = new StringBuilder("Traits:\n");
			foreach (KeyValuePair<Type, ITraitContainer> t in _traits.OrderByDescending(t => t.Value.Queries)
						.TakeWhile(t => t.Value.Queries > 0)) {
				stringBuilder.AppendFormat("{0}: {1}\n", t.Key.Name, t.Value.Queries);
			}
			Debug.Log(stringBuilder.ToString());
		}

		public void AddTrait(Actor actor, object val) {
			Type t = val.GetType();

			foreach (Type i in t.GetInterfaces()) {
				InnerAdd(actor, i, val);
			}
			foreach (Type tt in t.BaseTypes()) {
				InnerAdd(actor, tt, val);
			}
		}

		private void InnerAdd(Actor actor, Type t, object val) {
			InnerGet(t).Add(actor, val);
		}

		private static void CheckDestroyed(Actor actor) {
			if (actor.Disposed) {
				throw new InvalidOperationException($"Attempted to get trait from destroyed object ({actor})");
			}
		}

		public T Get<T>(Actor actor) {
			CheckDestroyed(actor);
			return InnerGet<T>().Get(actor);
		}

		public T GetOrDefault<T>(Actor actor) {
			CheckDestroyed(actor);
			return InnerGet<T>().GetOrDefault(actor);
		}

		public IEnumerable<T> WithInterface<T>(Actor actor) {
			CheckDestroyed(actor);
			return InnerGet<T>().GetMultiple(actor.ActorID);
		}

		public IEnumerable<TraitPair<T>> ActorsWithTrait<T>() {
			return InnerGet<T>().All();
		}

		public IEnumerable<Actor> ActorsHavingTrait<T>() {
			return InnerGet<T>().Actors();
		}

		public IEnumerable<Actor> ActorsHavingTrait<T>(Func<T, bool> predicate) {
			return InnerGet<T>().Actors(predicate);
		}

		public void RemoveActor(Actor a) {
			foreach (KeyValuePair<Type, ITraitContainer> t in _traits) {
				t.Value.RemoveActor(a.ActorID);
			}
		}

		public void ApplyToActorsWithTrait<T>(Action<Actor, T> action) {
			InnerGet<T>().ApplyToAll(action);
		}

		public void ApplyToActorsWithTraitTimed<T>(Action<Actor, T> action, string text) {
			InnerGet<T>().ApplyToAllTimed(action, text);
		}

		private interface ITraitContainer
		{
			int Queries { get; }
			void Add(Actor actor, object trait);
			void RemoveActor(uint actor);
		}

		private class TraitContainer<T> : ITraitContainer
		{
			private readonly List<Actor> _actors = new List<Actor>();
			private readonly List<T> _traits = new List<T>();
			//private readonly PerfTickLogger perfLogger = new PerfTickLogger();

			public int Queries { get; private set; }

			public void Add(Actor actor, object trait) {
				int insertIndex = _actors.BinarySearchMany(actor.ActorID + 1);
				_actors.Insert(insertIndex, actor);
				_traits.Insert(insertIndex, (T)trait);
			}

			public void RemoveActor(uint actor) {
				int startIndex = _actors.BinarySearchMany(actor);
				if (startIndex >= _actors.Count || _actors[startIndex].ActorID != actor) {
					return;
				}

				int endIndex = startIndex + 1;
				while (endIndex < _actors.Count && _actors[endIndex].ActorID == actor) {
					endIndex++;
				}

				int count = endIndex - startIndex;
				_actors.RemoveRange(startIndex, count);
				_traits.RemoveRange(startIndex, count);
			}

			public T Get(Actor actor) {
				T result = GetOrDefault(actor);
				if (result == null) {
					throw new InvalidOperationException(
						$"Actor {actor.Info.Name} does not have trait of type `{typeof(T)}`");
				}

				return result;
			}

			public T GetOrDefault(Actor actor) {
				++Queries;
				int index = _actors.BinarySearchMany(actor.ActorID);
				if (index >= _actors.Count || _actors[index] != actor) {
					return default;
				}

				if (index + 1 < _actors.Count && _actors[index + 1] == actor) {
					throw new InvalidOperationException(
						$"Actor {actor.Info.Name} has multiple traits of type `{typeof(T)}`");
				}

				return _traits[index];
			}

			public IEnumerable<T> GetMultiple(uint actor) {
				// PERF: Custom enumerator for efficiency - using `yield` is slower.
				++Queries;
				return new MultipleEnumerable(this, actor);
			}

			public IEnumerable<TraitPair<T>> All() {
				// PERF: Custom enumerator for efficiency - using `yield` is slower.
				++Queries;
				return new AllEnumerable(this);
			}

			public IEnumerable<Actor> Actors() {
				++Queries;
				Actor last = null;
				for (int i = 0; i < _actors.Count; i++) {
					Actor current = _actors[i];
					if (current == last) {
						continue;
					}

					yield return current;
					last = current;
				}
			}

			public IEnumerable<Actor> Actors(Func<T, bool> predicate) {
				++Queries;
				Actor last = null;

				for (int i = 0; i < _actors.Count; i++) {
					Actor current = _actors[i];
					if (current == last || !predicate(_traits[i])) {
						continue;
					}

					yield return current;
					last = current;
				}
			}

			public void ApplyToAll(Action<Actor, T> action) {
				for (int i = 0; i < _actors.Count; i++) {
					action(_actors[i], _traits[i]);
				}
			}

			public void ApplyToAllTimed(Action<Actor, T> action, string text) {
				//perfLogger.Start();
				for (int i = 0; i < _actors.Count; i++) {
					Actor actor = _actors[i];
					T trait = _traits[i];
					action(actor, trait);

					//perfLogger.LogTickAndRestartTimer(text, trait);
				}
			}

			private class MultipleEnumerable : IEnumerable<T>
			{
				private readonly uint _actor;
				private readonly TraitContainer<T> _container;
				public MultipleEnumerable(TraitContainer<T> container, uint actor) {
					_container = container;
					_actor = actor;
				}
				public IEnumerator<T> GetEnumerator() {
					return new MultipleEnumerator(_container, _actor);
				}
				IEnumerator IEnumerable.GetEnumerator() {
					return GetEnumerator();
				}
			}

			private struct MultipleEnumerator : IEnumerator<T>
			{
				private readonly List<Actor> _actors;
				private readonly List<T> _traits;
				private readonly uint _actor;
				private int _index;
				public MultipleEnumerator(TraitContainer<T> container, uint actor)
					: this() {
					_actors = container._actors;
					_traits = container._traits;
					_actor = actor;
					Reset();
				}

				public void Reset() {
					_index = _actors.BinarySearchMany(_actor) - 1;
				}
				public bool MoveNext() {
					return ++_index < _actors.Count && _actors[_index].ActorID == _actor;
				}
				public T Current => _traits[_index];
				object IEnumerator.Current => Current;
				public void Dispose() {
				}
			}

			private readonly struct AllEnumerable : IEnumerable<TraitPair<T>>
			{
				private readonly TraitContainer<T> _container;
				public AllEnumerable(TraitContainer<T> container) {
					_container = container;
				}
				public IEnumerator<TraitPair<T>> GetEnumerator() {
					return new AllEnumerator(_container);
				}
				IEnumerator IEnumerable.GetEnumerator() {
					return GetEnumerator();
				}
			}

			private struct AllEnumerator : IEnumerator<TraitPair<T>>
			{
				private readonly List<Actor> _actors;
				private readonly List<T> _traits;
				private int _index;
				public AllEnumerator(TraitContainer<T> container)
					: this() {
					_actors = container._actors;
					_traits = container._traits;
					Reset();
				}

				public void Reset() {
					_index = -1;
				}
				public bool MoveNext() {
					return ++_index < _actors.Count;
				}
				public TraitPair<T> Current => new TraitPair<T>(_actors[_index], _traits[_index]);
				object IEnumerator.Current => Current;
				public void Dispose() {
				}
			}
		}
	}
}