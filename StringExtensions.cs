namespace Search;

public static class StringExtensions
{
	/// <summary>
	/// Returns true if the string starts with any of the given values.
	/// </summary>
	public static bool StartsWithAny(this string str, params string[] values)
	{
		foreach (string value in values)
			if (str.StartsWith(value))
				return true;

		return false;
	}

	/// <summary>
	/// Returns the index of the first value the string starts with, or -1 if none of the values match.
	/// </summary>
	public static int StartsWithAnyIndex(this string str, params string[] values)
	{
		for (int i = 0; i < values.Length; i++)
			if (str.StartsWith(values[i]))
				return i;
		return -1;
	}
}