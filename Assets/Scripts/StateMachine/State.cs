namespace StateMachine
{
	public abstract class State
	{
		public abstract bool IsDone { get; protected set; }
		public abstract State NextState { get; protected set; }

		public abstract void OnEnter();
		public abstract void OnExit();
		public abstract void Update();
	}
}
