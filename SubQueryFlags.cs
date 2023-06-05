namespace Search;

[Flags]
public enum SubQueryFlags
{
	None = 0,
	Required = 1,
	Optional = 1 << 1,
	Fail = 1 << 2,
	Unique = 1 << 3,
	MatchCase = 1 << 4,
	OnlyDir = 1 << 5
}