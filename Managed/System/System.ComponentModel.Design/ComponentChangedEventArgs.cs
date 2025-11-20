namespace System.ComponentModel.Design;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanged" /> event. This class cannot be inherited.</summary>
public sealed class ComponentChangedEventArgs : EventArgs
{
	/// <summary>Gets the component that was modified.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the component that was modified.</returns>
	public object Component { get; }

	/// <summary>Gets the member that has been changed.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.MemberDescriptor" /> that indicates the member that has been changed.</returns>
	public MemberDescriptor Member { get; }

	/// <summary>Gets the new value of the changed member.</summary>
	/// <returns>The new value of the changed member. This property can be null.</returns>
	public object NewValue { get; }

	/// <summary>Gets the old value of the changed member.</summary>
	/// <returns>The old value of the changed member. This property can be null.</returns>
	public object OldValue { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.ComponentChangedEventArgs" /> class.</summary>
	/// <param name="component">The component that was changed. </param>
	/// <param name="member">A <see cref="T:System.ComponentModel.MemberDescriptor" /> that represents the member that was changed. </param>
	/// <param name="oldValue">The old value of the changed member. </param>
	/// <param name="newValue">The new value of the changed member. </param>
	public ComponentChangedEventArgs(object component, MemberDescriptor member, object oldValue, object newValue)
	{
		Component = component;
		Member = member;
		OldValue = oldValue;
		NewValue = newValue;
	}
}
