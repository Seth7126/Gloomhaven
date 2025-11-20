using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes;

/// <summary>Exposes a <see cref="T:System.IO.Stream" /> around a named pipe, which supports both synchronous and asynchronous read and write operations.</summary>
public sealed class NamedPipeClientStream : PipeStream
{
	private const int CancellationCheckInterval = 50;

	private readonly string _normalizedPipePath;

	private readonly TokenImpersonationLevel _impersonationLevel;

	private readonly PipeOptions _pipeOptions;

	private readonly HandleInheritability _inheritability;

	private readonly PipeDirection _direction;

	private int _access;

	/// <summary>Gets the number of server instances that share the same pipe name.</summary>
	/// <returns>The number of server instances that share the same pipe name.</returns>
	/// <exception cref="T:System.InvalidOperationException">The pipe handle has not been set.-or-The current <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> object has not yet connected to a <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> object.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe is broken or an I/O error occurred.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The underlying pipe handle is closed.</exception>
	public int NumberOfServerInstances
	{
		get
		{
			CheckPipePropertyOperations();
			if (!global::Interop.Kernel32.GetNamedPipeHandleState(base.InternalHandle, IntPtr.Zero, out var lpCurInstances, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0))
			{
				throw WinIOError(Marshal.GetLastWin32Error());
			}
			return lpCurInstances;
		}
	}

