using System.Text;

namespace Search;

public class SearchRunner
{
	int foundCount;
	string searchPath;

	public SearchRunner() => Run();

	void Run()
	{
		Terms terms;

		while (true)
		{
			//Executes given commands or gets the search path
			while (string.IsNullOrWhiteSpace(searchPath))
			{
				Console.WriteLine("Enter command or search path (directory/folder):");
				ProcessInput(Console.ReadLine());
			}

			terms = GetSearchTerms();
			Search(terms);
		}
	}

	bool ProcessInput(string input)
	{
		searchPath = string.Empty;

		if (input.StartsWith("-"))
			return ProcessCommand(input[1..]);
		else if (input.StartsWith("--"))
			return ProcessCommand(input[2..]);

		if (!Directory.Exists(input))
		{
			Console.WriteLine("Invalid search path. Enter a new command or search path (directory/folder):");
			return false;
		}

		searchPath = input;
		return true;
	}

	bool ProcessCommand(string command)
	{
		switch (command)
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

	void PrintSearchHelp(bool fromCommandInput = false)
	{
		Console.WriteLine("Term input usage:");
		Console.WriteLine("* Flags can be used in any order and can be mixed in with search terms");
		Console.WriteLine("* When entering single letter flags after the first '-' symbol, you can enter multiple flags at once (without spaces)");
		Console.WriteLine("* When entering full word flags after the first '--' symbol, you must enter each flag separately (with spaces)");
		Console.WriteLine("* When entering search terms, you can enter multiple terms at once separated by spaces");
		Console.WriteLine(string.Empty);

		Console.WriteLine("Modifiers (used at the start of a term word):");
		Console.WriteLine("  +                        Required term.    The term must be present in the search item");
		Console.WriteLine("  ?                        Optional term.    The term can be present in the search item");
		Console.WriteLine("  !                        Fail term.        The term must not be present in the search item");
		Console.WriteLine("  *                        Unique term.      The search item must be unique");
		Console.WriteLine("  \\                        Escape modifier.  Makes the modifier a part of the search term word");
		Console.WriteLine("Note:");
		Console.WriteLine("    * Only one modifier can be used per term");
		Console.WriteLine("    * Modifiers override flags");
		Console.WriteLine("    * If no modifier is used, the term is treated as a required term");
		Console.WriteLine("    * If multiple modifiers are used for a single term, the first one is used and the rest are treated as part of the search term word");
		Console.WriteLine("    * If the term consists of no words and a single modifier, the modifier is treated as a search term word");

		Console.WriteLine(string.Empty);

		Console.WriteLine($"Search flags{(fromCommandInput ? " (usable only in the term input)" : "")}:");
		Console.WriteLine("  -, --                    Return to the command and search path input");
		Console.WriteLine("  -h, --help               Prints this help message");
		Console.WriteLine("  -q, --quit               Exits the program");
		Console.WriteLine("  -n, --new                Starts a new search when the current one finishes");
		Console.WriteLine("----------------------------------------------------------------------------");
		Console.WriteLine("  -a, --any                Matches any of the given terms");
		Console.WriteLine("  -c, --matchCase          Matches terms with the same case");
		Console.WriteLine("  -d, --onlyDir            Only searches directories");
		Console.WriteLine("----------------------------------------------------------------------------");
		Console.WriteLine("  -r, --required,          Matches only if all terms are found");
		Console.WriteLine("  -o, --optional,          Matches if any terms are found");
		Console.WriteLine("  -x, --fail,              Matches if none of the terms are found");
		Console.WriteLine("  -u, --unique,            Matches if the search item is unique");
		Console.WriteLine("----------------------------------------------------------------------------");
		Console.WriteLine("  -t, --tree               Outputs the file/directory tree of the current search");
		Console.WriteLine("  -m, --markdown           Outputs the current search as markdown");
		Console.WriteLine("  -j, --json,              Outputs the current search as JSON");
		Console.WriteLine("  -s, --silent,            Suppresses console output");
		Console.WriteLine("----------------------------------------------------------------------------");
		Console.WriteLine("  -f, --filter,            Filter the currently loaded search");
		Console.WriteLine("  -w, --write,             Writes the current search result to a 'search.[extension]' file encoded in UTF-8");
		Console.WriteLine("  Note:                    The file extension is determined by the output format");

	}

	Terms GetSearchTerms()
	{
		Console.WriteLine("Enter search options and terms separated by a space (use '-h' for help):");

		return new(Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries));
	}

