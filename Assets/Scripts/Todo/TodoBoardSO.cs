using System;
using System.Collections.Generic;
using UnityEngine;

namespace Todo
{
	[CreateAssetMenu(fileName = "TodoBoard", menuName = "ScriptableObjects/Todo/Board", order = 0)]
	public class TodoBoardSO : ScriptableObject
	{
		[SerializeField] private List<TodoTask> Tasks = new();

		public IReadOnlyList<TodoTask> AllTasks => Tasks;

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
