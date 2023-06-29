using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenRA.Traits
{
	internal partial class TraitDictionary
	{
		private class TraitContainer<T> : ITraitContainer
		{
			private readonly List<Actor> _actors = new List<Actor>();
			private readonly List<T> _traits = new List<T>();
			//private readonly PerfTickLogger perfLogger = new PerfTickLogger();

			public int Queries { get; private set; }

			public void Add(Actor actor, object trait) {
				int insertIndex = ListExts.BinarySearchMany(_actors, actor.ActorID + 1);
				_actors.Insert(insertIndex, actor);
				_traits.Insert(insertIndex, (T)trait);
			}

			public void RemoveActor(uint actor) {
				int startIndex = ListExts.BinarySearchMany(_actors, actor);
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
				int index = ListExts.BinarySearchMany(_actors, actor.ActorID);
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