using System;

namespace Todo
{
	[Serializable]
	public class TodoTask
	{
		public string Category;
		public string Name;
		public TodoStatus Status;
	}
}
