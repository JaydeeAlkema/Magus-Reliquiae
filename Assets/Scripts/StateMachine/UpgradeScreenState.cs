using System;

namespace StateMachine
{
	public class UpgradeScreenState : State
	{
		public override bool IsDone { get; protected set; }
		public override State NextState { get; protected set; }

		public override void OnEnter()
		{
			throw new NotImplementedException();
		}
		public override void OnExit()
		{
			throw new NotImplementedException();
		}
		public override void Update()
		{
			throw new NotImplementedException();
		}
	}
}
