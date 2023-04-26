using MoreMountains.Feedbacks;

namespace Framework.Kits.AnimKits.AnimStates
{
	public class FeedbackAnim:AnimState
	{
		private MMFeedbacks _feedbacks;
		
		public override float ElapsedTime => _feedbacks.ElapsedTime;
		public override float Duration => _feedbacks.TotalDuration;
		public override bool IsPlaying => _feedbacks.IsPlaying;
		
		public static FeedbackAnim Create( string name, MMFeedbacks feedbacks)
		{
			var state=new FeedbackAnim {
				Name = name,
				_feedbacks = feedbacks,
			};
			return state;
		}
		public override void OnEnter()
		{
			_feedbacks.PlayFeedbacks();
		}
		public override void OnPause()
		{
			_feedbacks.PauseFeedbacks();
		}
		public override void OnResume()
		{
			_feedbacks.ResumeFeedbacks();
		}
		public override void OnUpdate()
		{
			
		}
		public override void OnLeave()
		{
			_feedbacks.StopFeedbacks();
		}
	}
}