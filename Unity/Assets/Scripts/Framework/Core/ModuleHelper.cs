using Framework.Kits.SingletonKits;

namespace Framework
{
	public abstract class ModuleHelper<T>:MonoSingleton<T> where T : ModuleHelper<T>
	{
		
	}
}