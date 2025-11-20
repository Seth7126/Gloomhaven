using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Unity;

namespace System.Drawing.Design;

/// <summary>Provides a base implementation of a toolbox item.</summary>
[Serializable]
[System.MonoTODO("Implementation is incomplete.")]
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
public class ToolboxItem : ISerializable
{
	private bool locked;

	private Hashtable properties = new Hashtable();

	/// <summary>Gets or sets the name of the assembly that contains the type or types that the toolbox item creates.</summary>
	/// <returns>An <see cref="T:System.Reflection.AssemblyName" /> that indicates the assembly containing the type or types to create.</returns>
	public AssemblyName AssemblyName
	{
		get
		{
			return (AssemblyName)properties["AssemblyName"];
		}
		set
		{
			SetValue("AssemblyName", value);
		}
	}

	/// <summary>Gets or sets a bitmap to represent the toolbox item in the toolbox.</summary>
	/// <returns>A <see cref="T:System.Drawing.Bitmap" /> that represents the toolbox item in the toolbox.</returns>
	public Bitmap Bitmap
	{
		get
		{
			return (Bitmap)properties["Bitmap"];
		}
		set
		{
			SetValue("Bitmap", value);
		}
	}

	/// <summary>Gets or sets the display name for the toolbox item.</summary>
	/// <returns>The display name for the toolbox item.</returns>
	public string DisplayName
	{
		get
		{
			return GetValue("DisplayName");
		}
		set
		{
			SetValue("DisplayName", value);
		}
	}

