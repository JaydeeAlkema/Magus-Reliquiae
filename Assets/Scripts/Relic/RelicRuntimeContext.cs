using System;
using PlayerStats;

namespace Relic
{
	/// <summary>
	///     Runtime services bundle passed into relic behaviors.
	/// </summary>
	/// <remarks>
	///     Build it from the active player and stat model before ticking behavior logic.
	/// </remarks>
	public sealed class RelicRuntimeContext
	{
		/// <summary>
		///     Owning player, if available.
		/// </summary>
		public Player.Player Owner { get; }
		/// <summary>
		///     Player stat model.
		/// </summary>
		public PlayerStatsModel PlayerStats { get; }

		/// <summary>
		///     Creates the runtime context.
		/// </summary>
		/// <param name="playerStats">Player stat model.</param>
		/// <param name="owner">Optional owning player.</param>
		public RelicRuntimeContext(PlayerStatsModel playerStats, Player.Player owner = null)
		{
			PlayerStats = playerStats ?? throw new ArgumentNullException(nameof(playerStats));
			Owner = owner;
		}
	}
}
