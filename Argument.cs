namespace Search;

public class Argument
{
	public string Name { get; }
	public List<string> Values { get; } = new();
	public bool IsFlag => Values.Count == 0;
	public bool NameIsValue { get; }

	public Argument(string name, bool nameIsValue)
	{
		Name = name;
		NameIsValue = nameIsValue;
	}

	public Argument(string name, string value) : this(name, false)
	{
		if (value != Name)
			Values.Add(value);
	}

	public override string ToString() => $"-{Name} {string.Join(" ", Values)}";

	public override bool Equals(object? obj) => obj is Argument arg && Name == arg.Name;

	public override int GetHashCode() => Name.GetHashCode();

	public static bool operator ==(Argument left, Argument right) => left.Equals(right);

	public static bool operator !=(Argument left, Argument right) => !(left == right);
}