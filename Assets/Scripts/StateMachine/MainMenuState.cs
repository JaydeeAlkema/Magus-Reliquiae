using System;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StateMachine
{
	/// <summary>
	///     Main-menu flow state.
	/// </summary>
	/// <remarks>
	///     Instantiate it through the game state machine to load the menu scene and wait for start input.
	/// </remarks>
	public class MainMenuState : State
	{
		private const int GAME_SCENE_BUILD_INDEX = 2;

		private readonly GameContext _context;

		/// <summary>
		///     True when the state is finished.
		/// </summary>
		public override bool IsDone { get; protected set; }
		/// <summary>
		///     The next state after the menu finishes loading.
		/// </summary>
		public override State NextState { get; protected set; }

		private AsyncOperation _loadOperation;

		/// <summary>
		///     Creates the menu state.
		/// </summary>
		/// <param name="context">Shared game context.</param>
		public MainMenuState(GameContext context)
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

			_loadOperation = SceneManager.LoadSceneAsync(GAME_SCENE_BUILD_INDEX);
			if (_loadOperation == null)
			{
				throw new InvalidOperationException($"Failed to load game scene at build index {GAME_SCENE_BUILD_INDEX}.");
			}

			_loadOperation.completed += OnGameSceneLoaded;
		}

		/// <summary>
		///     Unhooks scene-load callbacks.
		/// </summary>
		public override void OnExit()
		{
			if (_loadOperation == null)
				return;

			_loadOperation.completed -= OnGameSceneLoaded;
			_loadOperation = null;
		}

		/// <summary>
		///     No per-frame work is required while waiting for the menu scene.
		/// </summary>
		public override void Update() { }

		private void OnGameSceneLoaded(AsyncOperation _)
		{
			_loadOperation.completed -= OnGameSceneLoaded;
			_loadOperation = null;

			NextState = new GameplayState(_context);
			IsDone = true;
		}
	}
}
