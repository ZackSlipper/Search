namespace Search;

public class SubQuery
{
	public string[] Terms { get; }
	public SubQueryFlags Flags { get; }

	public SubQuery(string[] terms, SubQueryFlags flags)
	{
		Terms = terms;
		Flags = flags;
	}
}