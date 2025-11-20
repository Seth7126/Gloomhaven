namespace System.ComponentModel.Design;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.Design.IDesignerEventService.DesignerCreated" /> and <see cref="E:System.ComponentModel.Design.IDesignerEventService.DesignerDisposed" /> events.</summary>
public class DesignerEventArgs : EventArgs
{
	/// <summary>Gets the host of the document.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.Design.IDesignerHost" /> of the document.</returns>
	public IDesignerHost Designer { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesignerEventArgs" /> class.</summary>
	/// <param name="host">The <see cref="T:System.ComponentModel.Design.IDesignerHost" /> of the document. </param>
	public DesignerEventArgs(IDesignerHost host)
	{
		Designer = host;
	}
}
