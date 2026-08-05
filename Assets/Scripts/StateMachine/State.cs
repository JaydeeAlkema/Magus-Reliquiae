namespace StateMachine
{
	/// <summary>
	///     Base type for all state-machine states.
	/// </summary>
	/// <remarks>
	///     Derive from this for gameplay flow and let <see cref="StateMachine" /> own the lifecycle.
	/// </remarks>
	public abstract class State
	{
		/// <summary>
		///     True when the state has finished its work and can transition.
		/// </summary>
		public abstract bool IsDone { get; protected set; }
		/// <summary>
		///     Next state to enter once this one completes.
		/// </summary>
		public abstract State NextState { get; protected set; }

		/// <summary>
		///     Called when the state becomes active.
		/// </summary>
		public abstract void OnEnter();
		/// <summary>
		///     Called when the state is exited.
		/// </summary>
		public abstract void OnExit();
		/// <summary>
		///     Called every frame while the state is active.
		/// </summary>
		public abstract void Update();
	}
}
