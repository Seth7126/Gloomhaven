using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.ComponentModel.DataAnnotations;

internal class ValidationAttributeStore
{
	private abstract class StoreItem
	{
		private static IEnumerable<ValidationAttribute> _emptyValidationAttributeEnumerable = new ValidationAttribute[0];

		private IEnumerable<ValidationAttribute> _validationAttributes;

		internal IEnumerable<ValidationAttribute> ValidationAttributes => _validationAttributes;

		internal DisplayAttribute DisplayAttribute { get; set; }

		internal StoreItem(IEnumerable<Attribute> attributes)
		{
			_validationAttributes = attributes.OfType<ValidationAttribute>();
			DisplayAttribute = attributes.OfType<DisplayAttribute>().SingleOrDefault();
		}
	}

	private class TypeStoreItem : StoreItem
	{
		private object _syncRoot = new object();

		private Type _type;

		private Dictionary<string, PropertyStoreItem> _propertyStoreItems;

		internal TypeStoreItem(Type type, IEnumerable<Attribute> attributes)
			: base(attributes)
		{
			_type = type;
		}

		internal PropertyStoreItem GetPropertyStoreItem(string propertyName)
		{
			PropertyStoreItem item = null;
			if (!TryGetPropertyStoreItem(propertyName, out item))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The type '{0}' does not contain a public property named '{1}'.", _type.Name, propertyName), "propertyName");
			}
			return item;
		}

		internal bool TryGetPropertyStoreItem(string propertyName, out PropertyStoreItem item)
		{
			if (string.IsNullOrEmpty(propertyName))
			{
				throw new ArgumentNullException("propertyName");
			}
			if (_propertyStoreItems == null)
			{
				lock (_syncRoot)
				{
					if (_propertyStoreItems == null)
					{
						_propertyStoreItems = CreatePropertyStoreItems();
					}
				}
			}
			if (!_propertyStoreItems.TryGetValue(propertyName, out item))
			{
				return false;
			}
			return true;
		}

		private Dictionary<string, PropertyStoreItem> CreatePropertyStoreItems()
		{
			Dictionary<string, PropertyStoreItem> dictionary = new Dictionary<string, PropertyStoreItem>();
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(_type))
			{
				PropertyStoreItem value = new PropertyStoreItem(property.PropertyType, GetExplicitAttributes(property).Cast<Attribute>());
				dictionary[property.Name] = value;
			}
			return dictionary;
		}

		public static AttributeCollection GetExplicitAttributes(PropertyDescriptor propertyDescriptor)
		{
			List<Attribute> list = new List<Attribute>(propertyDescriptor.Attributes.Cast<Attribute>());
			IEnumerable<Attribute> enumerable = TypeDescriptor.GetAttributes(propertyDescriptor.PropertyType).Cast<Attribute>();
			bool flag = false;
			foreach (Attribute item in enumerable)
			{
				for (int num = list.Count - 1; num >= 0; num--)
				{
					if (item == list[num])
					{
						list.RemoveAt(num);
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return propertyDescriptor.Attributes;
			}
			return new AttributeCollection(list.ToArray());
		}
	}

	private class PropertyStoreItem : StoreItem
	{
		private Type _propertyType;

		internal Type PropertyType => _propertyType;

		internal PropertyStoreItem(Type propertyType, IEnumerable<Attribute> attributes)
			: base(attributes)
		{
			_propertyType = propertyType;
		}
	}

	private static ValidationAttributeStore _singleton = new ValidationAttributeStore();

	private Dictionary<Type, TypeStoreItem> _typeStoreItems = new Dictionary<Type, TypeStoreItem>();

	internal static ValidationAttributeStore Instance => _singleton;

	internal IEnumerable<ValidationAttribute> GetTypeValidationAttributes(ValidationContext validationContext)
	{
		EnsureValidationContext(validationContext);
		return GetTypeStoreItem(validationContext.ObjectType).ValidationAttributes;
	}

	internal DisplayAttribute GetTypeDisplayAttribute(ValidationContext validationContext)
	{
		EnsureValidationContext(validationContext);
		return GetTypeStoreItem(validationContext.ObjectType).DisplayAttribute;
	}

	internal IEnumerable<ValidationAttribute> GetPropertyValidationAttributes(ValidationContext validationContext)
	{
		EnsureValidationContext(validationContext);
		return GetTypeStoreItem(validationContext.ObjectType).GetPropertyStoreItem(validationContext.MemberName).ValidationAttributes;
	}

	internal DisplayAttribute GetPropertyDisplayAttribute(ValidationContext validationContext)
	{
		EnsureValidationContext(validationContext);
		return GetTypeStoreItem(validationContext.ObjectType).GetPropertyStoreItem(validationContext.MemberName).DisplayAttribute;
	}

	internal Type GetPropertyType(ValidationContext validationContext)
	{
		EnsureValidationContext(validationContext);
		return GetTypeStoreItem(validationContext.ObjectType).GetPropertyStoreItem(validationContext.MemberName).PropertyType;
	}

	internal bool IsPropertyContext(ValidationContext validationContext)
	{
		EnsureValidationContext(validationContext);
		TypeStoreItem typeStoreItem = GetTypeStoreItem(validationContext.ObjectType);
		PropertyStoreItem item = null;
		return typeStoreItem.TryGetPropertyStoreItem(validationContext.MemberName, out item);
	}

	private TypeStoreItem GetTypeStoreItem(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		lock (_typeStoreItems)
		{
			TypeStoreItem value = null;
			if (!_typeStoreItems.TryGetValue(type, out value))
			{
				IEnumerable<Attribute> attributes = TypeDescriptor.GetAttributes(type).Cast<Attribute>();
				value = new TypeStoreItem(type, attributes);
				_typeStoreItems[type] = value;
			}
			return value;
		}
	}

	private static void EnsureValidationContext(ValidationContext validationContext)
	{
		if (validationContext == null)
		{
			throw new ArgumentNullException("validationContext");
		}
	}
}
