using Todo;
using UnityEditor;
using UnityEngine;

public class TodoBoardWindow : EditorWindow
{
	private const float CategoryColumnWidth = 170f;
	private const float StatusColumnWidth = 110f;
	private const float DeleteColumnWidth = 28f;

	private TodoBoardSO _board;
	private Vector2 _scroll;
	private string _newCategory = string.Empty;
	private string _newName = string.Empty;
	private TodoStatus _newStatus = TodoStatus.Todo;

	[MenuItem("Tools/Todo Board")]
	public static void Open()
	{
		GetWindow<TodoBoardWindow>("Todo Board");
	}

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
		DrawHeader();
		DrawTasks();
		EditorGUILayout.Space(6f);
		DrawAddTask();
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

			if (board != _board)
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

	private static void DrawHeader()
	{
		using (new EditorGUILayout.HorizontalScope())
		{
			EditorGUILayout.LabelField("Category", EditorStyles.boldLabel, GUILayout.Width(CategoryColumnWidth));
			EditorGUILayout.LabelField("Name", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Status", EditorStyles.boldLabel, GUILayout.Width(StatusColumnWidth));
			GUILayout.Space(DeleteColumnWidth);
		}
	}

	private void DrawTasks()
	{
		SerializedObject serializedBoard = new(_board);
		SerializedProperty tasks = serializedBoard.FindProperty("Tasks");

		_scroll = EditorGUILayout.BeginScrollView(_scroll);
		for (int i = 0; i < tasks.arraySize; i++)
		{
			SerializedProperty task = tasks.GetArrayElementAtIndex(i);
			SerializedProperty category = task.FindPropertyRelative("Category");
			SerializedProperty name = task.FindPropertyRelative("Name");
			SerializedProperty status = task.FindPropertyRelative("Status");

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(category, GUIContent.none, GUILayout.Width(CategoryColumnWidth));
				EditorGUILayout.PropertyField(name, GUIContent.none);
				EditorGUILayout.PropertyField(status, GUIContent.none, GUILayout.Width(StatusColumnWidth));

				if (GUILayout.Button("-", GUILayout.Width(DeleteColumnWidth)))
				{
					tasks.DeleteArrayElementAtIndex(i);
					break;
				}
			}
		}

		EditorGUILayout.EndScrollView();

		if (serializedBoard.ApplyModifiedProperties())
			EditorUtility.SetDirty(_board);
	}

	private void DrawAddTask()
	{
		EditorGUILayout.LabelField("Add Task", EditorStyles.boldLabel);

		using (new EditorGUILayout.HorizontalScope())
		{
			_newCategory = EditorGUILayout.TextField(_newCategory, GUILayout.Width(CategoryColumnWidth));
			_newName = EditorGUILayout.TextField(_newName);
			_newStatus = (TodoStatus)EditorGUILayout.EnumPopup(_newStatus, GUILayout.Width(StatusColumnWidth));

			using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_newName)))
			{
				if (GUILayout.Button("+", GUILayout.Width(DeleteColumnWidth)))
				{
					Undo.RecordObject(_board, "Add Todo Task");
					_board.AddTask(_newCategory.Trim(), _newName.Trim(), _newStatus);
					EditorUtility.SetDirty(_board);

					_newName = string.Empty;
					_newCategory = string.Empty;
					_newStatus = TodoStatus.Todo;
				}
			}
		}
	}

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
