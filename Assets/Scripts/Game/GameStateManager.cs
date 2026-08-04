using System;
using StateMachine;

namespace Game
{
	/// <summary>
	/// Thin wrapper around the game state machine.
	/// </summary>
	/// <remarks>
	/// Use this to expose state-change notifications to the rest of the game layer without leaking the underlying implementation.
	/// </remarks>
	public class GameStateManager
	{
		private readonly StateMachine.StateMachine _stateMachine;

		/// <summary>
		/// Fired when the active state changes.
		/// </summary>
		public event Action<State, State> onStateChanged;

		/// <summary>
		/// Current active state.
		/// </summary>
		public State CurrentState => _stateMachine.CurrentState;

		/// <summary>
		/// Creates and initializes the wrapped state machine.
		/// </summary>
		/// <param name="gameStartState">Initial state to enter.</param>
		public GameStateManager(State gameStartState)
		{
			_stateMachine = new StateMachine.StateMachine();
			_stateMachine.onStateChange += (prev, next) => onStateChanged?.Invoke(prev, next);
			_stateMachine.Setup(gameStartState);
		}

		/// <summary>
		/// Advances the underlying state machine.
		/// </summary>
		public void Update()
		{
			_stateMachine.Update();
		}
	}
}
