namespace Framework
{
	public interface IModule
	{
		public virtual void Init() {
		}
		public virtual void Update(float elapseSeconds, float realElapseSeconds) {
		}
		public virtual void Shutdown() {
		}
	}
}