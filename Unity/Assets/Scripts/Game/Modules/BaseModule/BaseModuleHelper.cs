using Framework;
using UnityEngine;

namespace S100.Modules
{
	internal class BaseModuleHelper : ModuleHelper<BaseModuleHelper>
	{
		[SerializeField] internal bool monoSingletonAutoCreate;

		[SerializeField] internal int frameRate = 30;

		[SerializeField] internal float gameSpeed = 1f;

		[SerializeField] internal bool runInBackground = false;

		[SerializeField] internal bool neverSleep = false;
	}
}