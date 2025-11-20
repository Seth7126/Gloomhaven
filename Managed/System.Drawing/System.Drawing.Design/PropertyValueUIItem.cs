namespace System.Drawing.Design;

/// <summary>Provides information about a property displayed in the Properties window, including the associated event handler, pop-up information string, and the icon to display for the property.</summary>
public class PropertyValueUIItem
{
	private Image itemImage;

	private PropertyValueUIItemInvokeHandler handler;

	private string tooltip;

	/// <summary>Gets the 8 x 8 pixel image that will be drawn in the Properties window.</summary>
	/// <returns>The image to use for the property icon.</returns>
	public virtual Image Image => itemImage;

	/// <summary>Gets the handler that is raised when a user double-clicks this item.</summary>
	/// <returns>A <see cref="T:System.Drawing.Design.PropertyValueUIItemInvokeHandler" /> indicating the event handler for this user interface (UI) item.</returns>
	public virtual PropertyValueUIItemInvokeHandler InvokeHandler => handler;

	/// <summary>Gets or sets the information string to display for this item.</summary>
	/// <returns>A string containing the information string to display for this item.</returns>
	public virtual string ToolTip => tooltip;

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Design.PropertyValueUIItem" /> class.</summary>
	/// <param name="uiItemImage">The icon to display. The image must be 8 x 8 pixels. </param>
	/// <param name="handler">The handler to invoke when the image is double-clicked. </param>
	/// <param name="tooltip">The <see cref="P:System.Drawing.Design.PropertyValueUIItem.ToolTip" /> to display for the property that this <see cref="T:System.Drawing.Design.PropertyValueUIItem" /> is associated with. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="uiItemImage" /> or <paramref name="handler" /> is null.</exception>
	public PropertyValueUIItem(Image uiItemImage, PropertyValueUIItemInvokeHandler handler, string tooltip)
	{
		itemImage = uiItemImage;
		this.handler = handler;
		if (itemImage == null)
		{
			throw new ArgumentNullException("uiItemImage");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		this.tooltip = tooltip;
	}

	/// <summary>Resets the user interface (UI) item.</summary>
	public virtual void Reset()
	{
	}
}
