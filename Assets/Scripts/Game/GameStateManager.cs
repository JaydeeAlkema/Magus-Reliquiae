using StateMachine;

namespace Game
{
	public class GameStateManager
	{
		private readonly StateMachine.StateMachine _stateMachine;

		public GameStateManager(State gameStartState)
		{
			_stateMachine = new StateMachine.StateMachine();
			_stateMachine.Setup(gameStartState);
		}

		public void Update()
		{
			_stateMachine.Update();
		}
	}
}
