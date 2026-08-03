using System.Collections.Generic;
using UnityEngine;

namespace Relic
{
	[CreateAssetMenu(fileName = "RelicCatalogue", menuName = "ScriptableObjects/Relic/Catalogue", order = 1)]
	public class RelicCatalogueSO : ScriptableObject
	{
		public readonly List<RelicSO> Relics = new();
	}
}
