using Framework;
using Framework.Kits.ResourceKits;
using UnityEngine;

namespace S100.Modules
{
	internal class ResourceModuleHelper:ModuleHelper<ResourceModuleHelper>
	{
		[SerializeField]
		internal ResourceMode mode=ResourceMode.OfflinePlayMode;
		[SerializeField]
		internal string defaultPackageName = "DefaultPackage";
	}
}