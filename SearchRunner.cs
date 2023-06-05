using System.Text;

namespace Search;

public class SearchRunner
{
	int foundCount;
	string searchPath;

	public SearchRunner(string args) => Run(args);

	void Run(string args)
	{
		if (!string.IsNullOrWhiteSpace(args))
			ProcessInput(args);

		while (true)
		{
			//Executes given commands or gets the search path.
			while (string.IsNullOrWhiteSpace(searchPath))
			{
				Console.WriteLine("Enter command or search path (directory/folder):");
				ProcessInput(Console.ReadLine());
			}

			Search(ParseQuery(RequestQuery()));
		}
	}

	bool ProcessInput(string input)
	{
		searchPath = string.Empty;

		Argument[] arguments = ArgumentReader.Read(input);
		if (arguments.Length != 1 || arguments[0].Values.Count > 1)
			return false;
		else if (!arguments[0].NameIsValue)
		{
			ProcessCommand(arguments[0]);
			return true;
		}
		else if (!Directory.Exists(input))
		{
			Console.WriteLine("Invalid search path. Enter a new command or search path (directory/folder):");
			return false;
		}

		searchPath = arguments[0].Name;
		return true;
	}

	bool ProcessCommand(Argument argument)
	{
		switch (argument.Name)
		{
			case "quit":
			case "q":
				Environment.Exit(0);
				return true;
			case "help":
			case "h":
			case "?":
				PrintCommandHelp();
				return true;
			case "version":
			case "v":
				Console.WriteLine(Program.Version);
				return false;
			default:
				Console.WriteLine("Invalid command. Enter a new command or search path (directory/folder):");
				return false;
		}
	}

	void PrintCommandHelp()
	{
		Console.WriteLine("Commands:");
		Console.WriteLine("  -h, --help               Prints this help message");
		Console.WriteLine("  -v, --version,           Prints the current version");
		Console.WriteLine("  -r, --read,              Reads a search from a 'search.sr' file");
		Console.WriteLine("  -q, --quit               Exits the program");

		Console.WriteLine(string.Empty);
		PrintSearchHelp(true);
	}

	void PrintSearchHelp(bool fromCommandInput = false) => Console.WriteLine(@"Query input usage:
* Flags can be used in any order and can be mixed in with search terms
* When entering single letter flags after the first '-' symbol, you can enter multiple flags at once (without spaces)
* When entering full word flags after the first '--' symbol, you must enter each flag separately (with spaces)
* When entering search terms, you can enter multiple terms at once separated by spaces. If a term contains spaces, you must enclose it in double quotes

Query flags (usable only in the query input)
  -, --                    Return to the command and search path input
  -h, --help               Prints this help message
  -q, --quit               Exits the program after the search finishes
  -p, --print              Output the last search results
  -n, --silent,            Suppresses console output
---------------------------------------------------------------------------
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
  Note:                    The file extension is determined by the output format");

	string RequestQuery()
	{
		Console.WriteLine("Enter search options and terms separated by a space (use '-h' for help):");
		return Console.ReadLine();
	}

	Query ParseQuery(string queryText)
	{
		Argument[] args = ArgumentReader.Read(queryText);

		return null;
	}

	void PrintDirTree(List<string> dirTree)
	{
		foreach (string dir in dirTree)
			Console.WriteLine(dir);
		dirTree.Clear();
	}

	void Search(Query terms)
	{
		foundCount = 0;

		Console.WriteLine("Searching...");
		Find(searchPath, terms, 0, new());

		Console.WriteLine("");
		Console.WriteLine($"Found {foundCount} items");
	}

	void Find(string path, Query query, int level, List<string> dirTree)
	{
		int dirTreeCount = dirTree.Count;
		try
		{
			DirectoryInfo dir = new(path);

			if (!dir.Exists)
				return;

			string padding;
			StringBuilder sb = new();
			for (int i = 0; i < level; i++)
				sb.Append("   |");
			padding = sb.ToString();

			if (query.FullPath)
			{
				if (query.Match(dir.Name))
				{
					Console.WriteLine($"[DIR] {dir.FullName}");
					foundCount++;
				}
			}
			else
			{
				if (dirTree.Count == 0)
					dirTree.Add(padding);
				dirTree.Add($"{padding}---+{(query.Match(dir.Name) ? ">" : "")} {(query.FullPath ? dir.FullName : dir.Name)}");
			}

			if (!query.IgnoreFiles)
			{
				foreach (FileInfo file in dir.GetFiles())
				{
					if (query.Match(file.Name))
					{
						if (dirTree.Count > 0)
							PrintDirTree(dirTree);

						if (query.FullPath)
							Console.WriteLine(query.FullPath);
						else
							Console.WriteLine($"{padding}   |> {file.Name}");
						foundCount++;
					}
				}
			}

			foreach (DirectoryInfo directory in dir.GetDirectories())
				Find(directory.FullName, query, level + 1, dirTree);

			if (!query.FullPath && dirTree.Count > 1)
			{
				if (query.Match(dir.Name))
				{
					PrintDirTree(dirTree);
					foundCount++;
				}
				else
					dirTree.RemoveAt(dirTree.Count - 1);
			}
		}
		catch (Exception)
		{
			if (!query.FullPath && dirTree.Count > dirTreeCount)
				dirTree.RemoveAt(dirTree.Count - 1);
		}
	}
}