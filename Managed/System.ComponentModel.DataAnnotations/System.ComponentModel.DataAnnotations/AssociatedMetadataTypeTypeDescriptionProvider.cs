namespace System.ComponentModel.DataAnnotations;

/// <summary>Extends the metadata information for a class by adding attributes and property information that is defined in an associated class.</summary>
public class AssociatedMetadataTypeTypeDescriptionProvider : TypeDescriptionProvider
{
	private Type _associatedMetadataType;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.AssociatedMetadataTypeTypeDescriptionProvider" /> class by using the specified type.</summary>
	/// <param name="type">The type for which the metadata provider is created.</param>
	public AssociatedMetadataTypeTypeDescriptionProvider(Type type)
		: base(TypeDescriptor.GetProvider(type))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.AssociatedMetadataTypeTypeDescriptionProvider" /> class by using the specified metadata provider type and associated type.</summary>
	/// <param name="type">The type for which the metadata provider is created.</param>
	/// <param name="associatedMetadataType">The associated type that contains the metadata.</param>
	/// <exception cref="ArgumentNullException">The value of <paramref name="associatedMetadataType" /> is null.</exception>
	public AssociatedMetadataTypeTypeDescriptionProvider(Type type, Type associatedMetadataType)
		: this(type)
	{
		if (associatedMetadataType == null)
		{
			throw new ArgumentNullException("associatedMetadataType");
		}
		_associatedMetadataType = associatedMetadataType;
	}

	/// <summary>Gets a type descriptor for the specified type and object.</summary>
	/// <returns>The descriptor that provides metadata for the type.</returns>
	/// <param name="objectType">The type of object to retrieve the type descriptor for.</param>
	/// <param name="instance">An instance of the type. </param>
	public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
	{
		return new AssociatedMetadataTypeTypeDescriptor(base.GetTypeDescriptor(objectType, instance), objectType, _associatedMetadataType);
	}
}
