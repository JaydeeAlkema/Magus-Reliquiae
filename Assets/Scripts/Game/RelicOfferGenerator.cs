using System.Collections.Generic;
using Relic;
using UnityEngine;

namespace Game
{
	public sealed class RelicOfferGenerator
	{
		public static readonly float[] DefaultWeights =
		{
			60f, 25f, 12f, 3f,
		};

		private readonly RelicCatalogueSO _catalogue;
		private readonly float[] _rarityWeights;

		public RelicOfferGenerator(RelicCatalogueSO catalogue, float[] rarityWeights = null)
		{
			_catalogue = catalogue;
			_rarityWeights = rarityWeights ?? DefaultWeights;
		}

		public List<RelicSO> GenerateOffers(int count)
		{
			List<RelicSO> result = new(count);

			List<(RelicSO relic, float weight)> candidates = BuildCandidates();
			if (candidates.Count == 0)
			{
				Debug.LogWarning("[RelicOfferGenerator] No relics available — returning no offers.");
				return result;
			}

			int take = Mathf.Min(count, candidates.Count);
			for (int i = 0; i < take; i++)
			{
				int picked = PickWeighted(candidates);
				if (picked < 0) break;

				result.Add(candidates[picked].relic);
				candidates.RemoveAt(picked);
			}

			return result;
		}

		private List<(RelicSO, float)> BuildCandidates()
		{
			List<(RelicSO, float)> list = new();
			if (_catalogue && _catalogue.Relics != null)
			{
				list = new List<(RelicSO, float)>(_catalogue.Relics.Count);
				foreach (RelicSO relic in _catalogue.Relics)
				{
					if (!relic)
						continue;

					float weight = GetWeight(relic.Rarity);
					if (weight > 0f)
						list.Add((relic, weight));
				}
			}

			if (list.Count != 0)
				return list;
			{
				RelicSO[] resources = Resources.LoadAll<RelicSO>("Relics");
				if (resources == null || resources.Length <= 0)
					return list;
				list = new List<(RelicSO, float)>(resources.Length);
				foreach (RelicSO relic in resources)
				{
					if (!relic)
						continue;

					float weight = GetWeight(relic.Rarity);
					if (weight > 0f)
						list.Add((relic, weight));
				}
			}

			return list;
		}

		private float GetWeight(RelicRarity rarity)
		{
			int index = (int)rarity;
			return index >= 0 && index < _rarityWeights.Length ? _rarityWeights[index] : 1f;
		}

		private static int PickWeighted(List<(RelicSO relic, float weight)> candidates)
		{
			if (candidates.Count == 0) return -1;

			float total = 0f;
			foreach ((_, float w) in candidates)
			{
				total += w;
			}

			float roll = Random.value * total;
			float cumulative = 0f;

			for (int i = 0; i < candidates.Count; i++)
			{
				cumulative += candidates[i].weight;
				if (roll <= cumulative)
					return i;
			}
			
			return candidates.Count - 1;
		}
	}
}
