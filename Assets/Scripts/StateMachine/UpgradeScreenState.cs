using System;

namespace StateMachine
{
	public class UpgradeScreenState : State
	{
		public override bool IsDone { get; protected set; }
		public override State NextState { get; protected set; }

		public override void OnEnter()
		{
			throw new InvalidOperationException($"{nameof(UpgradeScreenState)} has not been implemented yet.");
		}

		public override void OnExit() { }
		public override void Update() { }
	}
}
