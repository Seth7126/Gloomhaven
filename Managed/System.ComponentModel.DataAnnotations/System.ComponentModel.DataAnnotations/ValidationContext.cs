using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Describes the context in which a validation check is performed.</summary>
public sealed class ValidationContext : IServiceProvider
{
	private class ValidationContextServiceContainer : IServiceContainer, IServiceProvider
	{
		private IServiceContainer _parentContainer;

		private Dictionary<Type, object> _services = new Dictionary<Type, object>();

		private readonly object _lock = new object();

		internal ValidationContextServiceContainer()
		{
		}

		internal ValidationContextServiceContainer(IServiceContainer parentContainer)
		{
			_parentContainer = parentContainer;
		}

		public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
		{
			if (promote && _parentContainer != null)
			{
				_parentContainer.AddService(serviceType, callback, promote);
				return;
			}
			lock (_lock)
			{
				if (_services.ContainsKey(serviceType))
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "A service of type '{0}' already exists in the container.", serviceType), "serviceType");
				}
				_services.Add(serviceType, callback);
			}
		}

		public void AddService(Type serviceType, ServiceCreatorCallback callback)
		{
			AddService(serviceType, callback, promote: true);
		}

		public void AddService(Type serviceType, object serviceInstance, bool promote)
		{
			if (promote && _parentContainer != null)
			{
				_parentContainer.AddService(serviceType, serviceInstance, promote);
				return;
			}
			lock (_lock)
			{
				if (_services.ContainsKey(serviceType))
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "A service of type '{0}' already exists in the container.", serviceType), "serviceType");
				}
				_services.Add(serviceType, serviceInstance);
			}
		}

		public void AddService(Type serviceType, object serviceInstance)
		{
			AddService(serviceType, serviceInstance, promote: true);
		}

		public void RemoveService(Type serviceType, bool promote)
		{
			lock (_lock)
			{
				if (_services.ContainsKey(serviceType))
				{
					_services.Remove(serviceType);
				}
			}
			if (promote && _parentContainer != null)
			{
				_parentContainer.RemoveService(serviceType);
			}
		}

		public void RemoveService(Type serviceType)
		{
			RemoveService(serviceType, promote: true);
		}

		public object GetService(Type serviceType)
		{
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			object value = null;
			_services.TryGetValue(serviceType, out value);
			if (value == null && _parentContainer != null)
			{
				value = _parentContainer.GetService(serviceType);
			}
			if (value is ServiceCreatorCallback serviceCreatorCallback)
			{
				value = serviceCreatorCallback(this, serviceType);
			}
			return value;
		}
	}

	private Func<Type, object> _serviceProvider;

	private object _objectInstance;

	private string _memberName;

	private string _displayName;

	private Dictionary<object, object> _items;

	private IServiceContainer _serviceContainer;

	/// <summary>Gets the object to validate.</summary>
	/// <returns>The object to validate.</returns>
	public object ObjectInstance => _objectInstance;

	/// <summary>Gets the type of the object to validate.</summary>
	/// <returns>The type of the object to validate.</returns>
	public Type ObjectType => ObjectInstance.GetType();

	/// <summary>Gets or sets the name of the member to validate. </summary>
	/// <returns>The name of the member to validate. </returns>
	public string DisplayName
	{
		get
		{
			if (string.IsNullOrEmpty(_displayName))
			{
				_displayName = GetDisplayName();
				if (string.IsNullOrEmpty(_displayName))
				{
					_displayName = MemberName;
					if (string.IsNullOrEmpty(_displayName))
					{
						_displayName = ObjectType.Name;
					}
				}
			}
			return _displayName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentNullException("value");
			}
			_displayName = value;
		}
	}

	/// <summary>Gets or sets the name of the member to validate. </summary>
	/// <returns>The name of the member to validate. </returns>
	public string MemberName
	{
		get
		{
			return _memberName;
		}
		set
		{
			_memberName = value;
		}
	}

	/// <summary>Gets the dictionary of key/value pairs that is associated with this context.</summary>
	/// <returns>The dictionary of the key/value pairs for this context.</returns>
	public IDictionary<object, object> Items => _items;

	/// <summary>Gets the validation services container.</summary>
	/// <returns>The validation services container.</returns>
	public IServiceContainer ServiceContainer
	{
		get
		{
			if (_serviceContainer == null)
			{
				_serviceContainer = new ValidationContextServiceContainer();
			}
			return _serviceContainer;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationContext" /> class using the specified object instance</summary>
	/// <param name="instance">The object instance to validate. It cannot be null.</param>
	public ValidationContext(object instance)
		: this(instance, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationContext" /> class using the specified object and an optional property bag.</summary>
	/// <param name="instance">The object instance to validate.  It cannot be null</param>
	/// <param name="items">An optional set of key/value pairs to make available to consumers.</param>
	public ValidationContext(object instance, IDictionary<object, object> items)
		: this(instance, null, items)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationContext" /> class using the service provider and dictionary of service consumers. </summary>
	/// <param name="instance">The object to validate. This parameter is required.</param>
	/// <param name="serviceProvider">The object that implements the <see cref="T:System.IServiceProvider" /> interface. This parameter is optional.</param>
	/// <param name="items">A dictionary of key/value pairs to make available to the service consumers. This parameter is optional.</param>
	public ValidationContext(object instance, IServiceProvider serviceProvider, IDictionary<object, object> items)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if (serviceProvider != null)
		{
			InitializeServiceProvider((Type serviceType) => serviceProvider.GetService(serviceType));
		}
		if (serviceProvider is IServiceContainer parentContainer)
		{
			_serviceContainer = new ValidationContextServiceContainer(parentContainer);
		}
		else
		{
			_serviceContainer = new ValidationContextServiceContainer();
		}
		if (items != null)
		{
			_items = new Dictionary<object, object>(items);
		}
		else
		{
			_items = new Dictionary<object, object>();
		}
		_objectInstance = instance;
	}

	private string GetDisplayName()
	{
		string text = null;
		ValidationAttributeStore instance = ValidationAttributeStore.Instance;
		DisplayAttribute displayAttribute = null;
		if (string.IsNullOrEmpty(_memberName))
		{
			displayAttribute = instance.GetTypeDisplayAttribute(this);
		}
		else if (instance.IsPropertyContext(this))
		{
			displayAttribute = instance.GetPropertyDisplayAttribute(this);
		}
		if (displayAttribute != null)
		{
			text = displayAttribute.GetName();
		}
		return text ?? MemberName;
	}

	/// <summary>Initializes the <see cref="T:System.ComponentModel.DataAnnotations.ValidationContext" /> using a service provider that can return service instances by type when GetService is called.</summary>
	/// <param name="serviceProvider">The service provider.</param>
	public void InitializeServiceProvider(Func<Type, object> serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	/// <summary>Returns the service that provides custom validation.</summary>
	/// <returns>An instance of the service, or null if the service is not available.</returns>
	/// <param name="serviceType">The type of the service to use for validation.</param>
	public object GetService(Type serviceType)
	{
		object obj = null;
		if (_serviceContainer != null)
		{
			obj = _serviceContainer.GetService(serviceType);
		}
		if (obj == null && _serviceProvider != null)
		{
			obj = _serviceProvider(serviceType);
		}
		return obj;
	}
}
