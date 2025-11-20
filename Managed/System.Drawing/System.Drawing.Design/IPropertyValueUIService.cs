using System.ComponentModel;

namespace System.Drawing.Design;

/// <summary>Provides an interface to manage the images, ToolTips, and event handlers for the properties of a component displayed in a property browser.</summary>
public interface IPropertyValueUIService
{
	/// <summary>Occurs when the list of <see cref="T:System.Drawing.Design.PropertyValueUIItem" /> objects is modified.</summary>
	event EventHandler PropertyUIValueItemsChanged;

	/// <summary>Adds the specified <see cref="T:System.Drawing.Design.PropertyValueUIHandler" /> to this service.</summary>
	/// <param name="newHandler">The property value UI handler to add. </param>
	void AddPropertyValueUIHandler(PropertyValueUIHandler newHandler);

	/// <summary>Gets the <see cref="T:System.Drawing.Design.PropertyValueUIItem" /> objects that match the specified context and property descriptor characteristics.</summary>
	/// <returns>An array of <see cref="T:System.Drawing.Design.PropertyValueUIItem" /> objects that match the specified parameters.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that can be used to gain additional context information. </param>
	/// <param name="propDesc">A <see cref="T:System.ComponentModel.PropertyDescriptor" /> that indicates the property to match with the properties to return. </param>
	PropertyValueUIItem[] GetPropertyUIValueItems(ITypeDescriptorContext context, PropertyDescriptor propDesc);

	/// <summary>Notifies the <see cref="T:System.Drawing.Design.IPropertyValueUIService" /> implementation that the global list of <see cref="T:System.Drawing.Design.PropertyValueUIItem" /> objects has been modified.</summary>
	void NotifyPropertyValueUIItemsChanged();

	/// <summary>Removes the specified <see cref="T:System.Drawing.Design.PropertyValueUIHandler" /> from the property value UI service.</summary>
	/// <param name="newHandler">The handler to remove. </param>
	void RemovePropertyValueUIHandler(PropertyValueUIHandler newHandler);
}
