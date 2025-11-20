using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations;

internal class AssociatedMetadataTypeTypeDescriptor : CustomTypeDescriptor
{
	private static class TypeDescriptorCache
	{
		private static readonly Attribute[] emptyAttributes = new Attribute[0];

		private static readonly ConcurrentDictionary<Type, Type> _metadataTypeCache = new ConcurrentDictionary<Type, Type>();

		private static readonly ConcurrentDictionary<Tuple<Type, string>, Attribute[]> _typeMemberCache = new ConcurrentDictionary<Tuple<Type, string>, Attribute[]>();

		private static readonly ConcurrentDictionary<Tuple<Type, Type>, bool> _validatedMetadataTypeCache = new ConcurrentDictionary<Tuple<Type, Type>, bool>();

		public static void ValidateMetadataType(Type type, Type associatedType)
		{
			Tuple<Type, Type> key = new Tuple<Type, Type>(type, associatedType);
			if (!_validatedMetadataTypeCache.ContainsKey(key))
			{
				CheckAssociatedMetadataType(type, associatedType);
				_validatedMetadataTypeCache.TryAdd(key, value: true);
			}
		}

		public static Type GetAssociatedMetadataType(Type type)
		{
			Type value = null;
			if (_metadataTypeCache.TryGetValue(type, out value))
			{
				return value;
			}
			MetadataTypeAttribute metadataTypeAttribute = (MetadataTypeAttribute)Attribute.GetCustomAttribute(type, typeof(MetadataTypeAttribute));
			if (metadataTypeAttribute != null)
			{
				value = metadataTypeAttribute.MetadataClassType;
			}
			_metadataTypeCache.TryAdd(type, value);
			return value;
		}

		private static void CheckAssociatedMetadataType(Type mainType, Type associatedMetadataType)
		{
			HashSet<string> other = new HashSet<string>(from p in mainType.GetProperties()
				select p.Name);
			IEnumerable<string> first = from f in associatedMetadataType.GetFields()
				select f.Name;
			IEnumerable<string> second = from p in associatedMetadataType.GetProperties()
				select p.Name;
			HashSet<string> hashSet = new HashSet<string>(first.Concat(second), StringComparer.Ordinal);
			if (!hashSet.IsSubsetOf(other))
			{
				hashSet.ExceptWith(other);
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The associated metadata type for type '{0}' contains the following unknown properties or fields: {1}. Please make sure that the names of these members match the names of the properties on the main type.", mainType.FullName, string.Join(", ", hashSet.ToArray())));
			}
		}

		public static Attribute[] GetAssociatedMetadata(Type type, string memberName)
		{
			Tuple<Type, string> key = new Tuple<Type, string>(type, memberName);
			if (_typeMemberCache.TryGetValue(key, out var value))
			{
				return value;
			}
			MemberTypes type2 = MemberTypes.Field | MemberTypes.Property;
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
			MemberInfo memberInfo = type.GetMember(memberName, type2, bindingAttr).FirstOrDefault();
			value = ((!(memberInfo != null)) ? emptyAttributes : Attribute.GetCustomAttributes(memberInfo, inherit: true));
			_typeMemberCache.TryAdd(key, value);
			return value;
		}
	}

	private Type AssociatedMetadataType { get; set; }

	private bool IsSelfAssociated { get; set; }

	public AssociatedMetadataTypeTypeDescriptor(ICustomTypeDescriptor parent, Type type, Type associatedMetadataType)
		: base(parent)
	{
		AssociatedMetadataType = associatedMetadataType ?? TypeDescriptorCache.GetAssociatedMetadataType(type);
		IsSelfAssociated = type == AssociatedMetadataType;
		if (AssociatedMetadataType != null)
		{
			TypeDescriptorCache.ValidateMetadataType(type, AssociatedMetadataType);
		}
	}

	public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		return GetPropertiesWithMetadata(base.GetProperties(attributes));
	}

	public override PropertyDescriptorCollection GetProperties()
	{
		return GetPropertiesWithMetadata(base.GetProperties());
	}

	private PropertyDescriptorCollection GetPropertiesWithMetadata(PropertyDescriptorCollection originalCollection)
	{
		if (AssociatedMetadataType == null)
		{
			return originalCollection;
		}
		bool flag = false;
		List<PropertyDescriptor> list = new List<PropertyDescriptor>();
		foreach (PropertyDescriptor item2 in originalCollection)
		{
			Attribute[] associatedMetadata = TypeDescriptorCache.GetAssociatedMetadata(AssociatedMetadataType, item2.Name);
			PropertyDescriptor item = item2;
			if (associatedMetadata.Length != 0)
			{
				item = new MetadataPropertyDescriptorWrapper(item2, associatedMetadata);
				flag = true;
			}
			list.Add(item);
		}
		if (flag)
		{
			return new PropertyDescriptorCollection(list.ToArray(), readOnly: true);
		}
		return originalCollection;
	}

	public override AttributeCollection GetAttributes()
	{
		AttributeCollection attributeCollection = base.GetAttributes();
		if (AssociatedMetadataType != null && !IsSelfAssociated)
		{
			Attribute[] newAttributes = TypeDescriptor.GetAttributes(AssociatedMetadataType).OfType<Attribute>().ToArray();
			attributeCollection = AttributeCollection.FromExisting(attributeCollection, newAttributes);
		}
		return attributeCollection;
	}
}
