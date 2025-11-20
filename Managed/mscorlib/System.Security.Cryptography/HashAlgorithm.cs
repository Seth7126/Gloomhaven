using System.Buffers;
using System.IO;

namespace System.Security.Cryptography;

/// <summary>Represents the base class from which all implementations of cryptographic hash algorithms must derive.</summary>
public abstract class HashAlgorithm : IDisposable, ICryptoTransform
{
	private bool _disposed;

	/// <summary>Represents the size, in bits, of the computed hash code.</summary>
	protected int HashSizeValue;

	/// <summary>Represents the value of the computed hash code.</summary>
	protected internal byte[] HashValue;

	/// <summary>Represents the state of the hash computation.</summary>
	protected int State;

	/// <summary>Gets the size, in bits, of the computed hash code.</summary>
	/// <returns>The size, in bits, of the computed hash code.</returns>
	public virtual int HashSize => HashSizeValue;

	/// <summary>Gets the value of the computed hash code.</summary>
	/// <returns>The current value of the computed hash code.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">
	///   <see cref="F:System.Security.Cryptography.HashAlgorithm.HashValue" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
	public virtual byte[] Hash
	{
		get
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(null);
			}
			if (State != 0)
			{
				throw new CryptographicUnexpectedOperationException("Hash must be finalized before the hash value is retrieved.");
			}
			return (byte[])HashValue?.Clone();
		}
	}

	/// <summary>When overridden in a derived class, gets the input block size.</summary>
	/// <returns>The input block size.</returns>
	public virtual int InputBlockSize => 1;

	/// <summary>When overridden in a derived class, gets the output block size.</summary>
	/// <returns>The output block size.</returns>
	public virtual int OutputBlockSize => 1;

	/// <summary>When overridden in a derived class, gets a value indicating whether multiple blocks can be transformed.</summary>
	/// <returns>true if multiple blocks can be transformed; otherwise, false.</returns>
	public virtual bool CanTransformMultipleBlocks => true;

	/// <summary>Gets a value indicating whether the current transform can be reused.</summary>
	/// <returns>Always true.</returns>
	public virtual bool CanReuseTransform => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> class.</summary>
	protected HashAlgorithm()
	{
	}

	/// <summary>Creates an instance of the default implementation of a hash algorithm.</summary>
	/// <returns>A new <see cref="T:System.Security.Cryptography.SHA1CryptoServiceProvider" /> instance, unless the default settings have been changed using the &lt;cryptoClass&gt; Element.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static HashAlgorithm Create()
	{
		return CryptoConfigForwarder.CreateDefaultHashAlgorithm();
	}

	/// <summary>Creates an instance of the specified implementation of a hash algorithm.</summary>
	/// <returns>A new instance of the specified hash algorithm, or null if <paramref name="hashName" /> is not a valid hash algorithm.</returns>
	/// <param name="hashName">The hash algorithm implementation to use. The following table shows the valid values for the <paramref name="hashName" /> parameter and the algorithms they map to. Parameter value Implements SHA <see cref="T:System.Security.Cryptography.SHA1CryptoServiceProvider" />SHA1 <see cref="T:System.Security.Cryptography.SHA1CryptoServiceProvider" />System.Security.Cryptography.SHA1 <see cref="T:System.Security.Cryptography.SHA1CryptoServiceProvider" />System.Security.Cryptography.HashAlgorithm <see cref="T:System.Security.Cryptography.SHA1CryptoServiceProvider" />MD5 <see cref="T:System.Security.Cryptography.MD5CryptoServiceProvider" />System.Security.Cryptography.MD5 <see cref="T:System.Security.Cryptography.MD5CryptoServiceProvider" />SHA256 <see cref="T:System.Security.Cryptography.SHA256Managed" />SHA-256 <see cref="T:System.Security.Cryptography.SHA256Managed" />System.Security.Cryptography.SHA256 <see cref="T:System.Security.Cryptography.SHA256Managed" />SHA384 <see cref="T:System.Security.Cryptography.SHA384Managed" />SHA-384 <see cref="T:System.Security.Cryptography.SHA384Managed" />System.Security.Cryptography.SHA384 <see cref="T:System.Security.Cryptography.SHA384Managed" />SHA512 <see cref="T:System.Security.Cryptography.SHA512Managed" />SHA-512 <see cref="T:System.Security.Cryptography.SHA512Managed" />System.Security.Cryptography.SHA512 <see cref="T:System.Security.Cryptography.SHA512Managed" /></param>
	public static HashAlgorithm Create(string hashName)
	{
		return (HashAlgorithm)CryptoConfigForwarder.CreateFromName(hashName);
	}

	/// <summary>Computes the hash value for the specified byte array.</summary>
	/// <returns>The computed hash code.</returns>
	/// <param name="buffer">The input to compute the hash code for. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
	public byte[] ComputeHash(byte[] buffer)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null);
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		HashCore(buffer, 0, buffer.Length);
		return CaptureHashCodeAndReinitialize();
	}

	public bool TryComputeHash(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null);
		}
		if (destination.Length < HashSizeValue / 8)
		{
			bytesWritten = 0;
			return false;
		}
		HashCore(source);
		if (!TryHashFinal(destination, out bytesWritten))
		{
			throw new InvalidOperationException("The algorithm's implementation is incorrect.");
		}
		HashValue = null;
		Initialize();
		return true;
	}

	/// <summary>Computes the hash value for the specified region of the specified byte array.</summary>
	/// <returns>The computed hash code.</returns>
	/// <param name="buffer">The input to compute the hash code for. </param>
	/// <param name="offset">The offset into the byte array from which to begin using data. </param>
	/// <param name="count">The number of bytes in the array to use as data. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="count" /> is an invalid value.-or-<paramref name="buffer" /> length is invalid.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> is out of range. This parameter requires a non-negative number.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
	public byte[] ComputeHash(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
		}
		if (count < 0 || count > buffer.Length)
		{
			throw new ArgumentException("Argument {0} should be larger than {1}.");
		}
		if (buffer.Length - count < offset)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (_disposed)
		{
			throw new ObjectDisposedException(null);
		}
		HashCore(buffer, offset, count);
		return CaptureHashCodeAndReinitialize();
	}

	/// <summary>Computes the hash value for the specified <see cref="T:System.IO.Stream" /> object.</summary>
	/// <returns>The computed hash code.</returns>
	/// <param name="inputStream">The input to compute the hash code for. </param>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
	public byte[] ComputeHash(Stream inputStream)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null);
		}
		byte[] array = ArrayPool<byte>.Shared.Rent(4096);
		try
		{
			int cbSize;
			while ((cbSize = inputStream.Read(array, 0, array.Length)) > 0)
			{
				HashCore(array, 0, cbSize);
			}
			return CaptureHashCodeAndReinitialize();
		}
		finally
		{
			CryptographicOperations.ZeroMemory(array);
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	private byte[] CaptureHashCodeAndReinitialize()
	{
		HashValue = HashFinal();
		byte[] result = (byte[])HashValue.Clone();
		Initialize();
		return result;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> class.</summary>
	public void Clear()
	{
		((IDisposable)this).Dispose();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposed = true;
		}
	}

	/// <summary>Computes the hash value for the specified region of the input byte array and copies the specified region of the input byte array to the specified region of the output byte array.</summary>
	/// <returns>The number of bytes written.</returns>
	/// <param name="inputBuffer">The input to compute the hash code for. </param>
	/// <param name="inputOffset">The offset into the input byte array from which to begin using data. </param>
	/// <param name="inputCount">The number of bytes in the input byte array to use as data. </param>
	/// <param name="outputBuffer">A copy of the part of the input array used to compute the hash code. </param>
	/// <param name="outputOffset">The offset into the output byte array from which to begin writing data. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="inputCount" /> uses an invalid value.-or-<paramref name="inputBuffer" /> has an invalid length.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inputBuffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="inputOffset" /> is out of range. This parameter requires a non-negative number.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
	public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
	{
		ValidateTransformBlock(inputBuffer, inputOffset, inputCount);
		State = 1;
		HashCore(inputBuffer, inputOffset, inputCount);
		if (outputBuffer != null && (inputBuffer != outputBuffer || inputOffset != outputOffset))
		{
			Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
		}
		return inputCount;
	}

	/// <summary>Computes the hash value for the specified region of the specified byte array.</summary>
	/// <returns>An array that is a copy of the part of the input that is hashed.</returns>
	/// <param name="inputBuffer">The input to compute the hash code for. </param>
	/// <param name="inputOffset">The offset into the byte array from which to begin using data. </param>
	/// <param name="inputCount">The number of bytes in the byte array to use as data. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="inputCount" /> uses an invalid value.-or-<paramref name="inputBuffer" /> has an invalid offset length.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inputBuffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="inputOffset" /> is out of range. This parameter requires a non-negative number.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
	public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
	{
		ValidateTransformBlock(inputBuffer, inputOffset, inputCount);
		HashCore(inputBuffer, inputOffset, inputCount);
		HashValue = CaptureHashCodeAndReinitialize();
		byte[] array;
		if (inputCount != 0)
		{
			array = new byte[inputCount];
			Buffer.BlockCopy(inputBuffer, inputOffset, array, 0, inputCount);
		}
		else
		{
			array = Array.Empty<byte>();
		}
		State = 0;
		return array;
	}

	private void ValidateTransformBlock(byte[] inputBuffer, int inputOffset, int inputCount)
	{
		if (inputBuffer == null)
		{
			throw new ArgumentNullException("inputBuffer");
		}
		if (inputOffset < 0)
		{
			throw new ArgumentOutOfRangeException("inputOffset", "Non-negative number required.");
		}
		if (inputCount < 0 || inputCount > inputBuffer.Length)
		{
			throw new ArgumentException("Argument {0} should be larger than {1}.");
		}
		if (inputBuffer.Length - inputCount < inputOffset)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (_disposed)
		{
			throw new ObjectDisposedException(null);
		}
	}

	/// <summary>When overridden in a derived class, routes data written to the object into the hash algorithm for computing the hash.</summary>
	/// <param name="array">The input to compute the hash code for. </param>
	/// <param name="ibStart">The offset into the byte array from which to begin using data. </param>
	/// <param name="cbSize">The number of bytes in the byte array to use as data. </param>
	protected abstract void HashCore(byte[] array, int ibStart, int cbSize);

	/// <summary>When overridden in a derived class, finalizes the hash computation after the last data is processed by the cryptographic stream object.</summary>
	/// <returns>The computed hash code.</returns>
	protected abstract byte[] HashFinal();

	/// <summary>Initializes an implementation of the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> class.</summary>
	public abstract void Initialize();

	protected virtual void HashCore(ReadOnlySpan<byte> source)
	{
		byte[] array = ArrayPool<byte>.Shared.Rent(source.Length);
		try
		{
			source.CopyTo(array);
			HashCore(array, 0, source.Length);
		}
		finally
		{
			Array.Clear(array, 0, source.Length);
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	protected virtual bool TryHashFinal(Span<byte> destination, out int bytesWritten)
	{
		int num = HashSizeValue / 8;
		if (destination.Length >= num)
		{
			byte[] array = HashFinal();
			if (array.Length == num)
			{
				new ReadOnlySpan<byte>(array).CopyTo(destination);
				bytesWritten = array.Length;
				return true;
			}
			throw new InvalidOperationException("The algorithm's implementation is incorrect.");
		}
		bytesWritten = 0;
		return false;
	}
}
