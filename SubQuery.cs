namespace Search;

public class SubQuery
{
	static readonly (SubQueryFlags, SubQueryFlags)[] InvalidFlagCombinations = new[]
	{
		(SubQueryFlags.Fail, SubQueryFlags.Optional),
		(SubQueryFlags.Fail, SubQueryFlags.Required),
		(SubQueryFlags.Optional, SubQueryFlags.Required),
	};


	public string[] Terms { get; }
	public SubQueryFlags Flags { get; }

	public SubQuery(string[] terms, SubQueryFlags flags)
	{
		Terms = terms;
		Flags = flags;
	}

	public void Validate()
	{
		if (Terms.Length == 0)
			throw new ArgumentException("SubQuery must have at least one term.");

		foreach ((SubQueryFlags, SubQueryFlags) invalidFlagCombination in InvalidFlagCombinations)
			if (Flags.HasFlag(invalidFlagCombination.Item1) && Flags.HasFlag(invalidFlagCombination.Item2))
				throw new ArgumentException($"SubQuery cannot have both {invalidFlagCombination.Item1} and {invalidFlagCombination.Item2} flags.");
	}
}