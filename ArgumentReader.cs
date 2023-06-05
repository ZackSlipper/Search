using System.Text;

namespace Search;

public static class ArgumentReader
{
	public static Argument[] Read(string args)
	{
		if (string.IsNullOrWhiteSpace(args))
			return Array.Empty<Argument>();

		List<string> arguments = new();
		StringBuilder sb = new();

		bool inQuotes = false;
		bool escaped = false;

		foreach (char c in args)
		{
			if (c == '\\')
			{
				if (escaped)
				{
					sb.Append(c);
					escaped = false;
				}
				else
					escaped = true;
			}
			else if (c == '"')
			{
				if (escaped)
				{
					sb.Append(c);
					escaped = false;
				}
				else
					inQuotes = !inQuotes;
			}
			else if (c == ' ' && !inQuotes)
			{
				if (escaped)
				{
					sb.Append(c);
					escaped = false;
				}
				else if (sb.Length > 0)
				{
					arguments.Add(sb.ToString());
					sb.Clear();
				}
			}
			else
			{
				sb.Append(c);
				escaped = false;
			}
		}

		if (sb.Length > 0)
			arguments.Add(sb.ToString());

		return BuildArguments(arguments.ToArray());
	}

	public static Argument[] BuildArguments(IEnumerable<string> argumentStrings)
	{
		List<Argument> arguments = new();
		Argument? currentArgument = null;

		foreach (string argText in argumentStrings)
		{
			if (argText.StartsWithAny("--", "-"))
			{
				if (currentArgument != null)
					arguments.Add(currentArgument);

				currentArgument = new(argText[1..], false);
			}
			else if (currentArgument != null)
				currentArgument.Values.Add(argText);
			else
				arguments.Add(new(argText, true));
		}

		return arguments.ToArray();
	}
}