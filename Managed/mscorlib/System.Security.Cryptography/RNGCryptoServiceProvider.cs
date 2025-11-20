using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Cryptography;

/// <summary>Implements a cryptographic Random Number Generator (RNG) using the implementation provided by the cryptographic service provider (CSP). This class cannot be inherited.</summary>
[ComVisible(true)]
public sealed class RNGCryptoServiceProvider : RandomNumberGenerator
{
	private static object _lock;

	private IntPtr _handle;

	static RNGCryptoServiceProvider()
	{
		if (RngOpen())
		{
			_lock = new object();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RNGCryptoServiceProvider" /> class.</summary>
	public unsafe RNGCryptoServiceProvider()
	{
		_handle = RngInitialize(null, IntPtr.Zero);
		Check();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RNGCryptoServiceProvider" /> class.</summary>
	/// <param name="rgb">A byte array. This value is ignored.</param>
	public unsafe RNGCryptoServiceProvider(byte[] rgb)
	{
		fixed (byte* seed = rgb)
		{
			_handle = RngInitialize(seed, (rgb != null) ? ((IntPtr)rgb.Length) : IntPtr.Zero);
		}
		Check();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RNGCryptoServiceProvider" /> class with the specified parameters.</summary>
	/// <param name="cspParams">The parameters to pass to the cryptographic service provider (CSP). </param>
	public unsafe RNGCryptoServiceProvider(CspParameters cspParams)
	{
		_handle = RngInitialize(null, IntPtr.Zero);
		Check();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.RNGCryptoServiceProvider" /> class.</summary>
	/// <param name="str">The string input. This parameter is ignored.</param>
	public unsafe RNGCryptoServiceProvider(string str)
	{
		if (str == null)
		{
			_handle = RngInitialize(null, IntPtr.Zero);
		}
		else
		{
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			fixed (byte* seed = bytes)
			{
				_handle = RngInitialize(seed, (IntPtr)bytes.Length);
			}
		}
		Check();
	}

	private void Check()
	{
		if (_handle == IntPtr.Zero)
		{
			throw new CryptographicException(Locale.GetText("Couldn't access random source."));
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool RngOpen();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern IntPtr RngInitialize(byte* seed, IntPtr seed_length);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern IntPtr RngGetBytes(IntPtr handle, byte* data, IntPtr data_length);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void RngClose(IntPtr handle);

	/// <summary>Fills an array of bytes with a cryptographically strong sequence of random values.</summary>
	/// <param name="data">The array to fill with a cryptographically strong sequence of random values. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	public unsafe override void GetBytes(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		fixed (byte* data2 = data)
		{
			if (_lock == null)
			{
				_handle = RngGetBytes(_handle, data2, (IntPtr)data.LongLength);
			}
			else
			{
				lock (_lock)
				{
					_handle = RngGetBytes(_handle, data2, (IntPtr)data.LongLength);
				}
			}
		}
		Check();
	}

	internal unsafe void GetBytes(byte* data, IntPtr data_length)
	{
		if (_lock == null)
		{
			_handle = RngGetBytes(_handle, data, data_length);
		}
		else
		{
			lock (_lock)
			{
				_handle = RngGetBytes(_handle, data, data_length);
			}
		}
		Check();
	}

	/// <summary>Fills an array of bytes with a cryptographically strong sequence of random nonzero values.</summary>
	/// <param name="data">The array to fill with a cryptographically strong sequence of random nonzero values. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	public unsafe override void GetNonZeroBytes(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte[] array = new byte[data.LongLength * 2];
		long num = 0L;
		while (num < data.LongLength)
		{
			fixed (byte* data2 = array)
			{
				_handle = RngGetBytes(_handle, data2, (IntPtr)array.LongLength);
			}
			Check();
			for (long num2 = 0L; num2 < array.LongLength; num2++)
			{
				if (num == data.LongLength)
				{
					break;
				}
				if (array[num2] != 0)
				{
					data[num++] = array[num2];
				}
			}
		}
	}

	~RNGCryptoServiceProvider()
	{
		if (_handle != IntPtr.Zero)
		{
			RngClose(_handle);
			_handle = IntPtr.Zero;
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}
}
