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
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Activities
{
	public enum ActivityState
	{
		Queued,
		Active,
		Canceling,
		Done,
	}

	/*
	 * Things to be aware of when writing activities:
	 *
	 * - Use "return true" at least once somewhere in the tick method.
	 * - Do not "reuse" activity objects (by queuing them as next or child, for example) that have already started running.
	 *   Queue a new instance instead.
	 * - Avoid calling actor.CancelActivity(). It is almost always a bug. Call activity.Cancel() instead.
	 * - Do not evaluate dynamic state (an actor's location, health, conditions, etc.) in the activity's constructor,
	 *   as that might change before the activity gets to tick for the first time.  Use the OnFirstRun() method instead.
	 */
	public abstract class Activity : IActivityInterface
	{

		private Activity _childActivity;
		private bool _finishing;
		private bool _firstRunCompleted;
		private bool _lastRun;

		private Activity _nextActivity;

		public Activity() {
			IsInterruptible = true;
			ChildHasPriority = true;
		}
		public ActivityState State { get; private set; }
		protected Activity ChildActivity {
			get => SkipDoneActivities(_childActivity);
			private set => _childActivity = value;
		}
		public Activity NextActivity { get => SkipDoneActivities(_nextActivity); private set => _nextActivity = value; }

		public bool IsInterruptible { get; protected set; }
		public bool ChildHasPriority { get; protected set; }
		public bool IsCanceling => State == ActivityState.Canceling;

		internal static Activity SkipDoneActivities(Activity first) {
			// If first.Cancel() was called while it was queued (i.e. before it first ticked), its state will be Done
			// rather than Queued (the activity system guarantees that it cannot be Active or Canceling).
			// An unknown number of ticks may have elapsed between the Cancel() call and now,
			// so we cannot make any assumptions on the value of first.NextActivity.
			// We must not return first (ticking it would be bogus), but returning null would potentially
			// drop valid activities queued after it. Walk the queue until we find a valid activity or
			// (more likely) run out of activities.
			while (first is { State: ActivityState.Done }) {
				first = first._nextActivity;
			}
			return first;
		}

		public Activity TickOuter(Actor self) {
			if (State == ActivityState.Done) {
				throw new InvalidOperationException(
					$"Actor {self} attempted to tick activity {GetType()} after it had already completed.");
			}

			if (State == ActivityState.Queued) {
				OnFirstRun(self);
				_firstRunCompleted = true;
				State = ActivityState.Active;
			}

			if (!_firstRunCompleted) {
				throw new InvalidOperationException(
					$"Actor {self} attempted to tick activity {GetType()} before running its OnFirstRun method.");
			}

			// Only run the parent tick when the child is done.
			// We must always let the child finish on its own before continuing.
			if (ChildHasPriority) {
				_lastRun = TickChild(self) && (_finishing || Tick(self));
				_finishing |= _lastRun;
			}

			// The parent determines whether the child gets a chance at ticking.
			else {
				_lastRun = Tick(self);
			}

			// Avoid a single tick delay if the childActivity was just queued.
			Activity ca = ChildActivity;
			if (ca is { State: ActivityState.Queued }) {
				if (ChildHasPriority) {
					_lastRun = TickChild(self) && _finishing;
				}
				else {
					TickChild(self);
				}
			}

			if (_lastRun) {
				State = ActivityState.Done;
				OnLastRun(self);
				return NextActivity;
			}

			return this;
		}

		protected bool TickChild(Actor self) {
			ChildActivity = self.RunActivity(ChildActivity);
			return ChildActivity == null;
		}

		/// <summary>
		///     Called every tick to run activity logic. Returns false if the activity should
		///     remain active, or true if it is complete. Cancelled activities must ensure they
		///     return the actor to a consistent state before returning true.
		///     Child activities can be queued using QueueChild, and these will be ticked
		///     instead of the parent while they are active. Activities that need to run logic
		///     in parallel with child activities should set ChildHasPriority to false and
		///     manually call TickChildren.
		///     Queuing one or more child activities and returning true is valid, and causes
		///     the activity to be completed immediately (without ticking again) once the
		///     children have completed.
		/// </summary>
		public virtual bool Tick(Actor self) {
			return true;
		}

		/// <summary>
		///     Runs once immediately before the first Tick() execution.
		/// </summary>
		protected virtual void OnFirstRun(Actor self) {
		}

		/// <summary>
		///     Runs once immediately after the last Tick() execution.
		/// </summary>
		protected virtual void OnLastRun(Actor self) {
		}

		/// <summary>
		///     Runs once on Actor.Dispose() (through OnActorDisposeOuter) and can be used to perform activity clean-up on actor
		///     death/disposal,
		///     for example by force-triggering OnLastRun (which would otherwise be skipped).
		/// </summary>
		protected virtual void OnActorDispose(Actor self) {
		}

		/// <summary>
		///     Runs once on Actor.Dispose().
		///     Main purpose is to ensure ChildActivity.OnActorDispose runs as well (which isn't otherwise accessible due to
		///     protection level).
		/// </summary>
		internal void OnActorDisposeOuter(Actor self) {
			ChildActivity?.OnActorDisposeOuter(self);

			OnActorDispose(self);
		}

		public virtual void Cancel(Actor self, bool keepQueue = false) {
			if (!keepQueue) {
				NextActivity = null;
			}

			if (!IsInterruptible) {
				return;
			}

			ChildActivity?.Cancel(self);

			// Directly mark activities that are queued and therefore didn't run yet as done
			State = State == ActivityState.Queued ? ActivityState.Done : ActivityState.Canceling;
		}

		public void Queue(Activity activity) {
			Activity it = this;
			while (it._nextActivity != null) {
				it = it._nextActivity;
			}
			it._nextActivity = activity;
		}

		public void QueueChild(Activity activity) {
			if (_childActivity != null) {
				_childActivity.Queue(activity);
			}
			else {
				_childActivity = activity;
			}
		}

		/// <summary>
		///     Prints the activity tree, starting from the top or optionally from a given origin.
		///     Call this method from any place that's called during a tick, such as the Tick() method itself or
		///     the Before(First|Last)Run() methods. The origin activity will be marked in the output.
		/// </summary>
		/// <param name="self">The actor performing this activity.</param>
		/// <param name="origin">
		///     Activity from which to start traversing, and which to mark. If null, mark the calling activity,
		///     and start traversal from the top.
		/// </param>
		/// <param name="level">Initial level of indentation.</param>
		protected void PrintActivityTree(Actor self, Activity origin = null, int level = 0) {
			if (origin == null) {
				self.CurrentActivity.PrintActivityTree(self, this);
			}
			else {
				Console.Write(new string(' ', level * 2));
				if (origin == this) {
					Console.Write("*");
				}

				Console.WriteLine(GetType().ToString().Split('.').Last());

				ChildActivity?.PrintActivityTree(self, origin, level + 1);

				NextActivity?.PrintActivityTree(self, origin, level);
			}
		}

		// public virtual IEnumerable<Target> GetTargets(Actor self)
		// {
		// 	yield break;
		// }
		//
		// public virtual IEnumerable<TargetLineNode> TargetLineNodes(Actor self)
		// {
		// 	yield break;
		// }

		public IEnumerable<string> DebugLabelComponents() {
			Activity act = this;
			while (act != null) {
				yield return act.GetType().Name;
				act = act.ChildActivity;
			}
		}

		public IEnumerable<T> ActivitiesImplementing<T>(bool includeChildren = true) where T : IActivityInterface {
			// Skips Done child and next activities
			if (includeChildren) {
				Activity ca = ChildActivity;
				if (ca != null) {
					foreach (T a in ca.ActivitiesImplementing<T>()) {
						yield return a;
					}
				}
			}

			if (this is T) {
				yield return (T)(object)this;
			}

			Activity na = NextActivity;
			if (na == null) yield break;
			foreach (T a in na.ActivitiesImplementing<T>()) {
				yield return a;
			}
		}
	}
}