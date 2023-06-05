using System.Text;

namespace Search;

public static class ArgumentReader
{
	public static Argument[] Read(string args, bool noValues = false)
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

		return BuildArguments(arguments.ToArray(), noValues);
	}

	private static Argument[] BuildArguments(IEnumerable<string> argumentStrings, bool noValues)
	{
		List<Argument> arguments = new();
		Argument? currentArgument = null;

		foreach (string argText in argumentStrings)
		{
			if (argText.StartsWith("--"))
			{
				currentArgument = new(argText[2..], false);
				arguments.Add(currentArgument);
			}
			if (argText.StartsWith("-"))
			{
				string multiflag = argText[1..];
				if (multiflag.Length > 1)
				{
					foreach (char flag in multiflag)
						arguments.Add(new(flag.ToString(), false));
					continue;
				}

				currentArgument = new(multiflag, false);
				arguments.Add(currentArgument);
			}
			else if (currentArgument == null || noValues)
				arguments.Add(new(argText, true));
			else
				currentArgument.Values.Add(argText);
		}

		return arguments.ToArray();
	}
}