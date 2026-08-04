using System;

namespace StateMachine
{
	/// <summary>
	/// Pause-mode state.
	/// </summary>
	/// <remarks>
	/// Create and enter it through the game state machine; it is not a scene component.
	/// </remarks>
	public class GamePlayPausedState : State
	{
		/// <summary>
		/// True when the state is finished.
		/// </summary>
		public override bool IsDone { get; protected set; }
		/// <summary>
		/// The next state after pause exits.
		/// </summary>
		public override State NextState { get; protected set; }

		/// <summary>
		/// Enters the pause state.
		/// </summary>
		public override void OnEnter()
		{
			throw new InvalidOperationException($"{nameof(GamePlayPausedState)} has not been implemented yet.");
		}

		/// <summary>
		/// No cleanup is required.
		/// </summary>
		public override void OnExit() { }
		/// <summary>
		/// No per-frame work is required.
		/// </summary>
		public override void Update() { }
	}
}
