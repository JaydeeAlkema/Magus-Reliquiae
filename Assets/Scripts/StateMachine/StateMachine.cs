using System;

namespace StateMachine
{
	/// <summary>
	/// Generic state machine used by the game flow.
	/// </summary>
	/// <remarks>
	/// Initialize it with a starting <see cref="State"/>, then call <see cref="Update"/> from the owning system each frame.
	/// </remarks>
	public class StateMachine
	{
		/// <summary>
		/// Fired whenever the active state changes.
		/// </summary>
		public event Action<State, State> onStateChange;

		/// <summary>
		/// Current active state.
		/// </summary>
		public State CurrentState { get; private set; }
		/// <summary>
		/// True after <see cref="Setup"/> has been called successfully.
		/// </summary>
		public bool IsReady { get; private set; }

		/// <summary>
		/// Starts the state machine with the given state.
		/// </summary>
		/// <param name="startState">Initial state.</param>
		public void Setup(State startState)
		{

			CurrentState = startState ?? throw new ArgumentNullException(nameof(startState));
			StateMachineLog.Log($"Starting state machine with {CurrentState.GetType().Name}");
			CurrentState.OnEnter();

			IsReady = true;
		}

		/// <summary>
		/// Advances the active state and performs transitions when needed.
		/// </summary>
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