	private bool TryConnect(int timeout, CancellationToken cancellationToken)
	{
		global::Interop.Kernel32.SECURITY_ATTRIBUTES secAttrs = PipeStream.GetSecAttrs(_inheritability);
		int num = (int)(_pipeOptions & ~PipeOptions.CurrentUserOnly);
		if (_impersonationLevel != TokenImpersonationLevel.None)
		{
			num |= 0x100000;
			num |= (int)(_impersonationLevel - 1) << 16;
		}
		int num2 = _access;
		if ((PipeDirection.In & _direction) != 0)
		{
			num2 |= int.MinValue;
		}
		if ((PipeDirection.Out & _direction) != 0)
		{
			num2 |= 0x40000000;
		}
		SafePipeHandle safePipeHandle = global::Interop.Kernel32.CreateNamedPipeClient(_normalizedPipePath, num2, FileShare.None, ref secAttrs, FileMode.Open, num, IntPtr.Zero);
		if (safePipeHandle.IsInvalid)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 231 && lastWin32Error != 2)
			{
				throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error);
			}
			if (!global::Interop.Kernel32.WaitNamedPipe(_normalizedPipePath, timeout))
			{
				lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error == 2 || lastWin32Error == 121)
				{
					return false;
				}
				throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error);
			}
			safePipeHandle = global::Interop.Kernel32.CreateNamedPipeClient(_normalizedPipePath, num2, FileShare.None, ref secAttrs, FileMode.Open, num, IntPtr.Zero);
			if (safePipeHandle.IsInvalid)
			{
				lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error == 231)
				{
					return false;
				}
				throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error);
			}
		}
		InitializeHandle(safePipeHandle, isExposed: false, (_pipeOptions & PipeOptions.Asynchronous) != 0);
		base.State = PipeState.Connected;
		ValidateRemotePipeUser();
		return true;
	}

	private void ValidateRemotePipeUser()
	{
		if (!base.IsCurrentUserOnly)
		{
			return;
		}
		IdentityReference owner = GetAccessControl().GetOwner(typeof(SecurityIdentifier));
		using WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
		SecurityIdentifier owner2 = windowsIdentity.Owner;
		if (owner != owner2)
		{
			base.State = PipeState.Closed;
			throw new UnauthorizedAccessException("Could not connect to the pipe because it was not owned by the current user.");
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe name.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".</exception>
	public NamedPipeClientStream(string pipeName)
		: this(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".</exception>
	public NamedPipeClientStream(string serverName, string pipeName)
		: this(serverName, pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names, and the specified pipe direction.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.</exception>
	public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction)
		: this(serverName, pipeName, direction, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names, and the specified pipe direction and pipe options.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.</exception>
	public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options)
		: this(serverName, pipeName, direction, options, TokenImpersonationLevel.None, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names, and the specified pipe direction, pipe options, and security impersonation level.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="impersonationLevel">One of the enumeration values that determines the security impersonation level.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="impersonationLevel" /> is not a valid <see cref="T:System.Security.Principal.TokenImpersonationLevel" /> value.</exception>
	public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options, TokenImpersonationLevel impersonationLevel)
		: this(serverName, pipeName, direction, options, impersonationLevel, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names, and the specified pipe direction, pipe options, security impersonation level, and inheritability mode.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="impersonationLevel">One of the enumeration values that determines the security impersonation level.</param>
	/// <param name="inheritability">One of the enumeration values that determines whether the underlying handle will be inheritable by child processes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="impersonationLevel" /> is not a valid <see cref="T:System.Security.Principal.TokenImpersonationLevel" /> value.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> value.</exception>
	public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
		: base(direction, 0)
	{
		if (pipeName == null)
		{
			throw new ArgumentNullException("pipeName");
		}
		if (serverName == null)
		{
			throw new ArgumentNullException("serverName", "serverName cannot be null. Use \\\".\\\" for current machine.");
		}
		if (pipeName.Length == 0)
		{
			throw new ArgumentException("pipeName cannot be an empty string.");
		}
		if (serverName.Length == 0)
		{
			throw new ArgumentException("serverName cannot be an empty string.  Use \\\\\\\".\\\\\\\" for current machine.");
		}
		if ((options & (PipeOptions)536870911) != PipeOptions.None)
		{
			throw new ArgumentOutOfRangeException("options", "options contains an invalid flag.");
		}
		if (impersonationLevel < TokenImpersonationLevel.None || impersonationLevel > TokenImpersonationLevel.Delegation)
		{
			throw new ArgumentOutOfRangeException("impersonationLevel", "TokenImpersonationLevel.None, TokenImpersonationLevel.Anonymous, TokenImpersonationLevel.Identification, TokenImpersonationLevel.Impersonation or TokenImpersonationLevel.Delegation required.");
		}
		if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable)
		{
			throw new ArgumentOutOfRangeException("inheritability", "HandleInheritability.None or HandleInheritability.Inheritable required.");
		}
		if ((options & PipeOptions.CurrentUserOnly) != PipeOptions.None)
		{
			base.IsCurrentUserOnly = true;
		}
		_normalizedPipePath = PipeStream.GetPipePath(serverName, pipeName);
		_direction = direction;
		_inheritability = inheritability;
		_impersonationLevel = impersonationLevel;
		_pipeOptions = options;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class for the specified pipe handle with the specified pipe direction.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="isAsync">true to indicate that the handle was opened asynchronously; otherwise, false.</param>
	/// <param name="isConnected">true to indicate that the pipe is connected; otherwise, false.</param>
	/// <param name="safePipeHandle">A safe handle for the pipe that this <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> object will encapsulate.</param>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="safePipeHandle" /> is not a valid handle.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="safePipeHandle" /> is not a valid handle.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="safePipeHandle" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.</exception>
	/// <exception cref="T:System.IO.IOException">The stream has been closed. </exception>
	public NamedPipeClientStream(PipeDirection direction, bool isAsync, bool isConnected, SafePipeHandle safePipeHandle)
		: base(direction, 0)
	{
		if (safePipeHandle == null)
		{
			throw new ArgumentNullException("safePipeHandle");
		}
		if (safePipeHandle.IsInvalid)
		{
			throw new ArgumentException("Invalid handle.", "safePipeHandle");
		}
		ValidateHandleIsPipe(safePipeHandle);
		InitializeHandle(safePipeHandle, isExposed: true, isAsync);
		if (isConnected)
		{
			base.State = PipeState.Connected;
		}
	}

	~NamedPipeClientStream()
	{
		Dispose(disposing: false);
	}

	/// <summary>Connects to a waiting server with an infinite time-out value.</summary>
	/// <exception cref="T:System.InvalidOperationException">The client is already connected.</exception>
	public void Connect()
	{
		Connect(-1);
	}

	/// <summary>Connects to a waiting server within the specified time-out period.</summary>
	/// <param name="timeout">The number of milliseconds to wait for the server to respond before the connection times out.</param>
	/// <exception cref="T:System.TimeoutException">Could not connect to the server within the specified <paramref name="timeout" /> period.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is less than 0 and not set to <see cref="F:System.Threading.Timeout.Infinite" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The client is already connected.</exception>
	/// <exception cref="T:System.IO.IOException">The server is connected to another client and the time-out period has expired.</exception>
	public void Connect(int timeout)
	{
		CheckConnectOperationsClient();
		if (timeout < 0 && timeout != -1)
		{
			throw new ArgumentOutOfRangeException("timeout", "Timeout must be non-negative or equal to -1 (Timeout.Infinite)");
		}
		ConnectInternal(timeout, CancellationToken.None, Environment.TickCount);
	}

	private void ConnectInternal(int timeout, CancellationToken cancellationToken, int startTime)
	{
		int num = 0;
		SpinWait spinWait = default(SpinWait);
		do
		{
			cancellationToken.ThrowIfCancellationRequested();
			int num2 = timeout - num;
			if (cancellationToken.CanBeCanceled && num2 > 50)
			{
				num2 = 50;
			}
			if (TryConnect(num2, cancellationToken))
			{
				return;
			}
			spinWait.SpinOnce();
		}
		while (timeout == -1 || (num = Environment.TickCount - startTime) < timeout);
		throw new TimeoutException();
	}

	public Task ConnectAsync()
	{
		return ConnectAsync(-1, CancellationToken.None);
	}

	public Task ConnectAsync(int timeout)
	{
		return ConnectAsync(timeout, CancellationToken.None);
	}

	public Task ConnectAsync(CancellationToken cancellationToken)
	{
		return ConnectAsync(-1, cancellationToken);
	}

	public Task ConnectAsync(int timeout, CancellationToken cancellationToken)
	{
		CheckConnectOperationsClient();
		if (timeout < 0 && timeout != -1)
		{
			throw new ArgumentOutOfRangeException("timeout", "Timeout must be non-negative or equal to -1 (Timeout.Infinite)");
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled(cancellationToken);
		}
		int startTime = Environment.TickCount;
		return Task.Run(delegate
		{
			ConnectInternal(timeout, cancellationToken, startTime);
		}, cancellationToken);
	}

	protected internal override void CheckPipePropertyOperations()
	{
		base.CheckPipePropertyOperations();
		if (base.State == PipeState.WaitingToConnect)
		{
			throw new InvalidOperationException("Pipe hasn't been connected yet.");
		}
		if (base.State == PipeState.Broken)
		{
			throw new IOException("Pipe is broken.");
		}
	}

	private void CheckConnectOperationsClient()
	{
		if (base.State == PipeState.Connected)
		{
			throw new InvalidOperationException("Already in a connected state.");
		}
		if (base.State == PipeState.Closed)
		{
			throw Error.GetPipeNotOpen();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeClientStream" /> class with the specified pipe and server names, and the specified pipe options, security impersonation level, and inheritability mode.</summary>
	/// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="desiredAccessRights">One of the enumeration values that specifies the desired access rights of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="impersonationLevel">One of the enumeration values that determines the security impersonation level.</param>
	/// <param name="inheritability">One of the enumeration values that determines whether the underlying handle will be inheritable by child processes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> or <paramref name="serverName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="impersonationLevel" /> is not a valid <see cref="T:System.Security.Principal.TokenImpersonationLevel" /> value.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> value.</exception>
	public NamedPipeClientStream(string serverName, string pipeName, PipeAccessRights desiredAccessRights, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
		: this(serverName, pipeName, (PipeDirection)(desiredAccessRights & (PipeAccessRights.ReadData | PipeAccessRights.WriteData)), options, impersonationLevel, inheritability)
	{
		if ((desiredAccessRights & ~(PipeAccessRights.FullControl | PipeAccessRights.AccessSystemSecurity)) != 0)
		{
			throw new ArgumentOutOfRangeException("desiredAccessRights", "Invalid PipeAccessRights flag.");
		}
		_access = (int)desiredAccessRights;
	}
}
