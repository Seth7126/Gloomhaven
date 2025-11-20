using System.Collections.Generic;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Represents an attribute that is used to specify the filtering behavior for a column.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class FilterUIHintAttribute : Attribute
{
	private UIHintAttribute.UIHintImplementation _implementation;

	/// <summary>Gets the name of the control to use for filtering.</summary>
	/// <returns>The name of the control to use for filtering.</returns>
	public string FilterUIHint => _implementation.UIHint;

	/// <summary>Gets the name of the presentation layer that supports this control.</summary>
	/// <returns>The name of the presentation layer that supports this control.</returns>
	public string PresentationLayer => _implementation.PresentationLayer;

	/// <summary>Gets the name/value pairs that are used as parameters in the control's constructor.</summary>
	/// <returns>The name/value pairs that are used as parameters in the control's constructor.</returns>
	public IDictionary<string, object> ControlParameters => _implementation.ControlParameters;

	/// <summary>Returns the unique identifier for this attribute instance.</summary>
	/// <returns>This attribuet instance unique identifier.</returns>
	public override object TypeId => this;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.FilterUIHintAttribute" /> class by using the filter UI hint.</summary>
	/// <param name="filterUIHint">The name of the control to use for filtering.</param>
	public FilterUIHintAttribute(string filterUIHint)
		: this(filterUIHint, null, new object[0])
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.FilterUIHintAttribute" /> class by using the filter UI hint and presentation layer name.</summary>
	/// <param name="filterUIHint">The name of the control to use for filtering.</param>
	/// <param name="presentationLayer">The name of the presentation layer that supports this control.</param>
	public FilterUIHintAttribute(string filterUIHint, string presentationLayer)
		: this(filterUIHint, presentationLayer, new object[0])
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.FilterUIHintAttribute" /> class by using the filter UI hint, presentation layer name, and control parameters.</summary>
	/// <param name="filterUIHint">The name of the control to use for filtering.</param>
	/// <param name="presentationLayer">The name of the presentation layer that supports this control.</param>
	/// <param name="controlParameters">The list of parameters for the control.</param>
	public FilterUIHintAttribute(string filterUIHint, string presentationLayer, params object[] controlParameters)
	{
		_implementation = new UIHintAttribute.UIHintImplementation(filterUIHint, presentationLayer, controlParameters);
	}

	/// <summary>Returns the hash code for this attribute instance.</summary>
	/// <returns>This attribute insatnce hash code.</returns>
	public override int GetHashCode()
	{
		return _implementation.GetHashCode();
	}

	/// <summary>Returns a value that indicates whether this attribute instance is equal to a specified object.</summary>
	/// <returns>True if the passed object is equal to this attribute instance; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this attribute instance.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is FilterUIHintAttribute filterUIHintAttribute))
		{
			return false;
		}
		return _implementation.Equals(filterUIHintAttribute._implementation);
	}
}
