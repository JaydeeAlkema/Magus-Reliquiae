using System;
using System.Collections.Generic;
using UnityEngine;

namespace Todo
{
	/// <summary>
	///     ScriptableObject container for editor-managed todo tasks.
	/// </summary>
	/// <remarks>
	///     Create one board asset from the Todo window, then add tasks through the asset or the editor tool.
	/// </remarks>
	[CreateAssetMenu(fileName = "TodoBoard", menuName = "ScriptableObjects/Todo/Board", order = 0)]
	public class TodoBoardSO : ScriptableObject
	{
		[SerializeField] private List<TodoTask> Tasks = new();

		/// <summary>
		///     Read-only access to all stored tasks.
		/// </summary>
		public IReadOnlyList<TodoTask> AllTasks => Tasks;

		/// <summary>
		///     Adds a task to the board.
		/// </summary>
		/// <param name="category">Optional grouping label.</param>
		/// <param name="name">Task name.</param>
		/// <param name="status">Initial task status.</param>
		/// <param name="description">Optional task description.</param>
		/// <returns>The created task.</returns>
		public TodoTask AddTask(string category, string name, TodoStatus status = TodoStatus.Todo, string description = "")
		{
			TodoTask task = new()
			{
				Category = category ?? string.Empty,
				Name = name ?? string.Empty,
				Description = description ?? string.Empty,
				Status = status,
			};

			Tasks.Add(task);
			return task;
		}

		/// <summary>
		///     Removes the first task that matches the provided name and optional category.
		/// </summary>
		/// <param name="name">Task name to remove.</param>
		/// <param name="category">Optional category filter.</param>
		/// <returns>True when a task was removed.</returns>
		public bool RemoveTask(string name, string category = null)
		{
			for (int i = 0; i < Tasks.Count; i++)
			{
				TodoTask task = Tasks[i];
				bool nameMatch = string.Equals(task.Name, name, StringComparison.OrdinalIgnoreCase);
				if (!nameMatch)
					continue;

				if (!string.IsNullOrWhiteSpace(category) &&
				    !string.Equals(task.Category, category, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				Tasks.RemoveAt(i);
				return true;
			}

			return false;
		}

		/// <summary>
		///     Updates the first matching task's status.
		/// </summary>
		/// <param name="name">Task name to update.</param>
		/// <param name="status">New status.</param>
		/// <param name="category">Optional category filter.</param>
		/// <returns>True when a task was updated.</returns>
		public bool TrySetStatus(string name, TodoStatus status, string category = null)
		{
			foreach (TodoTask task in Tasks)
			{
				bool nameMatch = string.Equals(task.Name, name, StringComparison.OrdinalIgnoreCase);
				if (!nameMatch)
					continue;

				if (!string.IsNullOrWhiteSpace(category) &&
				    !string.Equals(task.Category, category, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				task.Status = status;
				return true;
			}

			return false;
		}

		/// <summary>
		///     Counts tasks by status.
		/// </summary>
		/// <param name="status">Status to count.</param>
		/// <returns>Number of matching tasks.</returns>
		public int CountByStatus(TodoStatus status)
		{
			int count = 0;
			foreach (TodoTask task in Tasks)
			{
				if (task.Status == status)
					count++;
			}

			return count;
		}
	}
}
