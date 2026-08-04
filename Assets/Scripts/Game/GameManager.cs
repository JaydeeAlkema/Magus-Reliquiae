using Relic;
using StateMachine;
using UnityEngine;

namespace Game
{
	/// <summary>
	/// Scene-level game bootstrapper.
	/// </summary>
	/// <remarks>
	/// Place this on the persistent GameManager prefab. It sets the target frame rate, builds the
	/// <see cref="GameContext"/>, and owns the top-level <see cref="GameStateManager"/>.
	/// </remarks>
	public class GameManager : MonoBehaviour
	{
		[Header("Relic System")]
		[SerializeField] private RelicCatalogueSO RelicCatalogue;
		[SerializeField] private float[] RarityWeights =
		{
			60f, 25f, 12f, 3f,
		};

		public GameStateManager StateManager { get; private set; }

		/// <summary>
		/// Initializes the run and creates the state machine.
		/// </summary>
		private void Awake()
		{
			DontDestroyOnLoad(this.gameObject);

			int refreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
			if (refreshRate <= 0)
				refreshRate = 60;

			Application.targetFrameRate = Application.isMobilePlatform
				? Mathf.Min(refreshRate, 60)
				: refreshRate;

			float[] weights = RarityWeights is { Length: 4 } ? RarityWeights : RelicOfferGenerator.DefaultWeights;
			GameContext context = new(RelicCatalogue, weights);
			StateManager = new GameStateManager(new StartGameState(context));
		}

		/// <summary>
		/// Advances the active game state each frame.
		/// </summary>
		private void Update()
		{
			StateManager.Update();
		}
	}
}
