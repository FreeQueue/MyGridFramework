using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.CommandKits
{
	public abstract class Command:IReference
	{
		public void Execute() {
			InnerExecute();
		}
		protected abstract bool InnerExecute();
		public abstract void Clear();
	}
}