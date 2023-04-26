using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenRA.Traits
{
	public enum SubCell : byte
	{
		Invalid = byte.MaxValue,
		Any = byte.MaxValue - 1,
		FullCell = 0,
		First = 1,
	}

	public interface IOccupySpaceInfo : ITraitInfoInterface
	{
		bool SharesCell { get; }
		IReadOnlyDictionary<Vector2Int, SubCell> OccupiedCells(
			ActorInfo info, Vector2Int location, SubCell subCell = SubCell.Any
		);
	}

	public interface IOccupySpace
	{
		Vector2 CenterPosition { get; }
		Vector2Int TopLeft { get; }
		(Vector2Int cell, SubCell subCell)[] OccupiedCells();
	}


	public interface IActorMap
	{
		int LargestActorRadius { get; }
		int LargestBlockingActorRadius { get; }

		IEnumerable<Actor> GetActorsAt(Vector2Int a);
		IEnumerable<Actor> GetActorsAt(Vector2Int a, SubCell sub);
		bool HasFreeSubCell(Vector2Int cell, bool checkTransient = true);
		SubCell FreeSubCell(Vector2Int cell, SubCell preferredSubCell = SubCell.Any, bool checkTransient = true);
		SubCell FreeSubCell(Vector2Int cell, SubCell preferredSubCell, Func<Actor, bool> checkIfBlocker);
		bool AnyActorsAt(Vector2Int a);
		bool AnyActorsAt(Vector2Int a, SubCell sub, bool checkTransient = true);
		bool AnyActorsAt(Vector2Int a, SubCell sub, Func<Actor, bool> withCondition);
		IEnumerable<Actor> AllActors();
		void AddInfluence(Actor self, IOccupySpace ios);
		void RemoveInfluence(Actor self, IOccupySpace ios);
		IEnumerable<Vector2Int> TriggerPositions();
		int AddCellTrigger(Vector2Int[] cells, Action<Actor> onEntry, Action<Actor> onExit);
		void RemoveCellTrigger(int id);
		int AddProximityTrigger(Vector2Int pos, int range, int vRange, Action<Actor> onEntry, Action<Actor> onExit);
		void RemoveProximityTrigger(int id);
		void UpdateProximityTrigger(int id, Vector2Int newPos, int newRange, int newVRange);
		void AddPosition(Actor a, IOccupySpace ios);
		void RemovePosition(Actor a, IOccupySpace ios);
		void UpdatePosition(Actor a, IOccupySpace ios);
		IEnumerable<Actor> ActorsInBox(Vector2Int a, Vector2Int b);

		void UpdateOccupiedCells(IOccupySpace ios);
		event Action<Vector2Int> CellUpdated;
	}
}