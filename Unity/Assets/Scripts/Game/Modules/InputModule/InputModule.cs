using Framework;

namespace S100.Modules
{
	public class InputModule:IModule
	{
		public DefaultControls Controls { get; private set; }
		void IModule.Init() {
			Controls = new DefaultControls();
		}
	}
}