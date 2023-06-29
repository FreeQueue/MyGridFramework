using Framework.Kits.IOCKits;

namespace Framework.Kits.ConfigKits
{
	public class ConfigManager
	{
		private NameIocContainer<IConfig> _configs;
		public void Register(IConfig config,string name) {
			config.Init();
			_configs.Register(config,name);
		}
		public bool Contains<TConfig>(string name=null) where TConfig : IConfig {
			return _configs.Contains<TConfig>(name);
		}

		public TConfig Get<TConfig>(string name=null) where TConfig : IConfig {
			return _configs.Get<TConfig>(name);
		}
	}
}