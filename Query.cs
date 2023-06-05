namespace Search;

public class Query
{
	QueryCommandFlags Commands { get; }
	OutputType OutputType { get; } = OutputType.List;
	SubQuery[] SubQueries { get; }

	Query(SubQuery[] subQueries, QueryCommandFlags commands, OutputType outputType)
	{
		SubQueries = subQueries;
		Commands = commands;
		OutputType = outputType;
	}

	public bool Match(string text)
	{
		if (!matchCase)
			text = text.ToLower();

		//Fail (any)
		foreach (string term in failTerms)
			if (text.Contains(term))
				return false;

		//Optional (any)
		bool contains = optionalTerms.Count == 0;
		foreach (string term in optionalTerms)
		{
			if (text.Contains(term))
			{
				contains = true;
				break;
			}
		}

		//Required (all)
		foreach (string term in requiredTerms)
			if (!text.Contains(term))
				return false;

		return contains;
	}


	public static Query? Parse(string query)
	{
		Argument[] args = ArgumentReader.Read(query, true);
		if (args.Length == 0)
			throw new ArgumentException("Query is empty.");

		OutputType outputType = OutputType.List;
		QueryCommandFlags commands = 0;
		List<SubQuery> subQueries = new();

		List<string> terms = new();
		SubQueryFlags flags = 0;

		foreach (Argument arg in args)
		{
			OutputType newOutputType = MatchOutputType(arg.Name);
			if (outputType != OutputType.List && newOutputType != OutputType.List)
				throw new ArgumentException("Output type can only be set once.");
			outputType = newOutputType;

			QueryCommandFlags addCommands = MatchCommand(arg.Name);

			SubQueryFlags addFlags = MatchSubQueryFlag(arg.Name);
			if (addFlags != 0)
			{
				commands |= addCommands;

				subQueries.Add(new SubQuery(terms.ToArray(), flags));
				terms.Clear();
				flags = addFlags;
				continue;
			}

			if (addCommands != 0)
			{
				commands |= addCommands;
				continue;
			}

			if (!arg.NameIsValue)
				throw new ArgumentException($"Invalid argument: {arg.Name}");

			terms.Add(arg.Name);
		}

		Query outputQuery = new(subQueries.ToArray(), commands, outputType);
		Validate(outputQuery);
		return outputQuery;
	}

	#region StringEnumMatching
	private static QueryCommandFlags MatchCommand(string arg) => arg.ToLower() switch
	{
		"" => QueryCommandFlags.Return,
		"help" or "h" => QueryCommandFlags.Help,
		"quit" or "q" => QueryCommandFlags.Quit,
		"print" or "p" => QueryCommandFlags.Print,
		"silent" or "n" => QueryCommandFlags.Silent,
		"filter" or "f" => QueryCommandFlags.Filter,
		"load" or "l" => QueryCommandFlags.Load,
		"save" or "s" => QueryCommandFlags.Save,
		"write" or "w" => QueryCommandFlags.Write,
		_ => QueryCommandFlags.None
	};

	private static SubQueryFlags MatchSubQueryFlag(string arg) => arg.ToLower() switch
	{
		"required" or "r" => SubQueryFlags.Required,
		"optional" or "o" => SubQueryFlags.Optional,
		"fail" or "f" => SubQueryFlags.Fail,
		"unique" or "u" => SubQueryFlags.Unique,
		"matchcase" or "m" => SubQueryFlags.MatchCase,
		"onlydir" or "d" => SubQueryFlags.OnlyDir,
		_ => SubQueryFlags.None
	};

	private static OutputType MatchOutputType(string arg) => arg.ToLower() switch
	{
		"tree" or "t" => OutputType.Tree,
		"markdown" or "m" => OutputType.Markdown,
		"json" or "j" => OutputType.Json,
		_ => OutputType.List
	};
	#endregion

	#region Validation
	static readonly QueryCommandFlags[] ExclusiveCommands = new[]
	{
		QueryCommandFlags.Return,
		QueryCommandFlags.Help
	};

	static readonly QueryCommandFlags[] NoTermCommands = new[]
	{
		QueryCommandFlags.Print
	};

	static readonly QueryCommandFlags[] RequireTermCommands = new[]
	{
		QueryCommandFlags.Filter,
	};

	static void Validate(Query query)
	{
		//Exclusive commands
		foreach (QueryCommandFlags command in ExclusiveCommands)
			if (!IsExclusiveCommand(query, command))
				throw new ArgumentException($"The '{command}' command must be used exclusively.");

		//No term commands
		foreach (QueryCommandFlags command in NoTermCommands)
			if (query.SubQueries.Length > 0)
				throw new ArgumentException($"The '{command}' command cannot be used with search terms.");

		//Require term commands
		foreach (QueryCommandFlags command in RequireTermCommands)
			if (query.SubQueries.Length == 0)
				throw new ArgumentException($"The '{command}' command requires search terms.");

		//Subquery validation
		foreach (SubQuery subQuery in query.SubQueries)
			subQuery.Validate();
	}

	static bool IsExclusiveCommand(Query query, QueryCommandFlags command)
	{
		if (!Enum.GetValues<QueryCommandFlags>().Any(x => x == command))
			throw new ArgumentException($"Invalid check command value: {(int)command}");

		return query.SubQueries.Length == 0 && query.Commands == command;
	}
	#endregion
}

/*
Query input usage:
* Flags can be used in any order and can be mixed in with search terms
* When entering single letter flags after the first '-' symbol, you can enter multiple flags at once (without spaces)
* When entering full word flags after the first '--' symbol, you must enter each flag separately (with spaces)
* When entering search terms, you can enter multiple terms at once separated by spaces

Search flags (usable only in the term input)
  -, --                    [Exclusive] Return to the command and search path input
  -h, --help               [Exclusive] Prints this help message
  -q, --quit               Exits the program after the search finishes
  -p, --print              Output the last search results
  -n, --silent,            Suppresses console output
  -a, --all,               Gets the entire file/directory tree
----------------------------------------------------------------------------
  -r, --required,          Matches only if all terms are found
  -o, --optional,          Matches if any terms without modifiers are found
  -x, --fail,              Matches if none of the terms without modifiers are found
  -u, --unique,            Matches if the search item is unique
  -c, --matchCase          Matches terms with the same case
  -d, --onlyDir            Only searches directories
----------------------------------------------------------------------------
  -t, --tree               Outputs the file/directory tree of the current search
  -m, --markdown           Outputs the current search as markdown
  -j, --json,              Outputs the current search as JSON
----------------------------------------------------------------------------
  -f, --filter,            Filter the currently loaded search
  -l, --load,          	  Loads a search query from a 'query.sr' file. Any additional terms are added to the loaded query
  -s, --save,              Saves the current search query to a 'query.sr' file without executing it
  -w, --write,             Writes the current search result to a 'search.[extension]' file encoded in UTF-8
  Note:                    The file extension is determined by the output format
*/