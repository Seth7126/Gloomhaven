namespace System.Runtime.Serialization;

/// <summary>Holds the value, <see cref="T:System.Type" />, and name of a serialized object. </summary>
public readonly struct SerializationEntry
{
	private readonly string _name;

	private readonly object _value;

	private readonly Type _type;

	/// <summary>Gets the value contained in the object.</summary>
	/// <returns>The value contained in the object.</returns>
	public object Value => _value;

	/// <summary>Gets the name of the object.</summary>
	/// <returns>The name of the object.</returns>
	public string Name => _name;

	/// <summary>Gets the <see cref="T:System.Type" /> of the object.</summary>
	/// <returns>The <see cref="T:System.Type" /> of the object.</returns>
	public Type ObjectType => _type;

	internal SerializationEntry(string entryName, object entryValue, Type entryType)
	{
		_name = entryName;
		_value = entryValue;
		_type = entryType;
	}
}
