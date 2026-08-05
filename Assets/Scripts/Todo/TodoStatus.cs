namespace Todo
{
	/// <summary>
	///     Workflow state for a todo item.
	/// </summary>
	public enum TodoStatus
	{
		/// <summary>
		///     Not started.
		/// </summary>
		Todo = 0,
		/// <summary>
		///     Currently being worked on.
		/// </summary>
		InProgress = 1,
		/// <summary>
		///     Finished.
		/// </summary>
		Done = 2,
		/// <summary>
		///     Blocked by an issue.
		/// </summary>
		Blocked = 3,
	}
}
