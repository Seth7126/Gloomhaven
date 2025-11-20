namespace System.ComponentModel.Design;

/// <summary>Provides data for the <see cref="P:System.ComponentModel.Design.IDesignerEventService.ActiveDesigner" /> event.</summary>
public class ActiveDesignerEventArgs : EventArgs
{
	/// <summary>Gets the document that is losing activation.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.Design.IDesignerHost" /> that represents the document losing activation.</returns>
	public IDesignerHost OldDesigner { get; }

	/// <summary>Gets the document that is gaining activation.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.Design.IDesignerHost" /> that represents the document gaining activation.</returns>
	public IDesignerHost NewDesigner { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.ActiveDesignerEventArgs" /> class.</summary>
	/// <param name="oldDesigner">The document that is losing activation. </param>
	/// <param name="newDesigner">The document that is gaining activation. </param>
	public ActiveDesignerEventArgs(IDesignerHost oldDesigner, IDesignerHost newDesigner)
	{
		OldDesigner = oldDesigner;
		NewDesigner = newDesigner;
	}
}
