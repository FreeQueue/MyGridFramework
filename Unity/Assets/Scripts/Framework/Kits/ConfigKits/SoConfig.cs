using System;
using Framework.Kits.SingletonKits;

namespace Framework.Kits.ConfigKits
{
	public abstract class SoConfig<T> : SerializedScriptableSingleton<T> where T : SoConfig<T>,IConfig
	{
	}
}