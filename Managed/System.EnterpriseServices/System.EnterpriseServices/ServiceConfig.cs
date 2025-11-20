using System.Runtime.InteropServices;
using System.Transactions;

namespace System.EnterpriseServices;

/// <summary>Specifies and configures the services that are to be active in the domain which is entered when calling <see cref="M:System.EnterpriseServices.ServiceDomain.Enter(System.EnterpriseServices.ServiceConfig)" /> or creating an <see cref="T:System.EnterpriseServices.Activity" />. This class cannot be inherited.</summary>
[System.MonoTODO]
[ComVisible(false)]
public sealed class ServiceConfig
{
	/// <summary>Gets or sets the binding option, which indicates whether all work submitted by the activity is to be bound to only one single-threaded apartment (STA).</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.BindingOption" /> values. The default is <see cref="F:System.EnterpriseServices.BindingOption.NoBinding" />.</returns>
	[System.MonoTODO]
	public BindingOption Binding
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Transactions.Transaction" /> that represents an existing transaction that supplies the settings used to run the transaction identified by <see cref="T:System.EnterpriseServices.ServiceConfig" />.</summary>
	/// <returns>A <see cref="T:System.Transactions.Transaction" />. The default is null.</returns>
	[System.MonoTODO]
	public Transaction BringYourOwnSystemTransaction
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.EnterpriseServices.ITransaction" /> that represents an existing transaction that supplies the settings used to run the transaction identified by <see cref="T:System.EnterpriseServices.ServiceConfig" />.</summary>
	/// <returns>An <see cref="T:System.EnterpriseServices.ITransaction" />. The default is null.</returns>
	[System.MonoTODO]
	public ITransaction BringYourOwnTransaction
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a value that indicates whether COM Transaction Integrator (COMTI) intrinsics are enabled.</summary>
	/// <returns>true if COMTI intrinsics are enabled; otherwise, false. The default is false.</returns>
	[System.MonoTODO]
	public bool COMTIIntrinsicsEnabled
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a value that indicates whether Internet Information Services (IIS) intrinsics are enabled.</summary>
	/// <returns>true if IIS intrinsics are enabled; otherwise, false. The default is false.</returns>
	[System.MonoTODO]
	public bool IISIntrinsicsEnabled
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a value that indicates whether to construct a new context based on the current context or to create a new context based solely on the information in <see cref="T:System.EnterpriseServices.ServiceConfig" />.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.InheritanceOption" /> values. The default is <see cref="F:System.EnterpriseServices.InheritanceOption.Inherit" />.</returns>
	[System.MonoTODO]
	public InheritanceOption Inheritance
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the isolation level of the transaction.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.TransactionIsolationLevel" /> values. The default is <see cref="F:System.EnterpriseServices.TransactionIsolationLevel.Any" />.</returns>
	[System.MonoTODO]
	public TransactionIsolationLevel IsolationLevel
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the GUID for the COM+ partition that is to be used.</summary>
	/// <returns>The GUID for the partition to be used. The default is a zero GUID.</returns>
	[System.MonoTODO]
	public Guid PartitionId
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a value that indicates how partitions are used for the enclosed work.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.PartitionOption" /> values. The default is <see cref="F:System.EnterpriseServices.PartitionOption.Ignore" />.</returns>
	[System.MonoTODO]
	public PartitionOption PartitionOption
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the directory for the side-by-side assembly for the enclosed work.</summary>
	/// <returns>The name of the directory to be used for the side-by-side assembly. The default value is null.</returns>
	[System.MonoTODO]
	public string SxsDirectory
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the file name of the side-by-side assembly for the enclosed work.</summary>
	/// <returns>The file name of the side-by-side assembly. The default value is null.</returns>
	[System.MonoTODO]
	public string SxsName
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a value that indicates how to configure the side-by-side assembly.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.SxsOption" /> values. The default is <see cref="F:System.EnterpriseServices.SxsOption.Ignore" />.</returns>
	[System.MonoTODO]
	public SxsOption SxsOption
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a value in that indicates the type of automatic synchronization requested by the component.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.SynchronizationOption" /> values. The default is <see cref="F:System.EnterpriseServices.SynchronizationOption.Disabled" />.</returns>
	[System.MonoTODO]
	public SynchronizationOption Synchronization
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a value that indicates the thread pool which runs the work submitted by the activity.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.ThreadPoolOption" /> values. The default is <see cref="F:System.EnterpriseServices.ThreadPoolOption.None" />.</returns>
	[System.MonoTODO]
	public ThreadPoolOption ThreadPool
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the Transaction Internet Protocol (TIP) URL that allows the enclosed code to run in an existing transaction.</summary>
	/// <returns>A TIP URL. The default value is null.</returns>
	[System.MonoTODO]
	public string TipUrl
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a text string that corresponds to the application ID under which tracker information is reported.</summary>
	/// <returns>The application ID under which tracker information is reported. The default value is null.</returns>
	[System.MonoTODO]
	public string TrackingAppName
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a text string that corresponds to the context name under which tracker information is reported.</summary>
	/// <returns>The context name under which tracker information is reported. The default value is null.</returns>
	[System.MonoTODO]
	public string TrackingComponentName
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a value that indicates whether tracking is enabled.</summary>
	/// <returns>true if tracking is enabled; otherwise, false. The default is false.</returns>
	[System.MonoTODO]
	public bool TrackingEnabled
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a value that indicates how transactions are used in the enclosed work.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.TransactionOption" /> values. The default is <see cref="F:System.EnterpriseServices.TransactionOption.Disabled" />.</returns>
	[System.MonoTODO]
	public TransactionOption Transaction
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the name that is used when transaction statistics are displayed.</summary>
	/// <returns>The name used when transaction statistics are displayed. The default value is null.</returns>
	[System.MonoTODO]
	public string TransactionDescription
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the transaction time-out for a new transaction.</summary>
	/// <returns>The transaction time-out, in seconds.</returns>
	[System.MonoTODO]
	public int TransactionTimeout
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ServiceConfig" /> class, setting the properties to configure the desired services.</summary>
	/// <exception cref="T:System.PlatformNotSupportedException">
	///   <see cref="T:System.EnterpriseServices.ServiceConfig" /> is not supported on the current platform. </exception>
	[System.MonoTODO]
	public ServiceConfig()
	{
		throw new NotImplementedException();
	}
}
