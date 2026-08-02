using System;
using UnityEngine;

namespace Todo
{
	[Serializable]
	public class TodoTask
	{
		public string Category;
		public string Name;
		[TextArea(2, 6)] public string Description;
		public TodoStatus Status;
	}
}
