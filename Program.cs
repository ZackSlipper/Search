namespace Search;
static class Program
{
	public const string Version = "0.0.1";

	public static void Main(string[] args)
	{
		SearchRunner search = new(string.Join(' ', args));
	}
}