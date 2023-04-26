using System.Collections.Generic;
using Framework;
using Framework.Kits.BitSetKits;
using UnityEngine;

namespace OpenRA.Traits
{
	/// <summary>
	///     Indicates target types as defined on <see cref="ITargetable" /> are present in a <see cref="BitSet{T}" />.
	/// </summary>
	public sealed class TargetableType
	{
		private TargetableType() {
		}
	}

	public interface ITargetableInfo : ITraitInfoInterface
	{
		BitSet<TargetableType> GetTargetTypes();
	}

	public interface ITargetable
	{
		// Check IsTraitEnabled or !IsTraitDisabled first
		BitSet<TargetableType> TargetTypes { get; }
		bool RequiresForceFire { get; }
		bool TargetableBy(Actor self, Actor byActor);
	}

	[RequireExplicitImpl]
	public interface ITargetablePositions
	{
		IEnumerable<Vector2Int> TargetablePositions(Actor self);
	}
}