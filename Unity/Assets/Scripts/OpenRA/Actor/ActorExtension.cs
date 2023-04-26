using System.Collections.Generic;
using OpenRA.Activities;

namespace OpenRA
{
	public static class ActorExtension
	{
		public static Activity RunActivity(this Actor @this, Activity act) {
			// PERF: This is a hot path and must run with minimal added overhead.
			// If there are no activities we can bail straight away and save ourselves the overhead of setting up the perf logging.
			if (act == null) return null;

			// var perfLogger = new PerfTickLogger();
			// perfLogger.Start();
			while (act != null) {
				Activity prev = act;
				act = act.TickOuter(@this);
				//perfLogger.LogTickAndRestartTimer("Activity", prev);

				if (act == prev) {
					break;
				}
			}

			return act;
		}

		public static ActorInfo GetSystem(this IReadOnlyDictionary<string,ActorInfo> @this,SystemActors actor) {
			return @this[actor.ToString().ToLowerInvariant()];
		}
	}
}