namespace System.Runtime.InteropServices;

/// <summary>Wraps a managed object holding a handle to a resource that is passed to unmanaged code using platform invoke.</summary>
public readonly struct HandleRef
{
	private readonly object _wrapper;

	private readonly IntPtr _handle;

	/// <summary>Gets the object holding the handle to a resource.</summary>
	/// <returns>The object holding the handle to a resource.</returns>
	public object Wrapper => _wrapper;

	/// <summary>Gets the handle to a resource.</summary>
	/// <returns>The handle to a resource.</returns>
	public IntPtr Handle => _handle;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.HandleRef" /> class with the object to wrap and a handle to the resource used by unmanaged code.</summary>
	/// <param name="wrapper">A managed object that should not be finalized until the platform invoke call returns. </param>
	/// <param name="handle">An <see cref="T:System.IntPtr" /> that indicates a handle to a resource. </param>
	public HandleRef(object wrapper, IntPtr handle)
	{
		_wrapper = wrapper;
		_handle = handle;
	}

	/// <summary>Returns the handle to a resource of the specified <see cref="T:System.Runtime.InteropServices.HandleRef" /> object.</summary>
	/// <returns>The handle to a resource of the specified <see cref="T:System.Runtime.InteropServices.HandleRef" /> object.</returns>
	/// <param name="value">The object that needs a handle. </param>
	public static explicit operator IntPtr(HandleRef value)
	{
		return value._handle;
	}

	/// <summary>Returns the internal integer representation of a <see cref="T:System.Runtime.InteropServices.HandleRef" /> object.</summary>
	/// <returns>An <see cref="T:System.IntPtr" /> object that represents a <see cref="T:System.Runtime.InteropServices.HandleRef" /> object.</returns>
	/// <param name="value">A <see cref="T:System.Runtime.InteropServices.HandleRef" /> object to retrieve an internal integer representation from.</param>
	public static IntPtr ToIntPtr(HandleRef value)
	{
		return value._handle;
	}
}
