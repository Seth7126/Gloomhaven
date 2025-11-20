using System.ComponentModel.Design;

namespace System.Drawing.Design;

/// <summary>Provides data for the <see cref="E:System.Drawing.Design.ToolboxItem.ComponentsCreating" /> event that occurs when components are added to the toolbox.</summary>
public class ToolboxComponentsCreatingEventArgs : EventArgs
{
	private readonly IDesignerHost host;

	/// <summary>Gets or sets an instance of the <see cref="T:System.ComponentModel.Design.IDesignerHost" /> that made the request to create toolbox components.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.Design.IDesignerHost" /> that made the request to create toolbox components, or null if no designer host was provided to the toolbox item.</returns>
	public IDesignerHost DesignerHost => host;

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Design.ToolboxComponentsCreatingEventArgs" /> class.</summary>
	/// <param name="host">The designer host that is making the request. </param>
	public ToolboxComponentsCreatingEventArgs(IDesignerHost host)
	{
		this.host = host;
	}
}
