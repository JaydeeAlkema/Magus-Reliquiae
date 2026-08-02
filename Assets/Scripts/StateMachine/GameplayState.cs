namespace StateMachine
{
	public class GameplayState : State
	{
		public override bool IsDone { get; protected set; }
		public override State NextState { get; protected set; }

		public override void OnEnter()
		{
			StateMachineLog.Log("Entering Gameplay State");
		}

		public override void OnExit()
		{
			StateMachineLog.Log("Exiting Gameplay State");
		}

		public override void Update() { }
	}
}
