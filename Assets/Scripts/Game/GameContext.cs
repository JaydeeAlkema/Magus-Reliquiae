using Relic;

namespace Game
{
	/// <summary>
	///     Lightweight aggregate of core game services.
	/// </summary>
	/// <remarks>
	///     Construct this during startup and pass it into state-machine states and generators that need access
	///     to the player, relic catalogue, or rarity weighting.
	/// </remarks>
	public sealed class GameContext
	{
		/// <summary>
		///     The active player instance for the current run.
		/// </summary>
		public Player.Player Player { get; set; }

		/// <summary>
		///     The relic catalogue used for offer generation and runtime lookups.
		/// </summary>
		public RelicCatalogueSO RelicCatalogue { get; }

		/// <summary>
		///     Weight table used when picking relic rarities.
		/// </summary>
		public float[] RarityWeights { get; }

		/// <summary>
		///     Creates a new game context.
		/// </summary>
		/// <param name="catalogue">Relic catalogue for the current run.</param>
		/// <param name="rarityWeights">Weight table for rarity selection.</param>
		public GameContext(RelicCatalogueSO catalogue, float[] rarityWeights)
		{
			RelicCatalogue = catalogue;
			RarityWeights = rarityWeights;
		}
	}
}
