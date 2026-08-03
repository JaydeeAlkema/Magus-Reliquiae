using System;
using PlayerStats;

namespace Relic
{
	public sealed class RelicRuntimeContext
	{
		public PlayerStatsModel PlayerStats { get; }

		public RelicRuntimeContext(PlayerStatsModel playerStats)
		{
			PlayerStats = playerStats ?? throw new ArgumentNullException(nameof(playerStats));
		}
	}
}
