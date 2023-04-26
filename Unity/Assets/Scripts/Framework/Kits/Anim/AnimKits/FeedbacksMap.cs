using System;
using System.Collections.Generic;
using Framework.Kits.ReferencePoolKits;
using MoreMountains.Feedbacks;
using Sirenix.Serialization;
using UnityEngine;

namespace Framework.Kits.AnimKits
{
	[Serializable]
	public class FeedbacksMap:IReference
	{
		[OdinSerialize]
		private readonly Dictionary<string, MMFeedbacks> _feedbacksMap = new Dictionary<string, MMFeedbacks>();

		public MMFeedbacks GetFeedbacks(string name)
		{
			return _feedbacksMap.TryGetValue(name, out MMFeedbacks feedbacks) ? feedbacks : null;
		}

		public void AddFeedbacksList(IEnumerable<MMFeedbacks> feedbacksList)
		{
			foreach (MMFeedbacks feedbacks in feedbacksList) {
				AddFeedbacks(feedbacks);
			}
		}
		
		public void AddFeedbacks(MMFeedbacks feedbacks)
		{
			_feedbacksMap.Add(feedbacks.name, feedbacks);
		}

		public void RemoveFeedbacks(string name)
		{
			_feedbacksMap.Remove(name);
		}
		
		public void RemoveAllFeedbacks()
		{
			_feedbacksMap.Clear();
		}

		public void PlayFeedbacks(string name)
		{
			MMFeedbacks feedbacks = GetFeedbacks(name);
			Debug.Assert(feedbacks != null, $"Feedbacks with Name[{name}] is invalid.");
			feedbacks.PlayFeedbacks();
		}
		
		public void PlayAllFeedbacks()
		{
			foreach (MMFeedbacks feedbacks in _feedbacksMap.Values) {
				feedbacks.PlayFeedbacks();
			}
		}

		public void PauseFeedbacks(string name)
		{
			MMFeedbacks feedbacks = GetFeedbacks(name);
			Debug.Assert(feedbacks != null, $"Feedbacks with Name[{name}] is invalid.");
			feedbacks.PauseFeedbacks();
		}

		public void PauseAllFeedbacks()
		{
			foreach (MMFeedbacks feedbacks in _feedbacksMap.Values) {
				feedbacks.PauseFeedbacks();
			}
		}

		public void ResumeFeedbacks(string name)
		{
			MMFeedbacks feedbacks = GetFeedbacks(name);
			Debug.Assert(feedbacks != null, $"Feedbacks with Name[{name}] is invalid.");
			feedbacks.ResumeFeedbacks();
		}

		public void ResumeAllFeedbacks()
		{
			foreach (MMFeedbacks feedbacks in _feedbacksMap.Values) {
				feedbacks.ResumeFeedbacks();
			}
		}
		public void ResetFeedbacks(string name)
		{
			MMFeedbacks feedbacks = GetFeedbacks(name);
			Debug.Assert(feedbacks != null, $"Feedbacks with Name[{name}] is invalid.");
			feedbacks.ResetFeedbacks();
		}
		public void ResetAllFeedbacks()
		{
			foreach (MMFeedbacks feedbacks in _feedbacksMap.Values) {
				feedbacks.ResetFeedbacks();
			}
		}

		public void StopFeedbacks(string name)
		{
			MMFeedbacks feedbacks = GetFeedbacks(name);
			Debug.Assert(feedbacks != null, $"Feedbacks with Name[{name}] is invalid.");
			feedbacks.StopFeedbacks();
		}

		public void StopAllFeedbacks()
		{
			foreach (MMFeedbacks feedbacks in _feedbacksMap.Values) {
				feedbacks.StopFeedbacks();
			}
		}
		
		public void Clear()
		{
			RemoveAllFeedbacks();
		}
	}
}