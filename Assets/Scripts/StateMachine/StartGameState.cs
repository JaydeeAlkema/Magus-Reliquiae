using System;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StateMachine
{
	/// <summary>
	///     Boot state that starts a new run.
	/// </summary>
	/// <remarks>
	///     Create it through the state machine setup, not as a scene object.
	/// </remarks>
	public class StartGameState : State
	{
		private const int MAIN_MENU_SCENE_BUILD_INDEX = 1;

		private readonly GameContext _context;

		/// <summary>
		///     True when the state is finished.
		/// </summary>
		public override bool IsDone { get; protected set; }
		/// <summary>
		///     The next state after the main menu loads.
		/// </summary>
		public override State NextState { get; protected set; }

		private AsyncOperation _loadOperation;

		/// <summary>
		///     Creates the start-game state.
		/// </summary>
		/// <param name="context">Shared game context.</param>
		public StartGameState(GameContext context)
		{
			_context = context;
		}

		/// <summary>
		///     Begins loading the main menu scene.
		/// </summary>
		public override void OnEnter()
		{
			IsDone = false;
			NextState = null;

			_loadOperation = SceneManager.LoadSceneAsync(MAIN_MENU_SCENE_BUILD_INDEX);
			if (_loadOperation == null)
			{
				throw new InvalidOperationException($"Failed to load main menu scene at build index {MAIN_MENU_SCENE_BUILD_INDEX}.");
			}

			_loadOperation.completed += OnMainMenuSceneLoaded;
		}

		/// <summary>
		///     Unhooks scene-load callbacks.
		/// </summary>
		public override void OnExit()
		{
			if (_loadOperation == null)
				return;

			_loadOperation.completed -= OnMainMenuSceneLoaded;
			_loadOperation = null;
		}

		/// <summary>
		///     No per-frame work is required while waiting for the scene load.
		/// </summary>
		public override void Update() { }

		private void OnMainMenuSceneLoaded(AsyncOperation _)
		{
			_loadOperation.completed -= OnMainMenuSceneLoaded;
			_loadOperation = null;

			NextState = new MainMenuState(_context);
			IsDone = true;
		}
	}
}
