using System;
using StateMachine;

namespace Game
{
	public class GameStateManager
	{
		private readonly StateMachine.StateMachine _stateMachine;

		public event Action<State, State> onStateChanged;

		public State CurrentState => _stateMachine.CurrentState;

		public GameStateManager(State gameStartState)
		{
			_stateMachine = new StateMachine.StateMachine();
			_stateMachine.onStateChange += (prev, next) => onStateChanged?.Invoke(prev, next);
			_stateMachine.Setup(gameStartState);
		}

		public void Update()
		{
			_stateMachine.Update();
		}
	}
}
