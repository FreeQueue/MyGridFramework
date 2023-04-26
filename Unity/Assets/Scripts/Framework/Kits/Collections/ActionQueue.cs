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

namespace Framework.Kits.CollectionsUnmanaged
{
	/// <summary>
	/// A thread-safe action queue, suitable for passing units of work between threads.
	/// </summary>
	public class ActionQueue
	{
		private readonly List<DelayedAction> _actions = new List<DelayedAction>();

		public void Add(Action a, long desiredTime)
		{
			if (a == null)
				throw new ArgumentNullException(nameof(a));

			lock (_actions)
			{
				var action = new DelayedAction(a, desiredTime);
				var index = Index(action);
				_actions.Insert(index, action);
			}
		}

		public void PerformActions(long currentTime)
		{
			DelayedAction[] pendingActions;
			lock (_actions)
			{
				var dummyAction = new DelayedAction(null, currentTime);
				var index = Index(dummyAction);
				if (index <= 0)
					return;

				pendingActions = new DelayedAction[index];
				_actions.CopyTo(0, pendingActions, 0, index);
				_actions.RemoveRange(0, index);
			}

			foreach (DelayedAction delayedAction in pendingActions)
				delayedAction.action();
		}

		private int Index(DelayedAction action)
		{
			// Returns the index of the next action with a strictly greater time.
			var index = _actions.BinarySearch(action);
			if (index < 0)
				return ~index;
			while (index < _actions.Count && action.CompareTo(_actions[index]) >= 0)
				index++;
			return index;
		}
	}

	internal readonly struct DelayedAction : IComparable<DelayedAction>
	{
		public readonly long time;
		public readonly Action action;

		public DelayedAction(Action action, long time)
		{
			this.action = action;
			this.time = time;
		}

		public int CompareTo(DelayedAction other)
		{
			return time.CompareTo(other.time);
		}

		public override string ToString()
		{
			return "Time: " + time + " Action: " + action;
		}
	}
}
