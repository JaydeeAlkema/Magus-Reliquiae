using Relic;

namespace Game
{
	public sealed class GameContext
	{
		public Player.Player Player { get; set; }

		public RelicCatalogueSO RelicCatalogue { get; }

		public float[] RarityWeights { get; }

		public GameContext(RelicCatalogueSO catalogue, float[] rarityWeights)
		{
			RelicCatalogue = catalogue;
			RarityWeights = rarityWeights;
		}
	}
}
