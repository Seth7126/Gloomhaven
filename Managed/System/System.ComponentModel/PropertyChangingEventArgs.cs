namespace System.ComponentModel;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.INotifyPropertyChanging.PropertyChanging" /> event. </summary>
public class PropertyChangingEventArgs : EventArgs
{
	private readonly string _propertyName;

	/// <summary>Gets the name of the property whose value is changing.</summary>
	/// <returns>The name of the property whose value is changing.</returns>
	public virtual string PropertyName => _propertyName;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyChangingEventArgs" /> class. </summary>
	/// <param name="propertyName">The name of the property whose value is changing.</param>
	public PropertyChangingEventArgs(string propertyName)
	{
		_propertyName = propertyName;
	}
}
