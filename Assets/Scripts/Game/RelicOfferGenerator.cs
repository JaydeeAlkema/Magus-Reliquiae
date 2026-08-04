using System;
using System.Collections.Generic;
using Relic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
	/// <summary>
	/// Builds weighted relic offer lists for the upgrade screen.
	/// </summary>
	/// <remarks>
	/// Feed it a catalogue and optional rarity weights, then call <see cref="GenerateOffers"/> when the
	/// upgrade screen opens.
	/// </remarks>
	public sealed class RelicOfferGenerator
	{
		/// <summary>
		/// Default rarity weights used when no custom table is provided.
		/// </summary>
		public static readonly float[] DefaultWeights =
		{
			60f, 25f, 12f, 3f,
		};

		private readonly RelicCatalogueSO _catalogue;
		private readonly float[] _rarityWeights;

		/// <summary>
		/// Creates a relic offer generator.
		/// </summary>
		/// <param name="catalogue">Catalogue used as the source of relics.</param>
		/// <param name="rarityWeights">Optional rarity weight table.</param>
		public RelicOfferGenerator(RelicCatalogueSO catalogue, float[] rarityWeights = null)
		{
			_catalogue = catalogue;
			_rarityWeights = rarityWeights ?? DefaultWeights;
		}

		/// <summary>
		/// Generates a list of unique relic offers.
		/// </summary>
		/// <param name="count">Maximum number of offers to return.</param>
		/// <returns>Weighted, non-duplicate relic offers.</returns>
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

		/// <summary>
		/// Builds the candidate pool from the catalogue or Resources fallback.
		/// </summary>
		/// <returns>Candidate relics and their weights.</returns>
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
					if (HasTag(relic, "starting"))
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
					if (HasTag(relic, "starting"))
						continue;

					float weight = GetWeight(relic.Rarity);
					if (weight > 0f)
						list.Add((relic, weight));
				}
			}

			return list;
		}

		/// <summary>
		/// Resolves the weight for a given rarity.
		/// </summary>
		/// <param name="rarity">Rarity to look up.</param>
		/// <returns>Configured rarity weight or a fallback value.</returns>
		private float GetWeight(RelicRarity rarity)
		{
			int index = (int)rarity;
			return index >= 0 && index < _rarityWeights.Length ? _rarityWeights[index] : 1f;
		}

		/// <summary>
		/// Checks whether a relic contains a tag.
		/// </summary>
		/// <param name="relic">Relic to inspect.</param>
		/// <param name="tag">Tag name to search for.</param>
		/// <returns>True when the tag is present.</returns>
		private static bool HasTag(RelicSO relic, string tag)
		{
			if (relic == null || relic.Tags == null || string.IsNullOrWhiteSpace(tag))
				return false;

			foreach (string entry in relic.Tags)
			{
				if (string.Equals(entry, tag, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Randomly picks one candidate using weighted selection.
		/// </summary>
		/// <param name="candidates">Candidate list.</param>
		/// <returns>The picked index, or -1 when the list is empty.</returns>
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
