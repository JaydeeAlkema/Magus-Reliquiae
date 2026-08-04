using System.Collections.Generic;
using UnityEngine;

namespace Relic
{
	/// <summary>
	/// Catalogue asset for available relic definitions.
	/// </summary>
	/// <remarks>
	/// Create one asset and keep the unlockable relic list there.
	/// </remarks>
	[CreateAssetMenu(fileName = "RelicCatalogue", menuName = "ScriptableObjects/Relic/Catalogue", order = 1)]
	public class RelicCatalogueSO : ScriptableObject
	{
		/// <summary>
		/// Relics available to the run.
		/// </summary>
		public readonly List<RelicSO> Relics = new();
	}
}
