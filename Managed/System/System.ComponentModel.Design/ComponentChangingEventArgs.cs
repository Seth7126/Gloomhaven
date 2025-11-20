namespace System.ComponentModel.Design;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanging" /> event. This class cannot be inherited.</summary>
public sealed class ComponentChangingEventArgs : EventArgs
{
	/// <summary>Gets the component that is about to be changed or the component that is the parent container of the member that is about to be changed.</summary>
	/// <returns>The component that is about to have a member changed.</returns>
	public object Component { get; }

	/// <summary>Gets the member that is about to be changed.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.MemberDescriptor" /> indicating the member that is about to be changed, if known, or null otherwise.</returns>
	public MemberDescriptor Member { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.ComponentChangingEventArgs" /> class.</summary>
	/// <param name="component">The component that is about to be changed. </param>
	/// <param name="member">A <see cref="T:System.ComponentModel.MemberDescriptor" /> indicating the member of the component that is about to be changed. </param>
	public ComponentChangingEventArgs(object component, MemberDescriptor member)
	{
		Component = component;
		Member = member;
	}
}
