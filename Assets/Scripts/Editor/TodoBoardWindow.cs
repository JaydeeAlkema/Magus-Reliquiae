using System;
using System.Collections.Generic;
using Todo;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	///     Editor window for managing <see cref="TodoBoardSO" /> assets.
	/// </summary>
	/// <remarks>
	///     Open it from <c>Tools/Todo Board</c> to create boards, add tasks, and review task status.
	/// </remarks>
	public class TodoBoardWindow : EditorWindow
	{
		private const float CATEGORY_COLUMN_WIDTH = 170f;
		private const float STATUS_COLUMN_WIDTH = 110f;
		private const float DELETE_COLUMN_WIDTH = 28f;
		private const float TASK_BLOCK_INDENT = 14f;
		private const float DESCRIPTION_INDENT = 12f;
		private const float TASK_SPACER = 4f;
		private const float CATEGORY_SPACER = 8f;

		private enum Tab
		{
			Add = 0,
			View = 1,
		}

		private enum SortBy
		{
			Category = 0,
			Name = 1,
			Status = 2,
		}

		private sealed class TaskRow
		{
			public int Index;
			public string Category;
			public string Name;
			public string Description;
			public TodoStatus Status;
		}

		private TodoBoardSO _board;
		private Vector2 _addScroll;
		private Vector2 _viewScroll;
		private string _newCategory = string.Empty;
		private string _newName = string.Empty;
		private string _newDescription = string.Empty;
		private TodoStatus _newStatus = TodoStatus.Todo;
		private Tab _activeTab;
		private SortBy _sortBy = SortBy.Category;
		private bool _sortAscending = true;
		private readonly Dictionary<string, bool> _categoryFoldouts = new();
		private readonly Dictionary<string, bool> _taskFoldouts = new();

		/// <summary>
		///     Opens the todo board window.
		/// </summary>
		[MenuItem("Tools/Todo Board")]
		public static void Open()
		{
			GetWindow<TodoBoardWindow>("Todo Board");
		}

		/// <summary>
		///     Draws the window contents.
		/// </summary>
		private void OnGUI()
		{
			DrawToolbar();
			EditorGUILayout.Space(6f);

			if (!_board)
			{
				EditorGUILayout.HelpBox("Assign or create a TodoBoardSO asset.", MessageType.Info);
				return;
			}

			DrawSummary();
			EditorGUILayout.Space(6f);
			DrawTabs();

			if (_activeTab == Tab.Add)
			{
				DrawAddTab();
				return;
			}

			DrawViewTab();
		}

		private void DrawToolbar()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				TodoBoardSO board = (TodoBoardSO)EditorGUILayout.ObjectField(
					_board,
					typeof(TodoBoardSO),
					false,
					GUILayout.MinWidth(250f));

				if (board && board != _board)
					_board = board;

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Create", EditorStyles.toolbarButton, GUILayout.Width(60f)))
					CreateBoardAsset();
			}
		}

		private void DrawSummary()
		{
			int todo = _board.CountByStatus(TodoStatus.Todo);
			int inProgress = _board.CountByStatus(TodoStatus.InProgress);
			int done = _board.CountByStatus(TodoStatus.Done);
			int blocked = _board.CountByStatus(TodoStatus.Blocked);

			EditorGUILayout.LabelField(
				$"Todo: {todo}   InProgress: {inProgress}   Done: {done}   Blocked: {blocked}",
				EditorStyles.helpBox);
		}

		private void DrawTabs()
		{
			_activeTab = (Tab)GUILayout.Toolbar((int)_activeTab, new[]
			{
				"Add Tasks", "View Tasks",
			});
			EditorGUILayout.Space(6f);
		}

		private void DrawAddTab()
		{
			EditorGUILayout.LabelField("Add Task", EditorStyles.boldLabel);

			using (new EditorGUILayout.HorizontalScope())
			{
				_newCategory = EditorGUILayout.TextField(_newCategory, GUILayout.Width(CATEGORY_COLUMN_WIDTH));
				_newName = EditorGUILayout.TextField(_newName);
				_newStatus = (TodoStatus)EditorGUILayout.EnumPopup(_newStatus, GUILayout.Width(STATUS_COLUMN_WIDTH));

				using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_newName)))
				{
					if (GUILayout.Button("+", GUILayout.Width(DELETE_COLUMN_WIDTH)))
					{
						Undo.RecordObject(_board, "Add Todo Task");
						_board.AddTask(_newCategory.Trim(), _newName.Trim(), _newStatus, _newDescription.Trim());
						EditorUtility.SetDirty(_board);

						_newName = string.Empty;
						_newCategory = string.Empty;
						_newDescription = string.Empty;
						_newStatus = TodoStatus.Todo;
					}
				}
			}

			EditorGUILayout.LabelField("Description");
			_newDescription = EditorGUILayout.TextArea(_newDescription, GUILayout.MinHeight(54f));

			EditorGUILayout.Space(6f);
			EditorGUILayout.LabelField("Existing Tasks (full edit)", EditorStyles.boldLabel);
			DrawEditableTaskList();
		}

		private void DrawEditableTaskList()
		{
			SerializedObject serializedBoard = new(_board);
			SerializedProperty tasks = serializedBoard.FindProperty("Tasks");

			DrawAddHeader();

			_addScroll = EditorGUILayout.BeginScrollView(_addScroll);
			for (int i = 0; i < tasks.arraySize; i++)
			{
				SerializedProperty task = tasks.GetArrayElementAtIndex(i);
				SerializedProperty category = task.FindPropertyRelative("Category");
				SerializedProperty name = task.FindPropertyRelative("Name");
				SerializedProperty description = task.FindPropertyRelative("Description");
				SerializedProperty status = task.FindPropertyRelative("Status");

				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.PropertyField(category, GUIContent.none, GUILayout.Width(CATEGORY_COLUMN_WIDTH));
					EditorGUILayout.PropertyField(name, GUIContent.none);
					EditorGUILayout.PropertyField(status, GUIContent.none, GUILayout.Width(STATUS_COLUMN_WIDTH));

					if (GUILayout.Button("-", GUILayout.Width(DELETE_COLUMN_WIDTH)))
					{
						tasks.DeleteArrayElementAtIndex(i);
						break;
					}
				}

				string taskFoldoutKey = $"add::{category.stringValue}::{name.stringValue}::{i}";
				bool expanded = GetTaskExpanded(taskFoldoutKey);
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.Space(TASK_BLOCK_INDENT);
					expanded = EditorGUILayout.Foldout(expanded, "Description", true);
				}

				_taskFoldouts[taskFoldoutKey] = expanded;

				if (expanded)
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.Space(TASK_BLOCK_INDENT + DESCRIPTION_INDENT);
						EditorGUILayout.PropertyField(description, GUIContent.none);
					}
				}

				EditorGUILayout.Space(TASK_SPACER);
			}

			EditorGUILayout.EndScrollView();

			if (serializedBoard.ApplyModifiedProperties())
				EditorUtility.SetDirty(_board);
		}

		private static void DrawAddHeader()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Category", EditorStyles.boldLabel, GUILayout.Width(CATEGORY_COLUMN_WIDTH));
				EditorGUILayout.LabelField("Name", EditorStyles.boldLabel);
				EditorGUILayout.LabelField("Status", EditorStyles.boldLabel, GUILayout.Width(STATUS_COLUMN_WIDTH));
				GUILayout.Space(DELETE_COLUMN_WIDTH);
			}
		}

		private void DrawViewTab()
		{
			DrawViewControls();
			EditorGUILayout.Space(4f);
			DrawViewHeader();
			DrawGroupedView();
		}

		private void DrawViewControls()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Sort", GUILayout.Width(35f));
				_sortBy = (SortBy)EditorGUILayout.EnumPopup(_sortBy, GUILayout.Width(120f));
				_sortAscending = EditorGUILayout.ToggleLeft("Ascending", _sortAscending, GUILayout.Width(90f));
				GUILayout.FlexibleSpace();
			}
		}

		private static void DrawViewHeader()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Task", EditorStyles.boldLabel);
				EditorGUILayout.LabelField("Status", EditorStyles.boldLabel, GUILayout.Width(STATUS_COLUMN_WIDTH));
			}
		}

		private void DrawGroupedView()
		{
			SerializedObject serializedBoard = new(_board);
			SerializedProperty tasks = serializedBoard.FindProperty("Tasks");
			List<TaskRow> rows = BuildSortedRows(tasks);

			_viewScroll = EditorGUILayout.BeginScrollView(_viewScroll);

			string currentCategory = null;
			bool isFirstCategory = true;
			foreach (TaskRow row in rows)
			{
				if (!string.Equals(currentCategory, row.Category, StringComparison.Ordinal))
				{
					if (!isFirstCategory)
						EditorGUILayout.Space(CATEGORY_SPACER);

					currentCategory = row.Category;
					bool categoryExpanded = GetCategoryExpanded(currentCategory);
					categoryExpanded = EditorGUILayout.Foldout(categoryExpanded, currentCategory, true);
					_categoryFoldouts[currentCategory] = categoryExpanded;
					isFirstCategory = false;
				}

				if (currentCategory != null && !_categoryFoldouts[currentCategory])
					continue;

				SerializedProperty task = tasks.GetArrayElementAtIndex(row.Index);
				SerializedProperty status = task.FindPropertyRelative("Status");

				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.Space(TASK_BLOCK_INDENT);

					using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							EditorGUILayout.LabelField(row.Name);
							EditorGUI.BeginChangeCheck();
							TodoStatus nextStatus = (TodoStatus)EditorGUILayout.EnumPopup((TodoStatus)status.enumValueIndex, GUILayout.Width(STATUS_COLUMN_WIDTH));
							if (EditorGUI.EndChangeCheck())
							{
								status.enumValueIndex = (int)nextStatus;
							}
						}

						string taskFoldoutKey = $"view::{row.Category}::{row.Name}::{row.Index}";
						bool taskExpanded = GetTaskExpanded(taskFoldoutKey);
						using (new EditorGUILayout.HorizontalScope())
						{
							GUILayout.Space(DESCRIPTION_INDENT);
							taskExpanded = EditorGUILayout.Foldout(taskExpanded, "Description", true);
						}

						_taskFoldouts[taskFoldoutKey] = taskExpanded;

						if (taskExpanded)
						{
							using (new EditorGUILayout.HorizontalScope())
							{
								GUILayout.Space(DESCRIPTION_INDENT * 2f);
								using (new EditorGUI.DisabledScope(true))
								{
									string descriptionText = string.IsNullOrWhiteSpace(row.Description) ? "(No description)" : row.Description;
									EditorGUILayout.TextArea(descriptionText, GUILayout.MinHeight(44f));
								}
							}
						}
					}
				}

				EditorGUILayout.Space(TASK_SPACER);
			}

			EditorGUILayout.EndScrollView();

			if (serializedBoard.ApplyModifiedProperties())
				EditorUtility.SetDirty(_board);
		}

		private List<TaskRow> BuildSortedRows(SerializedProperty tasks)
		{
			List<TaskRow> rows = new(tasks.arraySize);
			for (int i = 0; i < tasks.arraySize; i++)
			{
				SerializedProperty task = tasks.GetArrayElementAtIndex(i);
				string category = task.FindPropertyRelative("Category").stringValue;
				string taskName = task.FindPropertyRelative("Name").stringValue;
				string description = task.FindPropertyRelative("Description").stringValue;
				TodoStatus status = (TodoStatus)task.FindPropertyRelative("Status").enumValueIndex;

				rows.Add(new TaskRow
				{
					Index = i,
					Category = string.IsNullOrWhiteSpace(category) ? "Uncategorized" : category,
					Name = string.IsNullOrWhiteSpace(taskName) ? "(Unnamed task)" : taskName,
					Description = description ?? string.Empty,
					Status = status,
				});
			}

			rows.Sort(CompareRows);
			return rows;
		}

		private int CompareRows(TaskRow a, TaskRow b)
		{
			int primary = CompareByMode(a, b);
			if (primary != 0)
				return _sortAscending ? primary : -primary;

			primary = string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase);

			if (primary != 0)
				return _sortAscending ? primary : -primary;

			primary = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);

			return _sortAscending ? primary : -primary;
		}

		private int CompareByMode(TaskRow a, TaskRow b)
		{
			return _sortBy switch
			{
				SortBy.Name => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase),
				SortBy.Status => a.Status.CompareTo(b.Status),
				_ => string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase),
			};
		}

		private bool GetCategoryExpanded(string category)
		{
			if (_categoryFoldouts.TryGetValue(category, out bool expanded))
				return expanded;

			_categoryFoldouts[category] = true;
			return true;
		}

		private bool GetTaskExpanded(string key)
		{
			if (_taskFoldouts.TryGetValue(key, out bool expanded))
				return expanded;

			_taskFoldouts[key] = false;
			return false;
		}

		/// <summary>
		///     Creates a new todo board asset in the project.
		/// </summary>
		private void CreateBoardAsset()
		{
			string path = EditorUtility.SaveFilePanelInProject(
				"Create Todo Board",
				"TodoBoard",
				"asset",
				"Choose where to create the todo board asset.");

			if (string.IsNullOrWhiteSpace(path))
				return;

			TodoBoardSO board = CreateInstance<TodoBoardSO>();
			AssetDatabase.CreateAsset(board, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			_board = board;
			Selection.activeObject = board;
			EditorGUIUtility.PingObject(board);
		}
	}
}
