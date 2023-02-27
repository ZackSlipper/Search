using System.ComponentModel;
using System.Text;

namespace Search
{
    public class SearchRunner
    {
        int foundCount;

        public SearchRunner()
        {
            Run();
        }

        void Run()
        {
            string path = null;
            Terms terms;

            while (true)
            {
                if (string.IsNullOrWhiteSpace(path))
                    path = GetSearchPath();
                terms = GetSearchTerms();

                Console.WriteLine("Searching...");

                foundCount = 0;
                Find(path, terms, 0, new());

                Console.WriteLine("");
                Console.WriteLine($"Found {foundCount} items");
            }
        }

        string GetSearchPath()
        {
            Console.WriteLine("Enter search path (directory/folder):");

            string path = Console.ReadLine();
            while (!Directory.Exists(path))
            {
                Console.WriteLine("Invalid path. Enter a new search path (directory/folder):");
                path = Console.ReadLine();
            }
            return path;
        }

        Terms GetSearchTerms()
        {
            Console.WriteLine("Enter a search term:");

            return new(Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries));
        }

        void PrintDirTree(List<string> dirTree)
        {
            foreach (string dir in dirTree)
                Console.WriteLine(dir);
            dirTree.Clear();
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
                    if(terms.Match(dir.Name))
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
                    if (option == "fullpath" || option == "f")
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
                    else if (terms[i].StartsWith("-"))
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
}