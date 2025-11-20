using System.Buffers;
using System.Text;
using Internal.Cryptography;

namespace System.Security.Cryptography;

/// <summary>Implements password-based key derivation functionality, PBKDF2, by using a pseudo-random number generator based on <see cref="T:System.Security.Cryptography.HMACSHA1" />.</summary>
public class Rfc2898DeriveBytes : DeriveBytes
{
	private const int MinimumSaltSize = 8;

	private readonly byte[] _password;

	private byte[] _salt;

	private uint _iterations;

	private HMAC _hmac;

	private int _blockSize;

	private byte[] _buffer;

	private uint _block;

	private int _startIndex;

	private int _endIndex;

	public HashAlgorithmName HashAlgorithm { get; }

	/// <summary>Gets or sets the number of iterations for the operation.</summary>
	/// <returns>The number of iterations for the operation.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of iterations is less than 1. </exception>
	public int IterationCount
	{
		get
		{
			return (int)_iterations;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("value", "Positive number required.");
			}
			_iterations = (uint)value;
			Initialize();
		}
	}

	/// <summary>Gets or sets the key salt value for the operation.</summary>
	/// <returns>The key salt value for the operation.</returns>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes. </exception>
	/// <exception cref="T:System.ArgumentNullException">The salt is null. </exception>
	public byte[] Salt
	{
		get
		{
			return _salt.CloneByteArray();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length < 8)
			{
				throw new ArgumentException("Salt is not at least eight bytes.");
			}
			_salt = value.CloneByteArray();
			Initialize();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using a password, a salt, and number of iterations to derive the key.</summary>
	/// <param name="password">The password used to derive the key. </param>
	/// <param name="salt">The key salt used to derive the key.</param>
	/// <param name="iterations">The number of iterations for the operation. </param>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes or the iteration count is less than 1. </exception>
	/// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
	public Rfc2898DeriveBytes(byte[] password, byte[] salt, int iterations)
		: this(password, salt, iterations, HashAlgorithmName.SHA1)
	{
	}

	public Rfc2898DeriveBytes(byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
	{
		if (salt == null)
		{
			throw new ArgumentNullException("salt");
		}
		if (salt.Length < 8)
		{
			throw new ArgumentException("Salt is not at least eight bytes.", "salt");
		}
		if (iterations <= 0)
		{
			throw new ArgumentOutOfRangeException("iterations", "Positive number required.");
		}
		if (password == null)
		{
			throw new NullReferenceException();
		}
		_salt = salt.CloneByteArray();
		_iterations = (uint)iterations;
		_password = password.CloneByteArray();
		HashAlgorithm = hashAlgorithm;
		_hmac = OpenHmac();
		_blockSize = _hmac.HashSize >> 3;
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using a password and salt to derive the key.</summary>
	/// <param name="password">The password used to derive the key. </param>
	/// <param name="salt">The key salt used to derive the key. </param>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes or the iteration count is less than 1. </exception>
	/// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
	public Rfc2898DeriveBytes(string password, byte[] salt)
		: this(password, salt, 1000)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using a password, a salt, and number of iterations to derive the key.</summary>
	/// <param name="password">The password used to derive the key. </param>
	/// <param name="salt">The key salt used to derive the key. </param>
	/// <param name="iterations">The number of iterations for the operation. </param>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes or the iteration count is less than 1. </exception>
	/// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
	public Rfc2898DeriveBytes(string password, byte[] salt, int iterations)
		: this(password, salt, iterations, HashAlgorithmName.SHA1)
	{
	}

	public Rfc2898DeriveBytes(string password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
		: this(Encoding.UTF8.GetBytes(password), salt, iterations, hashAlgorithm)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using the password and salt size to derive the key.</summary>
	/// <param name="password">The password used to derive the key. </param>
	/// <param name="saltSize">The size of the random salt that you want the class to generate. </param>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes. </exception>
	/// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
	public Rfc2898DeriveBytes(string password, int saltSize)
		: this(password, saltSize, 1000)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using a password, a salt size, and number of iterations to derive the key.</summary>
	/// <param name="password">The password used to derive the key. </param>
	/// <param name="saltSize">The size of the random salt that you want the class to generate. </param>
	/// <param name="iterations">The number of iterations for the operation. </param>
	/// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes or the iteration count is less than 1. </exception>
	/// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="iterations " />is out of range. This parameter requires a non-negative number.</exception>
	public Rfc2898DeriveBytes(string password, int saltSize, int iterations)
		: this(password, saltSize, iterations, HashAlgorithmName.SHA1)
	{
	}

	public Rfc2898DeriveBytes(string password, int saltSize, int iterations, HashAlgorithmName hashAlgorithm)
	{
		if (saltSize < 0)
		{
			throw new ArgumentOutOfRangeException("saltSize", "Non-negative number required.");
		}
		if (saltSize < 8)
		{
			throw new ArgumentException("Salt is not at least eight bytes.", "saltSize");
		}
		if (iterations <= 0)
		{
			throw new ArgumentOutOfRangeException("iterations", "Positive number required.");
		}
		_salt = Helpers.GenerateRandom(saltSize);
		_iterations = (uint)iterations;
		_password = Encoding.UTF8.GetBytes(password);
		HashAlgorithm = hashAlgorithm;
		_hmac = OpenHmac();
		_blockSize = _hmac.HashSize >> 3;
		Initialize();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_hmac != null)
			{
				_hmac.Dispose();
				_hmac = null;
			}
			if (_buffer != null)
			{
				Array.Clear(_buffer, 0, _buffer.Length);
			}
			if (_password != null)
			{
				Array.Clear(_password, 0, _password.Length);
			}
			if (_salt != null)
			{
				Array.Clear(_salt, 0, _salt.Length);
			}
		}
		base.Dispose(disposing);
	}

	/// <summary>Returns the pseudo-random key for this object.</summary>
	/// <returns>A byte array filled with pseudo-random key bytes.</returns>
	/// <param name="cb">The number of pseudo-random key bytes to generate. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="cb " />is out of range. This parameter requires a non-negative number.</exception>
	public override byte[] GetBytes(int cb)
	{
		if (cb <= 0)
		{
			throw new ArgumentOutOfRangeException("cb", "Positive number required.");
		}
		byte[] array = new byte[cb];
		int i = 0;
		int num = _endIndex - _startIndex;
		if (num > 0)
		{
			if (cb < num)
			{
				Buffer.BlockCopy(_buffer, _startIndex, array, 0, cb);
				_startIndex += cb;
				return array;
			}
			Buffer.BlockCopy(_buffer, _startIndex, array, 0, num);
			_startIndex = (_endIndex = 0);
			i += num;
		}
		for (; i < cb; i += _blockSize)
		{
			byte[] src = Func();
			int num2 = cb - i;
			if (num2 > _blockSize)
			{
				Buffer.BlockCopy(src, 0, array, i, _blockSize);
				continue;
			}
			Buffer.BlockCopy(src, 0, array, i, num2);
			i += num2;
			Buffer.BlockCopy(src, num2, _buffer, _startIndex, _blockSize - num2);
			_endIndex += _blockSize - num2;
			return array;
		}
		return array;
	}

	public byte[] CryptDeriveKey(string algname, string alghashname, int keySize, byte[] rgbIV)
	{
		throw new PlatformNotSupportedException();
	}

	/// <summary>Resets the state of the operation.</summary>
	public override void Reset()
	{
		Initialize();
	}

	private HMAC OpenHmac()
	{
		HashAlgorithmName hashAlgorithm = HashAlgorithm;
		if (string.IsNullOrEmpty(hashAlgorithm.Name))
		{
			throw new CryptographicException("The hash algorithm name cannot be null or empty.");
		}
		if (hashAlgorithm == HashAlgorithmName.SHA1)
		{
			return new HMACSHA1(_password);
		}
		if (hashAlgorithm == HashAlgorithmName.SHA256)
		{
			return new HMACSHA256(_password);
		}
		if (hashAlgorithm == HashAlgorithmName.SHA384)
		{
			return new HMACSHA384(_password);
		}
		if (hashAlgorithm == HashAlgorithmName.SHA512)
		{
			return new HMACSHA512(_password);
		}
		throw new CryptographicException(SR.Format("'{0}' is not a known hash algorithm.", hashAlgorithm.Name));
	}

	private void Initialize()
	{
		if (_buffer != null)
		{
			Array.Clear(_buffer, 0, _buffer.Length);
		}
		_buffer = new byte[_blockSize];
		_block = 1u;
		_startIndex = (_endIndex = 0);
	}

	private byte[] Func()
	{
		byte[] array = new byte[_salt.Length + 4];
		Buffer.BlockCopy(_salt, 0, array, 0, _salt.Length);
		Helpers.WriteInt(_block, array, _salt.Length);
		byte[] array2 = ArrayPool<byte>.Shared.Rent(_blockSize);
		try
		{
			Span<byte> span = new Span<byte>(array2, 0, _blockSize);
			if (!_hmac.TryComputeHash(array, span, out var bytesWritten) || bytesWritten != _blockSize)
			{
				throw new CryptographicException();
			}
			byte[] array3 = new byte[_blockSize];
			span.CopyTo(array3);
			for (int i = 2; i <= _iterations; i++)
			{
				if (!_hmac.TryComputeHash(span, span, out bytesWritten) || bytesWritten != _blockSize)
				{
					throw new CryptographicException();
				}
				for (int j = 0; j < _blockSize; j++)
				{
					array3[j] ^= array2[j];
				}
			}
			_block++;
			return array3;
		}
		finally
		{
			Array.Clear(array2, 0, _blockSize);
			ArrayPool<byte>.Shared.Return(array2);
		}
	}
}
