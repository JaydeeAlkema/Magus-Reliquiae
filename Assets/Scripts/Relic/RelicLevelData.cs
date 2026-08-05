using System;
using System.Collections.Generic;
using UnityEngine;

namespace Relic
{
	/// <summary>
	///     Per-level relic data container.
	/// </summary>
	/// <remarks>
	///     Populate it inside <see cref="RelicSO" /> to describe upgrades, stats, and level-specific text.
	/// </remarks>
	[Serializable]
	public class RelicLevelData
	{
		/// <summary>
		///     Level-specific description text.
		/// </summary>
		[TextArea(2, 6)] public string Description;
		/// <summary>
		///     Stat modifiers applied at this level.
		/// </summary>
		public List<RelicStatModifier> StatModifiers = new();
	}
}