	void PrintDirTree(List<string> dirTree)
	{
		foreach (string dir in dirTree)
			Console.WriteLine(dir);
		dirTree.Clear();
	}

	void Search(Terms terms)
	{
		foundCount = 0;

		Console.WriteLine("Searching...");
		Find(searchPath, terms, 0, new());

		Console.WriteLine("");
		Console.WriteLine($"Found {foundCount} items");
	}

	void Find(string path, Terms terms, int level, List<string> dirTree)
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

			if (terms.FullPath)
			{
				if (terms.Match(dir.Name))
				{
					Console.WriteLine($"[DIR] {dir.FullName}");
					foundCount++;
				}
			}
			else
			{
				if (dirTree.Count == 0)
					dirTree.Add(padding);
				dirTree.Add($"{padding}---+{(terms.Match(dir.Name) ? ">" : "")} {(terms.FullPath ? dir.FullName : dir.Name)}");
			}

			if (!terms.OnlyDirectories)
			{
				foreach (FileInfo file in dir.GetFiles())
				{
					if (terms.Match(file.Name))
					{
						if (dirTree.Count > 0)
							PrintDirTree(dirTree);

						if (terms.FullPath)
							Console.WriteLine(terms.FullPath);
						else
							Console.WriteLine($"{padding}   |> {file.Name}");
						foundCount++;
					}
				}
			}

			foreach (DirectoryInfo directory in dir.GetDirectories())
				Find(directory.FullName, terms, level + 1, dirTree);

			if (!terms.FullPath && dirTree.Count > 1)
			{
				if (terms.Match(dir.Name))
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
			if (!terms.FullPath && dirTree.Count > dirTreeCount)
				dirTree.RemoveAt(dirTree.Count - 1);
		}
	}
}

class Terms
{
	List<string> requiredTerms;
	List<string> optionalTerms;
	List<string> failTerms;

	bool any, matchCase;
	public bool FullPath { get; }
	public bool OnlyDirectories { get; }

	public Terms(string[] terms)
	{
		if (terms.Length > 0 && terms[0].StartsWith("/"))
		{
			string[] options = terms[0].ToLower().Split('/', StringSplitOptions.RemoveEmptyEntries);
			foreach (string option in options)
			{
				if (option == "any" || option == "a")
					any = true;
				if (option == "matchcase" || option == "c")
					matchCase = true;
				if (option == "fullpath" || option == "p")
					FullPath = true;
				if (option == "onlyDir" || option == "d")
					OnlyDirectories = true;
			}
			terms = terms.Take(new Range(1, terms.Length)).ToArray();
		}

		requiredTerms = new();
		optionalTerms = new();
		failTerms = new();

		for (int i = 0; i < terms.Length; i++)
		{
			if (!matchCase)
				terms[i] = terms[i].ToLower();

			if (terms[i].StartsWith("/"))
			{
				if (any)
					optionalTerms.Add(terms[i][1..]);
				else
					requiredTerms.Add(terms[i][1..]);
			}
			else
			{
				if (terms[i].StartsWith("+"))
					requiredTerms.Add(terms[i][1..]);
				else if (terms[i].StartsWith("*"))
					optionalTerms.Add(terms[i][1..]);
				else if (terms[i].StartsWith("!"))
					failTerms.Add(terms[i][1..]);
				else if (any)
					optionalTerms.Add(terms[i]);
				else
					requiredTerms.Add(terms[i]);
			}
		}
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
}