using System.Collections.Generic;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies that an entity member represents a data relationship, such as a foreign key relationship.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class AssociationAttribute : Attribute
{
	private string name;

	private string thisKey;

	private string otherKey;

	private bool isForeignKey;

	/// <summary>Gets the name of the association.</summary>
	/// <returns>The name of the association.</returns>
	public string Name => name;

	/// <summary>Gets the property names of the key values on the ThisKey side of the association.</summary>
	/// <returns>A comma-separated list of the property names that represent the key values on the ThisKey side of the association.</returns>
	public string ThisKey => thisKey;

	/// <summary>Gets the property names of the key values on the OtherKey side of the association.</summary>
	/// <returns>A comma-separated list of the property names that represent the key values on the OtherKey side of the association.</returns>
	public string OtherKey => otherKey;

	/// <summary>Gets or sets a value that indicates whether the association member represents a foreign key.</summary>
	/// <returns>true if the association represents a foreign key; otherwise, false.</returns>
	public bool IsForeignKey
	{
		get
		{
			return isForeignKey;
		}
		set
		{
			isForeignKey = value;
		}
	}

	/// <summary>Gets a collection of individual key members that are specified in the <see cref="P:System.ComponentModel.DataAnnotations.AssociationAttribute.ThisKey" /> property.</summary>
	/// <returns>A collection of individual key members that are specified in the <see cref="P:System.ComponentModel.DataAnnotations.AssociationAttribute.ThisKey" /> property.</returns>
	public IEnumerable<string> ThisKeyMembers => GetKeyMembers(ThisKey);

	/// <summary>Gets a collection of individual key members that are specified in the <see cref="P:System.ComponentModel.DataAnnotations.AssociationAttribute.OtherKey" /> property.</summary>
	/// <returns>A collection of individual key members that are specified in the <see cref="P:System.ComponentModel.DataAnnotations.AssociationAttribute.OtherKey" /> property.</returns>
	public IEnumerable<string> OtherKeyMembers => GetKeyMembers(OtherKey);

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.AssociationAttribute" /> class.</summary>
	/// <param name="name">The name of the association. </param>
	/// <param name="thisKey">A comma-separated list of the property names of the key values on the <paramref name="thisKey" /> side of the association.</param>
	/// <param name="otherKey">A comma-separated list of the property names of the key values on the <paramref name="otherKey" /> side of the association.</param>
	public AssociationAttribute(string name, string thisKey, string otherKey)
	{
		this.name = name;
		this.thisKey = thisKey;
		this.otherKey = otherKey;
	}

	private static string[] GetKeyMembers(string key)
	{
		return key.Replace(" ", string.Empty).Split(',');
	}
}
