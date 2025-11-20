using System.ComponentModel;

namespace System.Drawing.Design;

/// <summary>Provides data for the <see cref="E:System.Drawing.Design.ToolboxItem.ComponentsCreated" /> event that occurs when components are added to the toolbox.</summary>
public class ToolboxComponentsCreatedEventArgs : EventArgs
{
	private readonly IComponent[] comps;

	/// <summary>Gets or sets an array containing the components to add to the toolbox.</summary>
	/// <returns>An array of type <see cref="T:System.ComponentModel.IComponent" /> indicating the components to add to the toolbox.</returns>
	public IComponent[] Components => (IComponent[])comps.Clone();

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Design.ToolboxComponentsCreatedEventArgs" /> class.</summary>
	/// <param name="components">The components to include in the toolbox. </param>
	public ToolboxComponentsCreatedEventArgs(IComponent[] components)
	{
		comps = components;
	}
}
