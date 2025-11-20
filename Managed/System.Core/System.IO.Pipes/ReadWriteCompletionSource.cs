namespace System.IO.Pipes;

internal sealed class ReadWriteCompletionSource : PipeCompletionSource<int>
{
	private readonly bool _isWrite;

	private readonly PipeStream _pipeStream;

	private bool _isMessageComplete;

	private int _numBytes;

	internal ReadWriteCompletionSource(PipeStream stream, ReadOnlyMemory<byte> bufferToPin, bool isWrite)
		: base(stream._threadPoolBinding, bufferToPin)
	{
		_pipeStream = stream;
		_isWrite = isWrite;
		_isMessageComplete = true;
	}

	internal override void SetCompletedSynchronously()
	{
		if (!_isWrite)
		{
			_pipeStream.UpdateMessageCompletion(_isMessageComplete);
		}
		TrySetResult(_numBytes);
	}

	protected override void AsyncCallback(uint errorCode, uint numBytes)
	{
		_numBytes = (int)numBytes;
		if (!_isWrite && (errorCode == 109 || errorCode - 232 <= 1))
		{
			errorCode = 0u;
		}
		if (errorCode == 234)
		{
			errorCode = 0u;
			_isMessageComplete = false;
		}
		else
		{
			_isMessageComplete = true;
		}
		base.AsyncCallback(errorCode, numBytes);
	}

	protected override void HandleError(int errorCode)
	{
		TrySetException(_pipeStream.WinIOError(errorCode));
	}
}
