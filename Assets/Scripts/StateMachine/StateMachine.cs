using System;

namespace StateMachine
{
	public class StateMachine
	{
		public event Action<State, State> onStateChange;

		public State StartState;
		public State CurrentState;

		public bool IsReady;

		public void Setup(State startState)
		{
			StartState = startState;
			CurrentState = startState;

			IsReady = true;
		}

		public void Update()
		{
			if (!IsReady)
				return;

			Simulate();
		}

		private void Simulate()
		{
			if (CurrentState == null)
				return;

			CurrentState.Update();
			if (!CurrentState.IsDone)
				return;

			State nextState = CurrentState.NextState;
			if (nextState == null)
				return;

			State previousState = CurrentState;
			CurrentState = nextState;
			onStateChange?.Invoke(previousState, CurrentState);
		}
	}
}
