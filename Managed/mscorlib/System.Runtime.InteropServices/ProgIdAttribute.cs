namespace System.Runtime.InteropServices;

/// <summary>Allows the user to specify the ProgID of a class.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[ComVisible(true)]
public sealed class ProgIdAttribute : Attribute
{
	internal string _val;

	/// <summary>Gets the ProgID of the class.</summary>
	/// <returns>The ProgID of the class.</returns>
	public string Value => _val;

	/// <summary>Initializes a new instance of the ProgIdAttribute with the specified ProgID.</summary>
	/// <param name="progId">The ProgID to be assigned to the class. </param>
	public ProgIdAttribute(string progId)
	{
		_val = progId;
	}
}
