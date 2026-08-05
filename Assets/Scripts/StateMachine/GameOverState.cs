using System;

namespace StateMachine
{
	/// <summary>
	///     Game-over state.
	/// </summary>
	/// <remarks>
	///     Instantiate it through the state machine flow and use it for end-of-run cleanup or transition logic.
	/// </remarks>
	public class GameOverState : State
	{
		/// <summary>
		///     True when the state is finished.
		/// </summary>
		public override bool IsDone { get; protected set; }
		/// <summary>
		///     The next state after game over.
		/// </summary>
		public override State NextState { get; protected set; }

		/// <summary>
		///     Enters the game-over state.
		/// </summary>
		public override void OnEnter()
		{
			throw new InvalidOperationException($"{nameof(GameOverState)} has not been implemented yet.");
		}

		/// <summary>
		///     No cleanup is required.
		/// </summary>
		public override void OnExit() { }
		/// <summary>
		///     No per-frame work is required.
		/// </summary>
		public override void Update() { }
	}
}
