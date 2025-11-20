namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.Design.Serialization.IDesignerSerializationManager.ResolveName" /> event.</summary>
public class ResolveNameEventArgs : EventArgs
{
	/// <summary>Gets the name of the object to resolve.</summary>
	/// <returns>The name of the object to resolve.</returns>
	public string Name { get; }

	/// <summary>Gets or sets the object that matches the name.</summary>
	/// <returns>The object that the name is associated with.</returns>
	public object Value { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.ResolveNameEventArgs" /> class.</summary>
	/// <param name="name">The name to resolve. </param>
	public ResolveNameEventArgs(string name)
	{
		Name = name;
		Value = null;
	}
}
