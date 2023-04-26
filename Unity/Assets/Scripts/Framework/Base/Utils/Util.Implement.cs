using System;
using System.Collections.Generic;
using SType = System.Type;

namespace Framework
{
	partial class Util
	{
		public static class Implement<T> where T : class
		{
			private static IDictionary<string, SType> Cache = new Dictionary<string, SType>();
			private static int _dirty;

			public static IEnumerable<SType> GetTypes() => Cache.Values;
			public static IEnumerable<string> GetTypeNames() => Cache.Keys;
			public static Action Refresh;

			static Implement() {
				if (typeof(T).IsInterface) {
					Refresh = () => {
						Type.GetAllImplementsOfType(typeof(T), Cache);
						_dirty = Assembly.Dirty;
					};
				}
				else {
					Refresh = () => {
						Type.GetAllSubclassesOfType(typeof(T), Cache);
						_dirty = Assembly.Dirty;
					};
				}
			}

			public static SType GetByName(string name) {
				if (_dirty != Assembly.Dirty) Refresh();
				return Cache.TryGetValue(name, out SType type) ? type : null;
			}
		}
	}
}