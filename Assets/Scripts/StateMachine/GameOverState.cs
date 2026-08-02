using System;

namespace StateMachine
{
	public class GameOverState : State
	{
		public override bool IsDone { get; protected set; }
		public override State NextState { get; protected set; }

		public override void OnEnter()
		{
			throw new InvalidOperationException($"{nameof(GameOverState)} has not been implemented yet.");
		}

		public override void OnExit() { }
		public override void Update() { }
	}
}
