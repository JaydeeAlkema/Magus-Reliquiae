using Relic;
using StateMachine;
using UnityEngine;

namespace Game
{
	public class GameManager : MonoBehaviour
	{
		[Header("Relic System")]
		[SerializeField] private RelicCatalogueSO RelicCatalogue;
		[SerializeField] private float[] RarityWeights =
		{
			60f, 25f, 12f, 3f,
		};

		public GameStateManager StateManager { get; private set; }

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

		private void Update()
		{
			StateManager.Update();
		}
	}
}
