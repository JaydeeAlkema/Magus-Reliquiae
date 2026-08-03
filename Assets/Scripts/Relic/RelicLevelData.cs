using System;
using System.Collections.Generic;
using UnityEngine;

namespace Relic
{
	[Serializable]
	public class RelicLevelData
	{
		[TextArea(2, 6)] public string Description;
		public List<RelicStatModifier> StatModifiers = new();
	}
}
