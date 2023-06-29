namespace Framework
{
	public interface IModule
	{
		public void Init() {
		}
		public void Update(float elapseSeconds, float realElapseSeconds) {
		}
		public void Shutdown() {
		}
	}
}