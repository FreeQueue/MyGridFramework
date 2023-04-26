using UnityEngine;

namespace S100
{
	public class LocationInit : ValueActorInit<Vector2Int>, ISingleInstanceInit
	{
		public LocationInit(Vector2Int value)
			: base(value) {
		}
	}
}