using System;

namespace StateMachine
{
	public class StateMachine
	{
		public event Action<State, State> onStateChange;

		public State CurrentState { get; private set; }
		public bool IsReady { get; private set; }

		public void Setup(State startState)
		{

			CurrentState = startState ?? throw new ArgumentNullException(nameof(startState));
			StateMachineLog.Log($"Starting state machine with {CurrentState.GetType().Name}");
			CurrentState.OnEnter();

			IsReady = true;
		}

		public void Update()
		{
			if (!IsReady || CurrentState == null)
				return;

			CurrentState.Update();
			if (!CurrentState.IsDone)
				return;

			State previousState = CurrentState;
			State nextState = previousState.NextState;
			if (nextState == null)
			{
				StateMachineLog.LogError($"{previousState.GetType().Name} completed without a next state.");
				return;
			}

			previousState.OnExit();
			CurrentState = nextState;
			StateMachineLog.Log($"Transitioning from {previousState.GetType().Name} to {nextState.GetType().Name}");
			CurrentState.OnEnter();
			onStateChange?.Invoke(previousState, CurrentState);
		}
	}
}
