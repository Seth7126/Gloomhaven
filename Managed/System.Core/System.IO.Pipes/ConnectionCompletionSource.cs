namespace System.IO.Pipes;

internal sealed class ConnectionCompletionSource : PipeCompletionSource<VoidResult>
{
	private readonly NamedPipeServerStream _serverStream;

	internal ConnectionCompletionSource(NamedPipeServerStream server)
		: base(server._threadPoolBinding, ReadOnlyMemory<byte>.Empty)
	{
		_serverStream = server;
	}

	internal override void SetCompletedSynchronously()
	{
		_serverStream.State = PipeState.Connected;
		TrySetResult(default(VoidResult));
	}

	protected override void AsyncCallback(uint errorCode, uint numBytes)
	{
		if (errorCode == 535)
		{
			errorCode = 0u;
		}
		base.AsyncCallback(errorCode, numBytes);
	}

	protected override void HandleError(int errorCode)
	{
		TrySetException(Win32Marshal.GetExceptionForWin32Error(errorCode));
	}

	protected override void HandleUnexpectedCancellation()
	{
		TrySetException(Error.GetOperationAborted());
	}
}
