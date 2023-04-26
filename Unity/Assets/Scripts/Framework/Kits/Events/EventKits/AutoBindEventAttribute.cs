using System;

namespace Framework.Kits.EventKits
{
	[AttributeUsage(AttributeTargets.Method)]
	public class AutoBindEventAttribute:Attribute
	{
		public int EventId { get; }
		public AutoBindEventAttribute(int eventId) {
			EventId = eventId;
		}
	}
}