
using cfg;
using Framework;
using Framework.Kits.ConfigKits;
using Framework.Kits.ResourceKits;
using SimpleJSON;

namespace S100.Modules
{
	public class ConfigModule:ConfigManager,IModule
	{
		public Tables Tables { get; private set; }
		void IModule.Init() {
			Tables = new Tables(Loader);
		}

		private static JSONNode Loader(string fileName) {
			return JSON.Parse(ResourceManager.LoadRawTextSync(fileName));
		}
	}
}