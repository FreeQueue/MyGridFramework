using System.Collections.Generic;

namespace Framework.Kits.AnimKits
{
	public class AnimFsm
	{
		private readonly Dictionary<string, AnimState> _states=new Dictionary<string, AnimState>();
		private AnimState _currentState;
		
		public void Play(string animName)
		{
			_currentState?.OnLeave();
			_currentState = _states[animName];
			_currentState.OnEnter();
		}
		public void Play(AnimState animState)
		{
			_currentState?.OnLeave();
			_currentState = animState;
			_currentState.OnEnter();
		}
		
		public void Update()
		{
			_currentState?.OnUpdate();
		}
		
		public void AddState(AnimState state)
		{
			_states.Add(state.Name,state);
		}
		
		public void RemoveState(string stateName)
		{
			if (_currentState.Name==stateName) {
				_currentState.OnLeave();
			}
			_states.Remove(stateName);
		}
	}
}