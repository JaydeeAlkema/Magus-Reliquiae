using System;
using UnityEngine;

namespace Todo
{
	/// <summary>
	/// Serializable todo item stored inside <see cref="TodoBoardSO"/>.
	/// </summary>
	[Serializable]
	public class TodoTask
	{
		/// <summary>
		/// Optional task category.
		/// </summary>
		public string Category;
		/// <summary>
		/// Task title.
		/// </summary>
		public string Name;
		/// <summary>
		/// Optional longer description.
		/// </summary>
		[TextArea(2, 6)] public string Description;
		/// <summary>
		/// Current task status.
		/// </summary>
		public TodoStatus Status;
	}
}
