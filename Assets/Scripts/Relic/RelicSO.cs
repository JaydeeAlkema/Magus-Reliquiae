using System;
using System.Collections.Generic;
using UnityEngine;

namespace Relic
{
	[CreateAssetMenu(fileName = "Relic", menuName = "ScriptableObjects/Relic/Relic", order = 0)]
	public class RelicSO : ScriptableObject
	{
		public string Id;
		public string DisplayName;
		[TextArea(2, 6)] public string Description;
		public Sprite Icon;
		public RelicRarity Rarity = RelicRarity.Common;
		[Min(1)] public int MaxLevel = 1;
		public string[] Tags = Array.Empty<string>();
		public List<RelicLevelData> Levels = new();

		public readonly RelicShape[] ShapePerLevel = Array.Empty<RelicShape>();

		public RelicLevelData GetLevelData(int level)
		{
			if (Levels.Count == 0)
				return null;

			int index = Mathf.Clamp(level - 1, 0, Levels.Count - 1);
			return Levels[index];
		}

		public RelicShape GetShape(int level)
		{
			if (ShapePerLevel == null || ShapePerLevel.Length == 0)
				return RelicShape.Default;

			int index = Mathf.Clamp(level - 1, 0, ShapePerLevel.Length - 1);
			RelicShape shape = ShapePerLevel[index];
			return shape is { IsValid: true } ? shape : RelicShape.Default;
		}
	}
}
