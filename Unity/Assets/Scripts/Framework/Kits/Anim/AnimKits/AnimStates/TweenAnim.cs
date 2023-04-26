using DG.Tweening;

namespace Framework.Kits.AnimKits.AnimStates
{
	public class TweenAnim : AnimState
	{
		private Tween _tween;
		public override float ElapsedTime => _tween.Elapsed();
		public override float Duration => _tween.Duration();
		public override bool IsPlaying => _tween.IsPlaying();
		
		
		public static TweenAnim Create(string name,Tween tween)
		{
			var state=new TweenAnim {
				Name = name,
				_tween = tween,
			};
			return state;
		}
		
		public override void OnPause()
		{
			_tween.Pause();
		}
		public override void OnResume()
		{
			_tween.Play();
		}
		public override void OnEnter()
		{
			_tween.Play();
		}
		public override void OnUpdate()
		{
			
		}
		public override void OnLeave()
		{
			
		}
	}
}