using System.Collections;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Enables and configures object pooling for a component. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class)]
[ComVisible(false)]
public sealed class ObjectPoolingAttribute : Attribute, IConfigurationAttribute
{
	private int creationTimeout;

	private bool enabled;

	private int minPoolSize;

	private int maxPoolSize;

	/// <summary>Gets or sets the length of time to wait for an object to become available in the pool before throwing an exception. This value is in milliseconds.</summary>
	/// <returns>The time-out value in milliseconds.</returns>
	public int CreationTimeout
	{
		get
		{
			return creationTimeout;
		}
		set
		{
			creationTimeout = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether object pooling is enabled.</summary>
	/// <returns>true if object pooling is enabled; otherwise, false. The default is true.</returns>
	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
		}
	}

	/// <summary>Gets or sets the value for the maximum size of the pool.</summary>
	/// <returns>The maximum number of objects in the pool.</returns>
	public int MaxPoolSize
	{
		get
		{
			return maxPoolSize;
		}
		set
		{
			maxPoolSize = value;
		}
	}

	/// <summary>Gets or sets the value for the minimum size of the pool.</summary>
	/// <returns>The minimum number of objects in the pool.</returns>
	public int MinPoolSize
	{
		get
		{
			return minPoolSize;
		}
		set
		{
			minPoolSize = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ObjectPoolingAttribute" /> class and sets the <see cref="P:System.EnterpriseServices.ObjectPoolingAttribute.Enabled" />, <see cref="P:System.EnterpriseServices.ObjectPoolingAttribute.MaxPoolSize" />, <see cref="P:System.EnterpriseServices.ObjectPoolingAttribute.MinPoolSize" />, and <see cref="P:System.EnterpriseServices.ObjectPoolingAttribute.CreationTimeout" /> properties to their default values.</summary>
	public ObjectPoolingAttribute()
		: this(enable: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ObjectPoolingAttribute" /> class and sets the <see cref="P:System.EnterpriseServices.ObjectPoolingAttribute.Enabled" /> property.</summary>
	/// <param name="enable">true to enable object pooling; otherwise, false. </param>
	public ObjectPoolingAttribute(bool enable)
	{
		enabled = enable;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ObjectPoolingAttribute" /> class and sets the <see cref="P:System.EnterpriseServices.ObjectPoolingAttribute.MaxPoolSize" /> and <see cref="P:System.EnterpriseServices.ObjectPoolingAttribute.MinPoolSize" /> properties.</summary>
	/// <param name="minPoolSize">The minimum pool size. </param>
	/// <param name="maxPoolSize">The maximum pool size. </param>
	public ObjectPoolingAttribute(int minPoolSize, int maxPoolSize)
		: this(enable: true, minPoolSize, maxPoolSize)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ObjectPoolingAttribute" /> class and sets the <see cref="P:System.EnterpriseServices.ObjectPoolingAttribute.Enabled" />, <see cref="P:System.EnterpriseServices.ObjectPoolingAttribute.MaxPoolSize" />, and <see cref="P:System.EnterpriseServices.ObjectPoolingAttribute.MinPoolSize" /> properties.</summary>
	/// <param name="enable">true to enable object pooling; otherwise, false. </param>
	/// <param name="minPoolSize">The minimum pool size.</param>
	/// <param name="maxPoolSize">The maximum pool size.</param>
	public ObjectPoolingAttribute(bool enable, int minPoolSize, int maxPoolSize)
	{
		enabled = enable;
		this.minPoolSize = minPoolSize;
		this.maxPoolSize = maxPoolSize;
	}

	/// <summary>Called internally by the .NET Framework infrastructure while installing and configuring assemblies in the COM+ catalog.</summary>
	/// <returns>true if the method has made changes.</returns>
	/// <param name="info">A hash table that contains internal objects referenced by internal keys.</param>
	[System.MonoTODO]
	public bool AfterSaveChanges(Hashtable info)
	{
		throw new NotImplementedException();
	}

	/// <summary>Called internally by the .NET Framework infrastructure while applying the <see cref="T:System.EnterpriseServices.ObjectPoolingAttribute" /> class attribute to a serviced component.</summary>
	/// <returns>true if the method has made changes.</returns>
	/// <param name="info">A hash table that contains an internal object to which object pooling properties are applied, referenced by an internal key.</param>
	[System.MonoTODO]
	public bool Apply(Hashtable info)
	{
		throw new NotImplementedException();
	}

	/// <summary>Called internally by the .NET Framework infrastructure while installing and configuring assemblies in the COM+ catalog.</summary>
	/// <returns>true if the attribute is applied to a serviced component class.</returns>
	/// <param name="s">A string generated by the .NET Framework infrastructure that is checked for a special value that indicates a serviced component.</param>
	[System.MonoTODO]
	public bool IsValidTarget(string s)
	{
		throw new NotImplementedException();
	}
}
