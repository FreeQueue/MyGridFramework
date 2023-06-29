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
	internal partial class TraitDictionary
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

	}
}