using System;

namespace OpenRA
{
	public readonly struct TraitPair<T> : IEquatable<TraitPair<T>>
	{
		public readonly Actor Actor;
		public readonly T Trait;

		public TraitPair(Actor actor, T trait) {
			Actor = actor;
			Trait = trait;
		}

		public static bool operator ==(TraitPair<T> me, TraitPair<T> other) {
			return ReferenceEquals(me.Actor, other.Actor) && Equals(me.Trait, other.Trait);
		}
		public static bool operator !=(TraitPair<T> me, TraitPair<T> other) {
			return !(me == other);
		}

		public override int GetHashCode() {
			return Actor.GetHashCode() ^ Trait.GetHashCode();
		}

		public bool Equals(TraitPair<T> other) {
			return this == other;
		}
		public override bool Equals(object obj) {
			return obj is TraitPair<T> pair && Equals(pair);
		}

		public override string ToString() {
			return Actor.Info.Name + "->" + Trait.GetType().Name;
		}
	}
}