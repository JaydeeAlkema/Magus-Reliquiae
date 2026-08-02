using StateMachine;

namespace Game
{
	public class GameState
	{
		private readonly StateMachine.StateMachine _stateMachine;

		public GameState(State gameStartState)
		{
			_stateMachine = new StateMachine.StateMachine();
			_stateMachine.Setup(gameStartState);
		}

		public void Update()
		{
			_stateMachine?.Update();
		}
	}
}
