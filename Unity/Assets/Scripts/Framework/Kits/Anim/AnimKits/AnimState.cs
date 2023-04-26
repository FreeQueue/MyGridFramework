namespace Framework.Kits.AnimKits
{
	public abstract class AnimState
	{
		public AnimFsm Fsm { get;internal set; }
		public string Name { get;protected set; }
		public abstract float ElapsedTime { get; }
		public abstract float Duration { get; }
		public abstract bool IsPlaying { get; }
		public abstract void OnEnter();
		public abstract void OnPause();
		public abstract void OnResume();
		public abstract void OnUpdate();
		public abstract void OnLeave();
	}
}