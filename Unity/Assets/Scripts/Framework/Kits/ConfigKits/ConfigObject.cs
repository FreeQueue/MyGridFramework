using Sirenix.OdinInspector;

namespace Framework.Kits.ConfigKits
{
	public abstract class ConfigObject:SerializedScriptableObject
	{
		public abstract void OnInit();
	}
}