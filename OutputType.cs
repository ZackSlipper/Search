namespace Search;

public enum OutputType
{
	/// <summary>
	/// List of file and directory paths.
	/// </summary>
	List,

	/// <summary>
	/// File and directory tree.
	/// </summary>
	Tree,

	/// <summary>
	/// File and directory tree in the Markdown format.
	/// </summary>
	Markdown,

	/// <summary>
	/// File and directory tree in the JSON format.
	/// </summary>
	JSON
}