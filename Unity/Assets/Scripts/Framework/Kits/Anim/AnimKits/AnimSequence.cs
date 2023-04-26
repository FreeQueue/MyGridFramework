using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Framework.Kits.AnimKits
{
	//记录Tween动画，Feedback动画，顺序播放
	public class AnimSequence
	{
		private Sequence _sequence;
		private FeedbacksMap _feedbacksMap;
		public float TotalTime => _sequence.Duration();
		public float LastTotalTime { get; private set; }
		public FeedbacksMap FeedbacksMap {
			get => _feedbacksMap ??= new FeedbacksMap();
			set => _feedbacksMap = value;
		}

		public AnimSequence()
		{

		}
		public AnimSequence(IEnumerable<MMFeedbacks> players)
		{
			Debug.Assert(players != null);
			FeedbacksMap.AddFeedbacksList(players);
		}

		public void Play(TweenCallback onComplete = null)
		{
			_sequence.OnComplete(onComplete);
			_sequence.Play();
		}
		public void Reset()
		{
			_sequence?.Kill();
			LastTotalTime = 0;
			_sequence=DOTween.Sequence();
			_sequence.SetAutoKill(false);
		}
		
		public AnimSequence Append(Tween tween)
		{
			LastTotalTime = TotalTime;
			_sequence.Append(tween);
			return this;
		}

		public AnimSequence AppendInterval(float interval)
		{
			LastTotalTime = TotalTime;
			_sequence.AppendInterval(interval);
			return this;
		}

		public AnimSequence AppendIntervalTo(float atPosition)
		{
			LastTotalTime = TotalTime;
			_sequence.AppendInterval(atPosition - TotalTime);
			return this;
		}
		
		public AnimSequence Join(Tween tween)
		{
			_sequence.Join(tween);
			return this;
		}

		public AnimSequence Insert(float atPosition, Tween tween)
		{
			_sequence.Insert(atPosition, tween);
			return this;
		}

		public AnimSequence AppendPlayer(string playerName)
		{
			LastTotalTime = TotalTime;
			MMFeedbacks feedbacks = FeedbacksMap.GetFeedbacks(playerName);
			Debug.Assert(feedbacks != null);
			_sequence.AppendCallback(() => feedbacks.PlayFeedbacks());
			_sequence.AppendInterval(feedbacks.TotalDuration);
			return this;
		}

		public AnimSequence JoinPlayer(string playerName)
		{
			MMFeedbacks feedbacks = FeedbacksMap.GetFeedbacks(playerName);
			Debug.Assert(feedbacks != null);
			_sequence.InsertCallback(LastTotalTime, () => feedbacks.PlayFeedbacks());
			_sequence.AppendCallback(() => {
				if (feedbacks.IsPlaying) feedbacks.StopFeedbacks();
			});
			return this;
		}
		public AnimSequence PlayPlayer(string playerName)
		{
			MMFeedbacks feedbacks = FeedbacksMap.GetFeedbacks(playerName);
			Debug.Assert(feedbacks != null);
			_sequence.AppendCallback(() => feedbacks.PlayFeedbacks());
			return this;
		}
		
		public AnimSequence InsertPlayer(float atPosition, string playerName)
		{
			MMFeedbacks feedbacks = FeedbacksMap.GetFeedbacks(playerName);
			Debug.Assert(feedbacks != null);
			_sequence.InsertCallback(atPosition, () => feedbacks.PlayFeedbacks());
			return this;
		}
		
		public AnimSequence AppendCallback(TweenCallback callback, float delay = 0)
		{
			LastTotalTime = TotalTime;
			_sequence.AppendInterval(delay);
			_sequence.AppendCallback(callback);
			return this;
		}

		public AnimSequence JoinCallback(TweenCallback callback, float delay = 0)
		{
			_sequence.InsertCallback(LastTotalTime + delay, callback);
			return this;
		}
		public AnimSequence PlayCallback(TweenCallback callback, float delay = 0)
		{
			_sequence.InsertCallback(TotalTime + delay, callback);
			return this;
		}
		public AnimSequence InsertCallback(float atPosition, TweenCallback callback)
		{
			_sequence.InsertCallback(atPosition, callback);
			return this;
		}
	}
}