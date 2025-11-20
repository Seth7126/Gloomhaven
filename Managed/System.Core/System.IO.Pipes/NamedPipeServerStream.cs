using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes;

/// <summary>Exposes a <see cref="T:System.IO.Stream" /> around a named pipe, supporting both synchronous and asynchronous read and write operations.</summary>
public sealed class NamedPipeServerStream : PipeStream
{
	internal class ExecuteHelper
	{
		internal PipeStreamImpersonationWorker _userCode;

		internal SafePipeHandle _handle;

		internal bool _mustRevert;

		internal int _impersonateErrorCode;

		internal int _revertImpersonateErrorCode;

		internal ExecuteHelper(PipeStreamImpersonationWorker userCode, SafePipeHandle handle)
		{
			_userCode = userCode;
			_handle = handle;
		}
	}

	private static RuntimeHelpers.TryCode tryCode = ImpersonateAndTryCode;

	private static RuntimeHelpers.CleanupCode cleanupCode = RevertImpersonationOnBackout;

	/// <summary>Represents the maximum number of server instances that the system resources allow.</summary>
	public const int MaxAllowedServerInstances = -1;

	private void Create(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, HandleInheritability inheritability)
	{
		Create(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, null, inheritability, (PipeAccessRights)0);
	}

	private void Create(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability, PipeAccessRights additionalAccessRights)
	{
		string fullPath = Path.GetFullPath("\\\\.\\pipe\\" + pipeName);
		if (string.Equals(fullPath, "\\\\.\\pipe\\anonymous", StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentOutOfRangeException("pipeName", "The pipeName \\\"anonymous\\\" is reserved.");
		}
		if (base.IsCurrentUserOnly)
		{
			using (WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent())
			{
				SecurityIdentifier owner = windowsIdentity.Owner;
				PipeAccessRule rule = new PipeAccessRule(owner, PipeAccessRights.FullControl, AccessControlType.Allow);
				pipeSecurity = new PipeSecurity();
				pipeSecurity.AddAccessRule(rule);
				pipeSecurity.SetOwner(owner);
			}
			options &= ~PipeOptions.CurrentUserOnly;
		}
		int openMode = (int)((uint)direction | (uint)((maxNumberOfServerInstances == 1) ? 524288 : 0) | (uint)options) | (int)additionalAccessRights;
		int pipeMode = ((int)transmissionMode << 2) | ((int)transmissionMode << 1);
		if (maxNumberOfServerInstances == -1)
		{
			maxNumberOfServerInstances = 255;
		}
		GCHandle pinningHandle = default(GCHandle);
		try
		{
			global::Interop.Kernel32.SECURITY_ATTRIBUTES securityAttributes = PipeStream.GetSecAttrs(inheritability, pipeSecurity, ref pinningHandle);
			SafePipeHandle safePipeHandle = global::Interop.Kernel32.CreateNamedPipe(fullPath, openMode, pipeMode, maxNumberOfServerInstances, outBufferSize, inBufferSize, 0, ref securityAttributes);
			if (safePipeHandle.IsInvalid)
			{
				throw Win32Marshal.GetExceptionForLastWin32Error();
			}
			InitializeHandle(safePipeHandle, isExposed: false, (options & PipeOptions.Asynchronous) != 0);
		}
		finally
		{
			if (pinningHandle.IsAllocated)
			{
				pinningHandle.Free();
			}
		}
	}

	/// <summary>Waits for a client to connect to this <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> object.</summary>
	/// <exception cref="T:System.InvalidOperationException">A pipe connection has already been established.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe connection has been broken.</exception>
	public void WaitForConnection()
	{
		CheckConnectOperationsServerWithHandle();
		if (base.IsAsync)
		{
			WaitForConnectionCoreAsync(CancellationToken.None).GetAwaiter().GetResult();
			return;
		}
		if (!global::Interop.Kernel32.ConnectNamedPipe(base.InternalHandle, IntPtr.Zero))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 535)
			{
				throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error);
			}
			if (lastWin32Error == 535 && base.State == PipeState.Connected)
			{
				throw new InvalidOperationException("Already in a connected state.");
			}
		}
		base.State = PipeState.Connected;
	}

	public Task WaitForConnectionAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled(cancellationToken);
		}
		if (!base.IsAsync)
		{
			return Task.Factory.StartNew(delegate(object s)
			{
				((NamedPipeServerStream)s).WaitForConnection();
			}, this, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
		}
		return WaitForConnectionCoreAsync(cancellationToken);
	}

	/// <summary>Disconnects the current connection.</summary>
	/// <exception cref="T:System.InvalidOperationException">No pipe connections have been made yet.-or-The connected pipe has already disconnected.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	public void Disconnect()
	{
		CheckDisconnectOperations();
		if (!global::Interop.Kernel32.DisconnectNamedPipe(base.InternalHandle))
		{
			throw Win32Marshal.GetExceptionForLastWin32Error();
		}
		base.State = PipeState.Disconnected;
	}

	/// <summary>Gets the user name of the client on the other end of the pipe.</summary>
	/// <returns>The user name of the client on the other end of the pipe.</returns>
	/// <exception cref="T:System.InvalidOperationException">No pipe connections have been made yet.-or-The connected pipe has already disconnected.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe connection has been broken.-or-The user name of the client is longer than 19 characters.</exception>
	public string GetImpersonationUserName()
	{
		CheckWriteOperations();
		StringBuilder stringBuilder = new StringBuilder(514);
		if (!global::Interop.Kernel32.GetNamedPipeHandleState(base.InternalHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, stringBuilder, stringBuilder.Capacity))
		{
			throw WinIOError(Marshal.GetLastWin32Error());
		}
		return stringBuilder.ToString();
	}

	/// <summary>Calls a delegate while impersonating the client.</summary>
	/// <param name="impersonationWorker">The delegate that specifies a method to call.</param>
	/// <exception cref="T:System.InvalidOperationException">No pipe connections have been made yet.-or-The connected pipe has already disconnected.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe connection has been broken.-or-An I/O error occurred.</exception>
	public void RunAsClient(PipeStreamImpersonationWorker impersonationWorker)
	{
		CheckWriteOperations();
		ExecuteHelper executeHelper = new ExecuteHelper(impersonationWorker, base.InternalHandle);
		RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(tryCode, cleanupCode, executeHelper);
		if (executeHelper._impersonateErrorCode != 0)
		{
			throw WinIOError(executeHelper._impersonateErrorCode);
		}
		if (executeHelper._revertImpersonateErrorCode != 0)
		{
			throw WinIOError(executeHelper._revertImpersonateErrorCode);
		}
	}

	private static void ImpersonateAndTryCode(object helper)
	{
		ExecuteHelper executeHelper = (ExecuteHelper)helper;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
		}
		finally
		{
			if (global::Interop.Advapi32.ImpersonateNamedPipeClient(executeHelper._handle))
			{
				executeHelper._mustRevert = true;
			}
			else
			{
				executeHelper._impersonateErrorCode = Marshal.GetLastWin32Error();
			}
		}
		if (executeHelper._mustRevert)
		{
			executeHelper._userCode();
		}
	}

	private static void RevertImpersonationOnBackout(object helper, bool exceptionThrown)
	{
		ExecuteHelper executeHelper = (ExecuteHelper)helper;
		if (executeHelper._mustRevert && !global::Interop.Advapi32.RevertToSelf())
		{
			executeHelper._revertImpersonateErrorCode = Marshal.GetLastWin32Error();
		}
	}

	private unsafe Task WaitForConnectionCoreAsync(CancellationToken cancellationToken)
	{
		CheckConnectOperationsServerWithHandle();
		if (!base.IsAsync)
		{
			throw new InvalidOperationException("Pipe is not opened in asynchronous mode.");
		}
		ConnectionCompletionSource connectionCompletionSource = new ConnectionCompletionSource(this);
		if (!global::Interop.Kernel32.ConnectNamedPipe(base.InternalHandle, connectionCompletionSource.Overlapped))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			switch (lastWin32Error)
			{
			case 535:
				connectionCompletionSource.ReleaseResources();
				if (base.State == PipeState.Connected)
				{
					throw new InvalidOperationException("Already in a connected state.");
				}
				connectionCompletionSource.SetCompletedSynchronously();
				return Task.CompletedTask;
			default:
				connectionCompletionSource.ReleaseResources();
				throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error);
			case 997:
				break;
			}
		}
		connectionCompletionSource.RegisterForCancellation(cancellationToken);
		return connectionCompletionSource.Task;
	}

	private void CheckConnectOperationsServerWithHandle()
	{
		if (base.InternalHandle == null)
		{
			throw new InvalidOperationException("Pipe handle has not been set.  Did your PipeStream implementation call InitializeHandle?");
		}
		CheckConnectOperationsServer();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported. </exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName)
		: this(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name and pipe direction.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction)
		: this(pipeName, direction, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, and maximum number of server instances.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-A non-negative number is required. -or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<see cref="F:System.IO.HandleInheritability.None" />  or <see cref="F:System.IO.HandleInheritability.Inheritable" />  is required.-or-Access rights is limited to the <see cref="F:System.IO.Pipes.PipeAccessRights.ChangePermissions" /> , <see cref="F:System.IO.Pipes.PipeAccessRights.TakeOwnership" /> , and <see cref="F:System.IO.Pipes.PipeAccessRights.AccessSystemSecurity" />  flags.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances)
		: this(pipeName, direction, maxNumberOfServerInstances, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, and transmission mode.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode)
		: this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, PipeOptions.None, 0, 0, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, transmission mode, and pipe options.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options)
		: this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, 0, 0, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, transmission mode, pipe options, and recommended in and out buffer sizes.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="inBufferSize">A positive value greater than 0 that indicates the input buffer size.</param>
	/// <param name="outBufferSize">A positive value greater than 0 that indicates the output buffer size.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="inBufferSize" /> is negative.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize)
		: this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, HandleInheritability.None)
	{
	}

	private NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, HandleInheritability inheritability)
		: base(direction, transmissionMode, outBufferSize)
	{
		if (pipeName == null)
		{
			throw new ArgumentNullException("pipeName");
		}
		if (pipeName.Length == 0)
		{
			throw new ArgumentException("pipeName cannot be an empty string.");
		}
		if ((options & (PipeOptions)536870911) != PipeOptions.None)
		{
			throw new ArgumentOutOfRangeException("options", "options contains an invalid flag.");
		}
		if (inBufferSize < 0)
		{
			throw new ArgumentOutOfRangeException("inBufferSize", "Non negative number is required.");
		}
		if ((maxNumberOfServerInstances < 1 || maxNumberOfServerInstances > 254) && maxNumberOfServerInstances != -1)
		{
			throw new ArgumentOutOfRangeException("maxNumberOfServerInstances", "maxNumberOfServerInstances must either be a value between 1 and 254, or NamedPipeServerStream.MaxAllowedServerInstances (to obtain the maximum number allowed by system resources).");
		}
		if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable)
		{
			throw new ArgumentOutOfRangeException("inheritability", "HandleInheritability.None or HandleInheritability.Inheritable required.");
		}
		if ((options & PipeOptions.CurrentUserOnly) != PipeOptions.None)
		{
			base.IsCurrentUserOnly = true;
		}
		Create(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, inheritability);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class from the specified pipe handle.</summary>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="isAsync">true to indicate that the handle was opened asynchronously; otherwise, false.</param>
	/// <param name="isConnected">true to indicate that the pipe is connected; otherwise, false.</param>
	/// <param name="safePipeHandle">A safe handle for the pipe that this <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> object will encapsulate.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="safePipeHandle" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="safePipeHandle" /> is an invalid handle.</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="safePipeHandle" /> is not a valid pipe handle.-or-The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(PipeDirection direction, bool isAsync, bool isConnected, SafePipeHandle safePipeHandle)
		: base(direction, PipeTransmissionMode.Byte, 0)
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

	~NamedPipeServerStream()
	{
		Dispose(disposing: false);
	}

	public Task WaitForConnectionAsync()
	{
		return WaitForConnectionAsync(CancellationToken.None);
	}

	/// <summary>Begins an asynchronous operation to wait for a client to connect.</summary>
	/// <returns>An object that references the asynchronous request.</returns>
	/// <param name="callback">The method to call when a client connects to the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> object.</param>
	/// <param name="state">A user-provided object that distinguishes this particular asynchronous request from other requests.</param>
	/// <exception cref="T:System.InvalidOperationException">The pipe was not opened asynchronously.-or-A pipe connection has already been established.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe connection has been broken.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	public IAsyncResult BeginWaitForConnection(AsyncCallback callback, object state)
	{
		return TaskToApm.Begin(WaitForConnectionAsync(), callback, state);
	}

	/// <summary>Ends an asynchronous operation to wait for a client to connect.</summary>
	/// <param name="asyncResult">The pending asynchronous request.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The pipe was not opened asynchronously.-or-The pipe handle has not been set.</exception>
	/// <exception cref="T:System.IO.IOException">The pipe connection has been broken.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The pipe is closed.</exception>
	public void EndWaitForConnection(IAsyncResult asyncResult)
	{
		TaskToApm.End(asyncResult);
	}

	private void CheckConnectOperationsServer()
	{
		if (base.State == PipeState.Closed)
		{
			throw Error.GetPipeNotOpen();
		}
		if (base.InternalHandle != null && base.InternalHandle.IsClosed)
		{
			throw Error.GetPipeNotOpen();
		}
		if (base.State == PipeState.Broken)
		{
			throw new IOException("Pipe is broken.");
		}
	}

	private void CheckDisconnectOperations()
	{
		if (base.State == PipeState.WaitingToConnect)
		{
			throw new InvalidOperationException("Pipe hasn't been connected yet.");
		}
		if (base.State == PipeState.Disconnected)
		{
			throw new InvalidOperationException("Already in a disconnected state.");
		}
		if (base.InternalHandle == null)
		{
			throw new InvalidOperationException("Pipe handle has not been set.  Did your PipeStream implementation call InitializeHandle?");
		}
		if (base.State == PipeState.Closed || (base.InternalHandle != null && base.InternalHandle.IsClosed))
		{
			throw Error.GetPipeNotOpen();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, transmission mode, pipe options, recommended in and out buffer sizes, and pipe security.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="inBufferSize">A positive value greater than 0 that indicates the input buffer size.</param>
	/// <param name="outBufferSize">A positive value greater than 0 that indicates the output buffer size.</param>
	/// <param name="pipeSecurity">An object that determines the access control and audit security for the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="inBufferSize" /> is negative.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity)
		: this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, HandleInheritability.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, transmission mode, pipe options, recommended in and out buffer sizes, pipe security, and inheritability mode.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="inBufferSize">A positive value greater than 0 that indicates the input buffer size.</param>
	/// <param name="outBufferSize">A positive value greater than 0 that indicates the output buffer size.</param>
	/// <param name="pipeSecurity">An object that determines the access control and audit security for the pipe.</param>
	/// <param name="inheritability">One of the enumeration values that determines whether the underlying handle can be inherited by child processes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="inBufferSize" /> is negative.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> value.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability)
		: this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, inheritability)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.NamedPipeServerStream" /> class with the specified pipe name, pipe direction, maximum number of server instances, transmission mode, pipe options, recommended in and out buffer sizes, pipe security, inheritability mode, and pipe access rights.</summary>
	/// <param name="pipeName">The name of the pipe.</param>
	/// <param name="direction">One of the enumeration values that determines the direction of the pipe.</param>
	/// <param name="maxNumberOfServerInstances">The maximum number of server instances that share the same name.</param>
	/// <param name="transmissionMode">One of the enumeration values that determines the transmission mode of the pipe.</param>
	/// <param name="options">One of the enumeration values that determines how to open or create the pipe.</param>
	/// <param name="inBufferSize">The input buffer size.</param>
	/// <param name="outBufferSize">The output buffer size.</param>
	/// <param name="pipeSecurity">An object that determines the access control and audit security for the pipe.</param>
	/// <param name="inheritability">One of the enumeration values that determines whether the underlying handle can be inherited by child processes.</param>
	/// <param name="additionalAccessRights">One of the enumeration values that specifies the access rights of the pipe.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pipeName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pipeName" /> is a zero-length string.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="pipeName" /> is set to "anonymous".-or-<paramref name="direction" /> is not a valid <see cref="T:System.IO.Pipes.PipeDirection" /> value.-or-<paramref name="maxNumberofServerInstances" /> is less than one or greater than 254.-or-<paramref name="options" /> is not a valid <see cref="T:System.IO.Pipes.PipeOptions" /> value.-or-<paramref name="inBufferSize" /> is negative.-or-<paramref name="inheritability" /> is not a valid <see cref="T:System.IO.HandleInheritability" /> value.-or-<paramref name="additionalAccessRights" /> is not a valid <see cref="T:System.IO.Pipes.PipeAccessRights" /> value.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="pipeName" /> contains a colon (":").</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows Millennium Edition, Windows 98, or Windows 95, which are not supported.</exception>
	/// <exception cref="T:System.IO.IOException">The maximum number of server instances has been exceeded.</exception>
	public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability, PipeAccessRights additionalAccessRights)
		: base(direction, transmissionMode, outBufferSize)
	{
		if (pipeName == null)
		{
			throw new ArgumentNullException("pipeName");
		}
		if (pipeName.Length == 0)
		{
			throw new ArgumentException("pipeName cannot be an empty string.");
		}
		if ((options & (PipeOptions)1073741823) != PipeOptions.None)
		{
			throw new ArgumentOutOfRangeException("options", "options contains an invalid flag.");
		}
		if (inBufferSize < 0)
		{
			throw new ArgumentOutOfRangeException("inBufferSize", "Non negative number is required.");
		}
		if ((maxNumberOfServerInstances < 1 || maxNumberOfServerInstances > 254) && maxNumberOfServerInstances != -1)
		{
			throw new ArgumentOutOfRangeException("maxNumberOfServerInstances", "maxNumberOfServerInstances must either be a value between 1 and 254, or NamedPipeServerStream.MaxAllowedServerInstances (to obtain the maximum number allowed by system resources).");
		}
		if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable)
		{
			throw new ArgumentOutOfRangeException("inheritability", "HandleInheritability.None or HandleInheritability.Inheritable required.");
		}
		if ((additionalAccessRights & ~(PipeAccessRights.ChangePermissions | PipeAccessRights.TakeOwnership | PipeAccessRights.AccessSystemSecurity)) != 0)
		{
			throw new ArgumentOutOfRangeException("additionalAccessRights", "additionalAccessRights is limited to the PipeAccessRights.ChangePermissions, PipeAccessRights.TakeOwnership, and PipeAccessRights.AccessSystemSecurity flags when creating NamedPipeServerStreams.");
		}
		Create(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, pipeSecurity, inheritability, additionalAccessRights);
	}
}
