using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Forces the attributed object to be created in the context of the creator, if possible. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class)]
[ComVisible(false)]
public sealed class MustRunInClientContextAttribute : Attribute
{
	private bool val;

	/// <summary>Gets a value that indicates whether the attributed object is to be created in the context of the creator.</summary>
	/// <returns>true if the object is to be created in the context of the creator; otherwise, false. The default is true.</returns>
	public bool Value => val;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.MustRunInClientContextAttribute" /> class, requiring creation of the object in the context of the creator.</summary>
	public MustRunInClientContextAttribute()
		: this(val: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.MustRunInClientContextAttribute" /> class, optionally not creating the object in the context of the creator.</summary>
	/// <param name="val">true to create the object in the context of the creator; otherwise, false. </param>
	public MustRunInClientContextAttribute(bool val)
	{
		this.val = val;
	}
}