	/// <summary>Gets or sets the filter that determines whether the toolbox item can be used on a destination component.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> of <see cref="T:System.ComponentModel.ToolboxItemFilterAttribute" /> objects.</returns>
	public ICollection Filter
	{
		get
		{
			ICollection collection = (ICollection)properties["Filter"];
			if (collection == null)
			{
				collection = new ToolboxItemFilterAttribute[0];
			}
			return collection;
		}
		set
		{
			SetValue("Filter", value);
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Drawing.Design.ToolboxItem" /> is currently locked.</summary>
	/// <returns>true if the toolbox item is locked; otherwise, false.</returns>
	public virtual bool Locked => locked;

	/// <summary>Gets or sets the fully qualified name of the type of <see cref="T:System.ComponentModel.IComponent" /> that the toolbox item creates when invoked.</summary>
	/// <returns>The fully qualified type name of the type of component that this toolbox item creates.</returns>
	public string TypeName
	{
		get
		{
			return GetValue("TypeName");
		}
		set
		{
			SetValue("TypeName", value);
		}
	}

	/// <summary>Gets or sets the company name for this <see cref="T:System.Drawing.Design.ToolboxItem" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that specifies the company for this <see cref="T:System.Drawing.Design.ToolboxItem" />.</returns>
	public string Company
	{
		get
		{
			return (string)properties["Company"];
		}
		set
		{
			SetValue("Company", value);
		}
	}

	/// <summary>Gets the component type for this <see cref="T:System.Drawing.Design.ToolboxItem" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that specifies the component type for this <see cref="T:System.Drawing.Design.ToolboxItem" />.</returns>
	public virtual string ComponentType => ".NET Component";

	/// <summary>Gets or sets the <see cref="T:System.Reflection.AssemblyName" /> for the toolbox item.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.AssemblyName" /> objects.</returns>
	public AssemblyName[] DependentAssemblies
	{
		get
		{
			return (AssemblyName[])properties["DependentAssemblies"];
		}
		set
		{
			AssemblyName[] array = new AssemblyName[value.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = value[i];
			}
			SetValue("DependentAssemblies", array);
		}
	}

	/// <summary>Gets or sets the description for this <see cref="T:System.Drawing.Design.ToolboxItem" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that specifies the description for this <see cref="T:System.Drawing.Design.ToolboxItem" />.</returns>
	public string Description
	{
		get
		{
			return (string)properties["Description"];
		}
		set
		{
			SetValue("Description", value);
		}
	}

	/// <summary>Gets a value indicating whether the toolbox item is transient.</summary>
	/// <returns>true, if this toolbox item should not be stored in any toolbox database when an application that is providing a toolbox closes; otherwise, false.</returns>
	public bool IsTransient
	{
		get
		{
			object obj = properties["IsTransient"];
			if (obj != null)
			{
				return (bool)obj;
			}
			return false;
		}
		set
		{
			SetValue("IsTransient", value);
		}
	}

	/// <summary>Gets a dictionary of properties.</summary>
	/// <returns>A dictionary of name/value pairs (the names are property names and the values are property values).</returns>
	public IDictionary Properties => properties;

	/// <summary>Gets the version for this <see cref="T:System.Drawing.Design.ToolboxItem" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that specifies the version for this <see cref="T:System.Drawing.Design.ToolboxItem" />.</returns>
	public virtual string Version => string.Empty;

	public Bitmap OriginalBitmap
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Occurs immediately after components are created.</summary>
	public event ToolboxComponentsCreatedEventHandler ComponentsCreated;

	/// <summary>Occurs when components are about to be created.</summary>
	public event ToolboxComponentsCreatingEventHandler ComponentsCreating;

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Design.ToolboxItem" /> class.</summary>
	public ToolboxItem()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Design.ToolboxItem" /> class that creates the specified type of component.</summary>
	/// <param name="toolType">The type of <see cref="T:System.ComponentModel.IComponent" /> that the toolbox item creates. </param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Drawing.Design.ToolboxItem" /> was locked. </exception>
	public ToolboxItem(Type toolType)
	{
		Initialize(toolType);
	}

	/// <summary>Throws an exception if the toolbox item is currently locked.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Drawing.Design.ToolboxItem" /> is locked. </exception>
	protected void CheckUnlocked()
	{
		if (locked)
		{
			throw new InvalidOperationException("The ToolboxItem is locked");
		}
	}

	/// <summary>Creates the components that the toolbox item is configured to create.</summary>
	/// <returns>An array of created <see cref="T:System.ComponentModel.IComponent" /> objects.</returns>
	public IComponent[] CreateComponents()
	{
		return CreateComponents(null);
	}

	/// <summary>Creates the components that the toolbox item is configured to create, using the specified designer host.</summary>
	/// <returns>An array of created <see cref="T:System.ComponentModel.IComponent" /> objects.</returns>
	/// <param name="host">The <see cref="T:System.ComponentModel.Design.IDesignerHost" /> to use when creating the components. </param>
	public IComponent[] CreateComponents(IDesignerHost host)
	{
		OnComponentsCreating(new ToolboxComponentsCreatingEventArgs(host));
		IComponent[] array = CreateComponentsCore(host);
		OnComponentsCreated(new ToolboxComponentsCreatedEventArgs(array));
		return array;
	}

	/// <summary>Creates a component or an array of components when the toolbox item is invoked.</summary>
	/// <returns>An array of created <see cref="T:System.ComponentModel.IComponent" /> objects.</returns>
	/// <param name="host">The <see cref="T:System.ComponentModel.Design.IDesignerHost" /> to host the toolbox item. </param>
	protected virtual IComponent[] CreateComponentsCore(IDesignerHost host)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		Type type = GetType(host, AssemblyName, TypeName, reference: true);
		if (type == null)
		{
			return new IComponent[0];
		}
		return new IComponent[1] { host.CreateComponent(type) };
	}

	/// <summary>Creates an array of components when the toolbox item is invoked.</summary>
	/// <returns>An array of created <see cref="T:System.ComponentModel.IComponent" /> objects.</returns>
	/// <param name="host">The designer host to use when creating components.</param>
	/// <param name="defaultValues">A dictionary of property name/value pairs of default values with which to initialize the component.</param>
	protected virtual IComponent[] CreateComponentsCore(IDesignerHost host, IDictionary defaultValues)
	{
		IComponent[] array = CreateComponentsCore(host);
		IComponent[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Component component = (Component)array2[i];
			(host.GetDesigner(component) as IComponentInitializer).InitializeNewComponent(defaultValues);
		}
		return array;
	}

	/// <summary>Creates the components that the toolbox item is configured to create, using the specified designer host and default values.</summary>
	/// <returns>An array of created <see cref="T:System.ComponentModel.IComponent" /> objects.</returns>
	/// <param name="host">The <see cref="T:System.ComponentModel.Design.IDesignerHost" /> to use when creating the components.</param>
	/// <param name="defaultValues">A dictionary of property name/value pairs of default values with which to initialize the component.</param>
	public IComponent[] CreateComponents(IDesignerHost host, IDictionary defaultValues)
	{
		OnComponentsCreating(new ToolboxComponentsCreatingEventArgs(host));
		IComponent[] array = CreateComponentsCore(host, defaultValues);
		OnComponentsCreated(new ToolboxComponentsCreatedEventArgs(array));
		return array;
	}

	/// <summary>Filters a property value before returning it.</summary>
	/// <returns>A filtered property value.</returns>
	/// <param name="propertyName">The name of the property to filter.</param>
	/// <param name="value">The value against which to filter the property.</param>
	protected virtual object FilterPropertyValue(string propertyName, object value)
	{
		switch (propertyName)
		{
		case "AssemblyName":
			if (value != null)
			{
				return (value as ICloneable).Clone();
			}
			return null;
		case "DisplayName":
		case "TypeName":
			if (value != null)
			{
				return value;
			}
			return string.Empty;
		case "Filter":
			if (value != null)
			{
				return value;
			}
			return new ToolboxItemFilterAttribute[0];
		default:
			return value;
		}
	}

	/// <summary>Loads the state of the toolbox item from the specified serialization information object.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to load from. </param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that indicates the stream characteristics. </param>
	protected virtual void Deserialize(SerializationInfo info, StreamingContext context)
	{
		AssemblyName = (AssemblyName)info.GetValue("AssemblyName", typeof(AssemblyName));
		Bitmap = (Bitmap)info.GetValue("Bitmap", typeof(Bitmap));
		Filter = (ICollection)info.GetValue("Filter", typeof(ICollection));
		DisplayName = info.GetString("DisplayName");
		locked = info.GetBoolean("Locked");
		TypeName = info.GetString("TypeName");
	}

	/// <summary>Determines whether two <see cref="T:System.Drawing.Design.ToolboxItem" /> instances are equal.</summary>
	/// <returns>true if the specified <see cref="T:System.Drawing.Design.ToolboxItem" /> is equal to the current <see cref="T:System.Drawing.Design.ToolboxItem" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Drawing.Design.ToolboxItem" /> to compare with the current <see cref="T:System.Drawing.Design.ToolboxItem" />.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is ToolboxItem toolboxItem))
		{
			return false;
		}
		if (obj == this)
		{
			return true;
		}
		if (toolboxItem.AssemblyName.Equals(AssemblyName) && toolboxItem.Locked.Equals(locked) && toolboxItem.TypeName.Equals(TypeName) && toolboxItem.DisplayName.Equals(DisplayName))
		{
			return toolboxItem.Bitmap.Equals(Bitmap);
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Drawing.Design.ToolboxItem" />.</returns>
	public override int GetHashCode()
	{
		return (TypeName + DisplayName).GetHashCode();
	}

	/// <summary>Enables access to the type associated with the toolbox item.</summary>
	/// <returns>The type associated with the toolbox item.</returns>
	/// <param name="host">The designer host to query for <see cref="T:System.ComponentModel.Design.ITypeResolutionService" />.</param>
	public Type GetType(IDesignerHost host)
	{
		return GetType(host, AssemblyName, TypeName, reference: false);
	}

	/// <summary>Creates an instance of the specified type, optionally using a specified designer host and assembly name.</summary>
	/// <returns>An instance of the specified type, if it can be located.</returns>
	/// <param name="host">The <see cref="T:System.ComponentModel.Design.IDesignerHost" /> for the current document. This can be null. </param>
	/// <param name="assemblyName">An <see cref="T:System.Reflection.AssemblyName" /> that indicates the assembly that contains the type to load. This can be null. </param>
	/// <param name="typeName">The name of the type to create an instance of. </param>
	/// <param name="reference">A value indicating whether or not to add a reference to the assembly that contains the specified type to the designer host's set of references. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is not specified. </exception>
	protected virtual Type GetType(IDesignerHost host, AssemblyName assemblyName, string typeName, bool reference)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (host == null)
		{
			return null;
		}
		ITypeResolutionService typeResolutionService = host.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;
		Type result = null;
		if (typeResolutionService != null)
		{
			typeResolutionService.GetAssembly(assemblyName, throwOnError: true);
			if (reference)
			{
				typeResolutionService.ReferenceAssembly(assemblyName);
			}
			result = typeResolutionService.GetType(typeName, throwOnError: true);
		}
		else
		{
			Assembly assembly = Assembly.Load(assemblyName);
			if (assembly != null)
			{
				result = assembly.GetType(typeName);
			}
		}
		return result;
	}

	/// <summary>Initializes the current toolbox item with the specified type to create.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> that the toolbox item creates. </param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Drawing.Design.ToolboxItem" /> was locked. </exception>
	public virtual void Initialize(Type type)
	{
		CheckUnlocked();
		if (type == null)
		{
			return;
		}
		AssemblyName = type.Assembly.GetName();
		DisplayName = type.Name;
		TypeName = type.FullName;
		Image image = null;
		object[] customAttributes = type.GetCustomAttributes(inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i] is ToolboxBitmapAttribute toolboxBitmapAttribute)
			{
				image = toolboxBitmapAttribute.GetImage(type);
				break;
			}
		}
		if (image == null)
		{
			image = ToolboxBitmapAttribute.GetImageFromResource(type, null, large: false);
		}
		if (image != null)
		{
			Bitmap = image as Bitmap;
			if (Bitmap == null)
			{
				Bitmap = new Bitmap(image);
			}
		}
		Filter = type.GetCustomAttributes(typeof(ToolboxItemFilterAttribute), inherit: true);
	}

	/// <summary>For a description of this member, see the <see cref="M:System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)" /> method.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
	/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		Serialize(info, context);
	}

	/// <summary>Locks the toolbox item and prevents changes to its properties.</summary>
	public virtual void Lock()
	{
		locked = true;
	}

	/// <summary>Raises the <see cref="E:System.Drawing.Design.ToolboxItem.ComponentsCreated" /> event.</summary>
	/// <param name="args">A <see cref="T:System.Drawing.Design.ToolboxComponentsCreatedEventArgs" /> that provides data for the event. </param>
	protected virtual void OnComponentsCreated(ToolboxComponentsCreatedEventArgs args)
	{
		if (this.ComponentsCreated != null)
		{
			this.ComponentsCreated(this, args);
		}
	}

	/// <summary>Raises the <see cref="E:System.Drawing.Design.ToolboxItem.ComponentsCreating" /> event.</summary>
	/// <param name="args">A <see cref="T:System.Drawing.Design.ToolboxComponentsCreatingEventArgs" /> that provides data for the event. </param>
	protected virtual void OnComponentsCreating(ToolboxComponentsCreatingEventArgs args)
	{
		if (this.ComponentsCreating != null)
		{
			this.ComponentsCreating(this, args);
		}
	}

	/// <summary>Saves the state of the toolbox item to the specified serialization information object.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to save to. </param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that indicates the stream characteristics. </param>
	protected virtual void Serialize(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("AssemblyName", AssemblyName);
		info.AddValue("Bitmap", Bitmap);
		info.AddValue("Filter", Filter);
		info.AddValue("DisplayName", DisplayName);
		info.AddValue("Locked", locked);
		info.AddValue("TypeName", TypeName);
	}

	/// <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Drawing.Design.ToolboxItem" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:System.Drawing.Design.ToolboxItem" />.</returns>
	public override string ToString()
	{
		return DisplayName;
	}

	/// <summary>Validates that an object is of a given type.</summary>
	/// <param name="propertyName">The name of the property to validate.</param>
	/// <param name="value">Optional value against which to validate.</param>
	/// <param name="expectedType">The expected type of the property.</param>
	/// <param name="allowNull">true to allow null; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null, and <paramref name="allowNull" /> is false.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not the type specified by <paramref name="expectedType" />.</exception>
	protected void ValidatePropertyType(string propertyName, object value, Type expectedType, bool allowNull)
	{
		if (!allowNull && value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value != null && !expectedType.Equals(value.GetType()))
		{
			throw new ArgumentException(global::Locale.GetText("Type mismatch between value ({0}) and expected type ({1}).", value.GetType(), expectedType), "value");
		}
	}

	/// <summary>Validates a property before it is assigned to the property dictionary.</summary>
	/// <returns>The value used to perform validation.</returns>
	/// <param name="propertyName">The name of the property to validate.</param>
	/// <param name="value">The value against which to validate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null, and <paramref name="propertyName" /> is "IsTransient".</exception>
	protected virtual object ValidatePropertyValue(string propertyName, object value)
	{
		switch (propertyName)
		{
		case "AssemblyName":
			ValidatePropertyType(propertyName, value, typeof(AssemblyName), allowNull: true);
			break;
		case "Bitmap":
			ValidatePropertyType(propertyName, value, typeof(Bitmap), allowNull: true);
			break;
		case "Company":
		case "Description":
		case "DisplayName":
		case "TypeName":
			ValidatePropertyType(propertyName, value, typeof(string), allowNull: true);
			if (value == null)
			{
				value = string.Empty;
			}
			break;
		case "IsTransient":
			ValidatePropertyType(propertyName, value, typeof(bool), allowNull: false);
			break;
		case "Filter":
			ValidatePropertyType(propertyName, value, typeof(ToolboxItemFilterAttribute[]), allowNull: true);
			if (value == null)
			{
				value = new ToolboxItemFilterAttribute[0];
			}
			break;
		case "DependentAssemblies":
			ValidatePropertyType(propertyName, value, typeof(AssemblyName[]), allowNull: true);
			break;
		}
		return value;
	}

	private void SetValue(string propertyName, object value)
	{
		CheckUnlocked();
		properties[propertyName] = ValidatePropertyValue(propertyName, value);
	}

	private string GetValue(string propertyName)
	{
		string text = (string)properties[propertyName];
		if (text != null)
		{
			return text;
		}
		return string.Empty;
	}
}
