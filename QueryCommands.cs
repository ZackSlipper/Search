namespace Search;

[Flags]
public enum QueryCommands
{
	None = 0,
	Return = 1,
	Help = 1 << 1,
	Quit = 1 << 2,
	Print = 1 << 3,
	Silent = 1 << 4,
	Filter = 1 << 5,
	Load = 1 << 6,
	Save = 1 << 7,
	Write = 1 << 8
}