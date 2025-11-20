using System.Runtime.InteropServices;

namespace System.Drawing.Imaging;

[StructLayout(LayoutKind.Sequential)]
internal sealed class PropertyItemInternal : IDisposable
{
	public int id;

	public int len;

	public short type;

	public IntPtr value = IntPtr.Zero;

	public byte[] Value
	{
		get
		{
			if (len == 0)
			{
				return null;
			}
			byte[] array = new byte[len];
			Marshal.Copy(value, array, 0, len);
			return array;
		}
	}

	internal PropertyItemInternal()
	{
	}

	~PropertyItemInternal()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	private void Dispose(bool disposing)
	{
		if (value != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(value);
			value = IntPtr.Zero;
		}
		if (disposing)
		{
			GC.SuppressFinalize(this);
		}
	}

	internal static PropertyItemInternal ConvertFromPropertyItem(PropertyItem propItem)
	{
		PropertyItemInternal propertyItemInternal = new PropertyItemInternal();
		propertyItemInternal.id = propItem.Id;
		propertyItemInternal.len = 0;
		propertyItemInternal.type = propItem.Type;
		byte[] array = propItem.Value;
		if (array != null)
		{
			int num = (propertyItemInternal.len = array.Length);
			propertyItemInternal.value = Marshal.AllocHGlobal(num);
			Marshal.Copy(array, 0, propertyItemInternal.value, num);
		}
		return propertyItemInternal;
	}

	internal static PropertyItem[] ConvertFromMemory(IntPtr propdata, int count)
	{
		PropertyItem[] array = new PropertyItem[count];
		for (int i = 0; i < count; i++)
		{
			PropertyItemInternal propertyItemInternal = null;
			try
			{
				propertyItemInternal = (PropertyItemInternal)Marshal.PtrToStructure(propdata, typeof(PropertyItemInternal));
				array[i] = new PropertyItem();
				array[i].Id = propertyItemInternal.id;
				array[i].Len = propertyItemInternal.len;
				array[i].Type = propertyItemInternal.type;
				array[i].Value = propertyItemInternal.Value;
				propertyItemInternal.value = IntPtr.Zero;
			}
			finally
			{
				propertyItemInternal?.Dispose();
			}
			propdata = (IntPtr)((long)propdata + Marshal.SizeOf(typeof(PropertyItemInternal)));
		}
		return array;
	}
}
