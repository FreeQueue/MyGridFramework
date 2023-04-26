using System;
using System.Collections.Generic;
using Framework;

namespace OpenRA.Traits
{
	[Flags]
	public enum SelectionPriorityModifiers
	{
		None = 0,
		Ctrl = 1,
		Alt = 2,
	}

	[RequireExplicitImpl]
	public interface ISelectableInfo : ITraitInfoInterface
	{
		int Priority { get; }
		SelectionPriorityModifiers PriorityModifiers { get; }
		string Voice { get; }
	}

	public interface ISelection
	{
		int Hash { get; }
		IEnumerable<Actor> Actors { get; }

		void Add(Actor a);
		void Remove(Actor a);
		bool Contains(Actor a);
		//TODO//void Combine(World world, IEnumerable<Actor> newSelection, bool isCombine, bool isClick);
		void Clear();
		bool RolloverContains(Actor a);
		void SetRollover(IEnumerable<Actor> actors);
	}

	[RequireExplicitImpl]
	public interface INotifySelected
	{
		void Selected(Actor self);
	}

	[RequireExplicitImpl]
	public interface INotifySelection
	{
		void SelectionChanged();
	}
}