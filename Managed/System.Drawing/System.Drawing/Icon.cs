using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Drawing;

/// <summary>Represents a Windows icon, which is a small bitmap image that is used to represent an object. Icons can be thought of as transparent bitmaps, although their size is determined by the system.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[Editor("System.Drawing.Design.IconEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
[TypeConverter(typeof(IconConverter))]
public sealed class Icon : MarshalByRefObject, ISerializable, ICloneable, IDisposable
{
	internal struct IconDirEntry
	{
		internal byte width;

		internal byte height;

		internal byte colorCount;

		internal byte reserved;

		internal ushort planes;

		internal ushort bitCount;

		internal uint bytesInRes;

		internal uint imageOffset;

		internal bool ignore;
	}

	internal struct IconDir
	{
		internal ushort idReserved;

		internal ushort idType;

		internal ushort idCount;

		internal IconDirEntry[] idEntries;
	}

	internal struct BitmapInfoHeader
	{
		internal uint biSize;

		internal int biWidth;

		internal int biHeight;

		internal ushort biPlanes;

		internal ushort biBitCount;

		internal uint biCompression;

		internal uint biSizeImage;

		internal int biXPelsPerMeter;

		internal int biYPelsPerMeter;

		internal uint biClrUsed;

		internal uint biClrImportant;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal abstract class ImageData
	{
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class IconImage : ImageData
	{
		internal BitmapInfoHeader iconHeader;

		internal uint[] iconColors;

		internal byte[] iconXOR;

		internal byte[] iconAND;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class IconDump : ImageData
	{
		internal byte[] data;
	}

	private Size iconSize;

	private IntPtr handle = IntPtr.Zero;

	private IconDir iconDir;

	private ushort id;

	private ImageData[] imageData;

	private bool undisposable;

	private bool disposed;

	private Bitmap bitmap;

	/// <summary>Gets the Windows handle for this <see cref="T:System.Drawing.Icon" />. This is not a copy of the handle; do not free it.</summary>
	/// <returns>The Windows handle for the icon.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public IntPtr Handle
	{
		get
		{
			if (!disposed && handle == IntPtr.Zero)
			{
				if (GDIPlus.RunningOnUnix())
				{
					handle = GetInternalBitmap().NativeObject;
				}
				else
				{
					IconInfo piconinfo = default(IconInfo);
					piconinfo.IsIcon = true;
					piconinfo.hbmColor = ToBitmap().GetHbitmap();
					piconinfo.hbmMask = piconinfo.hbmColor;
					handle = GDIPlus.CreateIconIndirect(ref piconinfo);
				}
			}
			return handle;
		}
	}

	/// <summary>Gets the height of this <see cref="T:System.Drawing.Icon" />.</summary>
	/// <returns>The height of this <see cref="T:System.Drawing.Icon" />.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public int Height => iconSize.Height;

	/// <summary>Gets the size of this <see cref="T:System.Drawing.Icon" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Size" /> structure that specifies the width and height of this <see cref="T:System.Drawing.Icon" />.</returns>
	/// <filterpriority>1</filterpriority>
	public Size Size => iconSize;

	/// <summary>Gets the width of this <see cref="T:System.Drawing.Icon" />.</summary>
	/// <returns>The width of this <see cref="T:System.Drawing.Icon" />.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public int Width => iconSize.Width;

	private Icon()
	{
	}

	private Icon(IntPtr handle)
	{
		this.handle = handle;
		bitmap = Bitmap.FromHicon(handle);
		iconSize = new Size(bitmap.Width, bitmap.Height);
		if (GDIPlus.RunningOnUnix())
		{
			bitmap = Bitmap.FromHicon(handle);
			iconSize = new Size(bitmap.Width, bitmap.Height);
		}
		else
		{
			GDIPlus.GetIconInfo(handle, out var iconinfo);
			if (!iconinfo.IsIcon)
			{
				throw new NotImplementedException(global::Locale.GetText("Handle doesn't represent an ICON."));
			}
			iconSize = new Size(iconinfo.xHotspot * 2, iconinfo.yHotspot * 2);
			bitmap = Image.FromHbitmap(iconinfo.hbmColor);
		}
		undisposable = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Icon" /> class and attempts to find a version of the icon that matches the requested size.</summary>
	/// <param name="original">The icon to load the different size from. </param>
	/// <param name="width">The width of the new icon. </param>
	/// <param name="height">The height of the new icon. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="original" /> parameter is null.</exception>
	public Icon(Icon original, int width, int height)
		: this(original, new Size(width, height))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Icon" /> class and attempts to find a version of the icon that matches the requested size.</summary>
	/// <param name="original">The <see cref="T:System.Drawing.Icon" /> from which to load the newly sized icon. </param>
	/// <param name="size">A <see cref="T:System.Drawing.Size" /> structure that specifies the height and width of the new <see cref="T:System.Drawing.Icon" />. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="original" /> parameter is null.</exception>
	public Icon(Icon original, Size size)
	{
		if (original == null)
		{
			throw new ArgumentException("original");
		}
		iconSize = size;
		iconDir = original.iconDir;
		int idCount = iconDir.idCount;
		if (idCount > 0)
		{
			imageData = original.imageData;
			id = ushort.MaxValue;
			for (ushort num = 0; num < idCount; num++)
			{
				IconDirEntry iconDirEntry = iconDir.idEntries[num];
				if ((iconDirEntry.height == size.Height || iconDirEntry.width == size.Width) && !iconDirEntry.ignore)
				{
					id = num;
					break;
				}
			}
			if (id == ushort.MaxValue)
			{
				int num2 = Math.Min(size.Height, size.Width);
				IconDirEntry? iconDirEntry2 = null;
				for (ushort num3 = 0; num3 < idCount; num3++)
				{
					IconDirEntry value = iconDir.idEntries[num3];
					if ((value.height < num2 || value.width < num2) && !value.ignore)
					{
						if (!iconDirEntry2.HasValue)
						{
							iconDirEntry2 = value;
							id = num3;
						}
						else if (value.height > iconDirEntry2.Value.height || value.width > iconDirEntry2.Value.width)
						{
							iconDirEntry2 = value;
							id = num3;
						}
					}
				}
			}
			if (id == ushort.MaxValue)
			{
				int num4 = idCount;
				while (id == ushort.MaxValue && num4 > 0)
				{
					num4--;
					if (!iconDir.idEntries[num4].ignore)
					{
						id = (ushort)num4;
					}
				}
			}
			if (id == ushort.MaxValue)
			{
				throw new ArgumentException("Icon", "No valid icon image found");
			}
			iconSize.Height = iconDir.idEntries[id].height;
			iconSize.Width = iconDir.idEntries[id].width;
		}
		else
		{
			iconSize.Height = size.Height;
			iconSize.Width = size.Width;
		}
		if (original.bitmap != null)
		{
			bitmap = (Bitmap)original.bitmap.Clone();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Icon" /> class from the specified data stream.</summary>
	/// <param name="stream">The data stream from which to load the <see cref="T:System.Drawing.Icon" />. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="stream" /> parameter is null.</exception>
	public Icon(Stream stream)
		: this(stream, 32, 32)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Icon" /> class from the specified data stream and with the specified width and height.</summary>
	/// <param name="stream">The data stream from which to load the icon. </param>
	/// <param name="width">The width, in pixels, of the icon. </param>
	/// <param name="height">The height, in pixels, of the icon. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="stream" /> parameter is null.</exception>
	public Icon(Stream stream, int width, int height)
	{
		InitFromStreamWithSize(stream, width, height);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Icon" /> class from the specified file name.</summary>
	/// <param name="fileName">The file to load the <see cref="T:System.Drawing.Icon" /> from. </param>
	public Icon(string fileName)
	{
		using FileStream stream = File.OpenRead(fileName);
		InitFromStreamWithSize(stream, 32, 32);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Icon" /> class from a resource in the specified assembly.</summary>
	/// <param name="type">A <see cref="T:System.Type" /> that specifies the assembly in which to look for the resource. </param>
	/// <param name="resource">The resource name to load. </param>
	/// <exception cref="T:System.ArgumentException">An icon specified by <paramref name="resource" /> cannot be found in the assembly that contains the specified <paramref name="type" />.</exception>
	public Icon(Type type, string resource)
	{
		if (resource == null)
		{
			throw new ArgumentException("resource");
		}
		if (type == null)
		{
			throw new NullReferenceException();
		}
		using Stream stream = type.GetTypeInfo().Assembly.GetManifestResourceStream(type, resource);
		if (stream == null)
		{
			throw new FileNotFoundException(global::Locale.GetText("Resource '{0}' was not found.", resource));
		}
		InitFromStreamWithSize(stream, 32, 32);
	}

	private Icon(SerializationInfo info, StreamingContext context)
	{
		MemoryStream memoryStream = null;
		int width = 0;
		int height = 0;
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			if (string.Compare(current.Name, "IconData", ignoreCase: true) == 0)
			{
				memoryStream = new MemoryStream((byte[])current.Value);
			}
			if (string.Compare(current.Name, "IconSize", ignoreCase: true) == 0)
			{
				Size size = (Size)current.Value;
				width = size.Width;
				height = size.Height;
			}
		}
		if (memoryStream != null)
		{
			memoryStream.Seek(0L, SeekOrigin.Begin);
			InitFromStreamWithSize(memoryStream, width, height);
		}
	}

	internal Icon(string resourceName, bool undisposable)
	{
		using (Stream stream = typeof(Icon).GetTypeInfo().Assembly.GetManifestResourceStream(resourceName))
		{
			if (stream == null)
			{
				throw new FileNotFoundException(global::Locale.GetText("Resource '{0}' was not found.", resourceName));
			}
			InitFromStreamWithSize(stream, 32, 32);
		}
		this.undisposable = true;
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data that is required to serialize the target object.</summary>
	/// <param name="si">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
	/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
	{
		MemoryStream memoryStream = new MemoryStream();
		Save(memoryStream);
		si.AddValue("IconSize", Size, typeof(Size));
		si.AddValue("IconData", memoryStream.ToArray());
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Icon" /> class of the specified size from the specified stream.</summary>
	/// <param name="stream">The stream that contains the icon data.</param>
	/// <param name="size">The desired size of the icon.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="stream" /> is null or does not contain image data.</exception>
	public Icon(Stream stream, Size size)
		: this(stream, size.Width, size.Height)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Icon" /> class with the specified width and height from the specified file.</summary>
	/// <param name="fileName">The name and path to the file that contains the <see cref="T:System.Drawing.Icon" /> data.</param>
	/// <param name="width">The desired width of the <see cref="T:System.Drawing.Icon" />.</param>
	/// <param name="height">The desired height of the <see cref="T:System.Drawing.Icon" />.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="string" /> is null or does not contain image data.</exception>
	public Icon(string fileName, int width, int height)
	{
		using FileStream stream = File.OpenRead(fileName);
		InitFromStreamWithSize(stream, width, height);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Icon" /> class of the specified size from the specified file.</summary>
	/// <param name="fileName">The name and path to the file that contains the icon data.</param>
	/// <param name="size">The desired size of the icon.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="string" /> is null or does not contain image data.</exception>
	public Icon(string fileName, Size size)
	{
		using FileStream stream = File.OpenRead(fileName);
		InitFromStreamWithSize(stream, size.Width, size.Height);
	}

	/// <summary>Returns an icon representation of an image that is contained in the specified file.</summary>
	/// <returns>The <see cref="T:System.Drawing.Icon" /> representation of the image that is contained in the specified file.</returns>
	/// <param name="filePath">The path to the file that contains an image.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="filePath" /> does not indicate a valid file.-or-The <paramref name="filePath" /> indicates a Universal Naming Convention (UNC) path.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoLimitation("The same icon, SystemIcons.WinLogo, is returned for all file types.")]
	public static Icon ExtractAssociatedIcon(string filePath)
	{
		if (string.IsNullOrEmpty(filePath))
		{
			throw new ArgumentException(global::Locale.GetText("Null or empty path."), "filePath");
		}
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException(global::Locale.GetText("Couldn't find specified file."), filePath);
		}
		return SystemIcons.WinLogo;
	}

	/// <summary>Releases all resources used by this <see cref="T:System.Drawing.Icon" />.</summary>
	/// <filterpriority>1</filterpriority>
	public void Dispose()
	{
		if (undisposable)
		{
			return;
		}
		if (!disposed)
		{
			if (GDIPlus.RunningOnWindows() && handle != IntPtr.Zero)
			{
				GDIPlus.DestroyIcon(handle);
				handle = IntPtr.Zero;
			}
			if (bitmap != null)
			{
				bitmap.Dispose();
				bitmap = null;
			}
			GC.SuppressFinalize(this);
		}
		disposed = true;
	}

	/// <summary>Clones the <see cref="T:System.Drawing.Icon" />, creating a duplicate image.</summary>
	/// <returns>An object that can be cast to an <see cref="T:System.Drawing.Icon" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public object Clone()
	{
		return new Icon(this, Size);
	}

	/// <summary>Creates a GDI+ <see cref="T:System.Drawing.Icon" /> from the specified Windows handle to an icon (HICON).</summary>
	/// <returns>The <see cref="T:System.Drawing.Icon" /> this method creates.</returns>
	/// <param name="handle">A Windows handle to an icon. </param>
	/// <filterpriority>1</filterpriority>
	public static Icon FromHandle(IntPtr handle)
	{
		if (handle == IntPtr.Zero)
		{
			throw new ArgumentException("handle");
		}
		return new Icon(handle);
	}

	private void SaveIconImage(BinaryWriter writer, IconImage ii)
	{
		BitmapInfoHeader iconHeader = ii.iconHeader;
		writer.Write(iconHeader.biSize);
		writer.Write(iconHeader.biWidth);
		writer.Write(iconHeader.biHeight);
		writer.Write(iconHeader.biPlanes);
		writer.Write(iconHeader.biBitCount);
		writer.Write(iconHeader.biCompression);
		writer.Write(iconHeader.biSizeImage);
		writer.Write(iconHeader.biXPelsPerMeter);
		writer.Write(iconHeader.biYPelsPerMeter);
		writer.Write(iconHeader.biClrUsed);
		writer.Write(iconHeader.biClrImportant);
		int num = ii.iconColors.Length;
		for (int i = 0; i < num; i++)
		{
			writer.Write(ii.iconColors[i]);
		}
		writer.Write(ii.iconXOR);
		writer.Write(ii.iconAND);
	}

	private void SaveIconDump(BinaryWriter writer, IconDump id)
	{
		writer.Write(id.data);
	}

	private void SaveIconDirEntry(BinaryWriter writer, IconDirEntry ide, uint offset)
	{
		writer.Write(ide.width);
		writer.Write(ide.height);
		writer.Write(ide.colorCount);
		writer.Write(ide.reserved);
		writer.Write(ide.planes);
		writer.Write(ide.bitCount);
		writer.Write(ide.bytesInRes);
		writer.Write((offset == uint.MaxValue) ? ide.imageOffset : offset);
	}

	private void SaveAll(BinaryWriter writer)
	{
		writer.Write(iconDir.idReserved);
		writer.Write(iconDir.idType);
		ushort idCount = iconDir.idCount;
		writer.Write(idCount);
		for (int i = 0; i < idCount; i++)
		{
			SaveIconDirEntry(writer, iconDir.idEntries[i], uint.MaxValue);
		}
		for (int j = 0; j < idCount; j++)
		{
			while (writer.BaseStream.Length < iconDir.idEntries[j].imageOffset)
			{
				writer.Write((byte)0);
			}
			if (imageData[j] is IconDump)
			{
				SaveIconDump(writer, (IconDump)imageData[j]);
			}
			else
			{
				SaveIconImage(writer, (IconImage)imageData[j]);
			}
		}
	}

	private void SaveBestSingleIcon(BinaryWriter writer, int width, int height)
	{
		writer.Write(iconDir.idReserved);
		writer.Write(iconDir.idType);
		writer.Write((ushort)1);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < iconDir.idCount; i++)
		{
			IconDirEntry iconDirEntry = iconDir.idEntries[i];
			if (width == iconDirEntry.width && height == iconDirEntry.height && iconDirEntry.bitCount >= num2)
			{
				num2 = iconDirEntry.bitCount;
				num = i;
			}
		}
		SaveIconDirEntry(writer, iconDir.idEntries[num], 22u);
		SaveIconImage(writer, (IconImage)imageData[num]);
	}

	private void SaveBitmapAsIcon(BinaryWriter writer)
	{
		writer.Write((ushort)0);
		writer.Write((ushort)1);
		writer.Write((ushort)1);
		IconDirEntry ide = new IconDirEntry
		{
			width = (byte)bitmap.Width,
			height = (byte)bitmap.Height,
			colorCount = 0,
			reserved = 0,
			planes = 0,
			bitCount = 32,
			imageOffset = 22u
		};
		BitmapInfoHeader iconHeader = new BitmapInfoHeader
		{
			biSize = (uint)Marshal.SizeOf(typeof(BitmapInfoHeader)),
			biWidth = bitmap.Width,
			biHeight = 2 * bitmap.Height,
			biPlanes = 1,
			biBitCount = 32,
			biCompression = 0u,
			biSizeImage = 0u,
			biXPelsPerMeter = 0,
			biYPelsPerMeter = 0,
			biClrUsed = 0u,
			biClrImportant = 0u
		};
		IconImage iconImage = new IconImage();
		iconImage.iconHeader = iconHeader;
		iconImage.iconColors = new uint[0];
		int num = (((iconHeader.biBitCount * bitmap.Width + 31) & -32) >> 3) * bitmap.Height;
		iconImage.iconXOR = new byte[num];
		int num2 = 0;
		for (int num3 = bitmap.Height - 1; num3 >= 0; num3--)
		{
			for (int i = 0; i < bitmap.Width; i++)
			{
				Color pixel = bitmap.GetPixel(i, num3);
				iconImage.iconXOR[num2++] = pixel.B;
				iconImage.iconXOR[num2++] = pixel.G;
				iconImage.iconXOR[num2++] = pixel.R;
				iconImage.iconXOR[num2++] = pixel.A;
			}
		}
		int num4 = (((Width + 31) & -32) >> 3) * bitmap.Height;
		iconImage.iconAND = new byte[num4];
		ide.bytesInRes = (uint)(iconHeader.biSize + num + num4);
		SaveIconDirEntry(writer, ide, uint.MaxValue);
		SaveIconImage(writer, iconImage);
	}

	private void Save(Stream outputStream, int width, int height)
	{
		BinaryWriter binaryWriter = new BinaryWriter(outputStream);
		if (iconDir.idEntries != null)
		{
			if (width == -1 && height == -1)
			{
				SaveAll(binaryWriter);
			}
			else
			{
				SaveBestSingleIcon(binaryWriter, width, height);
			}
		}
		else if (bitmap != null)
		{
			SaveBitmapAsIcon(binaryWriter);
		}
		binaryWriter.Flush();
	}

	/// <summary>Saves this <see cref="T:System.Drawing.Icon" /> to the specified output <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="outputStream">The <see cref="T:System.IO.Stream" /> to save to. </param>
	/// <filterpriority>1</filterpriority>
	public void Save(Stream outputStream)
	{
		if (outputStream == null)
		{
			throw new NullReferenceException("outputStream");
		}
		Save(outputStream, -1, -1);
	}

	internal Bitmap BuildBitmapOnWin32()
	{
		if (imageData == null)
		{
			return new Bitmap(32, 32);
		}
		IconImage iconImage = (IconImage)imageData[id];
		BitmapInfoHeader iconHeader = iconImage.iconHeader;
		int num = iconHeader.biHeight / 2;
		if (iconHeader.biClrUsed == 0)
		{
			_ = iconHeader.biBitCount;
			_ = 24;
		}
		Bitmap bitmap = iconHeader.biBitCount switch
		{
			1 => new Bitmap(iconHeader.biWidth, num, PixelFormat.Format1bppIndexed), 
			4 => new Bitmap(iconHeader.biWidth, num, PixelFormat.Format4bppIndexed), 
			8 => new Bitmap(iconHeader.biWidth, num, PixelFormat.Format8bppIndexed), 
			24 => new Bitmap(iconHeader.biWidth, num, PixelFormat.Format24bppRgb), 
			32 => new Bitmap(iconHeader.biWidth, num, PixelFormat.Format32bppArgb), 
			_ => throw new Exception(global::Locale.GetText("Unexpected number of bits: {0}", iconHeader.biBitCount)), 
		};
		if (iconHeader.biBitCount < 24)
		{
			ColorPalette palette = bitmap.Palette;
			for (int i = 0; i < iconImage.iconColors.Length; i++)
			{
				palette.Entries[i] = Color.FromArgb((int)iconImage.iconColors[i] | -16777216);
			}
			bitmap.Palette = palette;
		}
		int num2 = ((iconHeader.biWidth * iconHeader.biBitCount + 31) & -32) >> 3;
		BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
		for (int j = 0; j < num; j++)
		{
			Marshal.Copy(iconImage.iconXOR, num2 * j, (IntPtr)(bitmapData.Scan0.ToInt64() + bitmapData.Stride * (num - 1 - j)), num2);
		}
		bitmap.UnlockBits(bitmapData);
		bitmap = new Bitmap(bitmap);
		num2 = ((iconHeader.biWidth + 31) & -32) >> 3;
		for (int k = 0; k < num; k++)
		{
			for (int l = 0; l < iconHeader.biWidth / 8; l++)
			{
				for (int num3 = 7; num3 >= 0; num3--)
				{
					if (((iconImage.iconAND[k * num2 + l] >> num3) & 1) != 0)
					{
						bitmap.SetPixel(l * 8 + 7 - num3, num - k - 1, Color.Transparent);
					}
				}
			}
		}
		return bitmap;
	}

	internal Bitmap GetInternalBitmap()
	{
		if (bitmap == null)
		{
			if (GDIPlus.RunningOnUnix())
			{
				using MemoryStream memoryStream = new MemoryStream();
				Save(memoryStream, Width, Height);
				memoryStream.Position = 0L;
				bitmap = (Bitmap)Image.LoadFromStream(memoryStream, keepAlive: false);
			}
			else
			{
				bitmap = BuildBitmapOnWin32();
			}
		}
		return bitmap;
	}

	/// <summary>Converts this <see cref="T:System.Drawing.Icon" /> to a GDI+ <see cref="T:System.Drawing.Bitmap" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Bitmap" /> that represents the converted <see cref="T:System.Drawing.Icon" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Bitmap ToBitmap()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(global::Locale.GetText("Icon instance was disposed."));
		}
		return new Bitmap(GetInternalBitmap());
	}

	/// <summary>Gets a human-readable string that describes the <see cref="T:System.Drawing.Icon" />.</summary>
	/// <returns>A string that describes the <see cref="T:System.Drawing.Icon" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override string ToString()
	{
		return "<Icon>";
	}

	~Icon()
	{
		Dispose();
	}

	private void InitFromStreamWithSize(Stream stream, int width, int height)
	{
		if (stream == null || stream.Length == 0L)
		{
			throw new ArgumentException("The argument 'stream' must be a picture that can be used as a Icon", "stream");
		}
		BinaryReader binaryReader = new BinaryReader(stream);
		iconDir.idReserved = binaryReader.ReadUInt16();
		if (iconDir.idReserved != 0)
		{
			throw new ArgumentException("Invalid Argument", "stream");
		}
		iconDir.idType = binaryReader.ReadUInt16();
		if (iconDir.idType != 1)
		{
			throw new ArgumentException("Invalid Argument", "stream");
		}
		ushort num = binaryReader.ReadUInt16();
		imageData = new ImageData[num];
		iconDir.idCount = num;
		iconDir.idEntries = new IconDirEntry[num];
		bool flag = false;
		IconDirEntry iconDirEntry = default(IconDirEntry);
		for (int i = 0; i < num; i++)
		{
			iconDirEntry.width = binaryReader.ReadByte();
			iconDirEntry.height = binaryReader.ReadByte();
			iconDirEntry.colorCount = binaryReader.ReadByte();
			iconDirEntry.reserved = binaryReader.ReadByte();
			iconDirEntry.planes = binaryReader.ReadUInt16();
			iconDirEntry.bitCount = binaryReader.ReadUInt16();
			iconDirEntry.bytesInRes = binaryReader.ReadUInt32();
			iconDirEntry.imageOffset = binaryReader.ReadUInt32();
			if (iconDirEntry.width == 0 && iconDirEntry.height == 0)
			{
				iconDirEntry.ignore = true;
			}
			else
			{
				iconDirEntry.ignore = false;
			}
			iconDir.idEntries[i] = iconDirEntry;
			if (!flag && (iconDirEntry.height == height || iconDirEntry.width == width) && !iconDirEntry.ignore)
			{
				id = (ushort)i;
				flag = true;
				iconSize.Height = iconDirEntry.height;
				iconSize.Width = iconDirEntry.width;
			}
		}
		int num2 = 0;
		for (int j = 0; j < num; j++)
		{
			if (!iconDir.idEntries[j].ignore)
			{
				num2++;
			}
		}
		if (num2 == 0)
		{
			throw new Win32Exception(0, "No valid icon entry were found.");
		}
		if (!flag)
		{
			uint num3 = 0u;
			for (int k = 0; k < num; k++)
			{
				if (iconDir.idEntries[k].bytesInRes >= num3 && !iconDir.idEntries[k].ignore)
				{
					num3 = iconDir.idEntries[k].bytesInRes;
					id = (ushort)k;
					iconSize.Height = iconDir.idEntries[k].height;
					iconSize.Width = iconDir.idEntries[k].width;
				}
			}
		}
		for (int l = 0; l < num; l++)
		{
			if (iconDir.idEntries[l].ignore)
			{
				IconDump iconDump = new IconDump();
				stream.Seek(iconDir.idEntries[l].imageOffset, SeekOrigin.Begin);
				iconDump.data = new byte[iconDir.idEntries[l].bytesInRes];
				stream.Read(iconDump.data, 0, iconDump.data.Length);
				imageData[l] = iconDump;
				continue;
			}
			IconImage iconImage = new IconImage();
			BitmapInfoHeader iconHeader = default(BitmapInfoHeader);
			stream.Seek(iconDir.idEntries[l].imageOffset, SeekOrigin.Begin);
			byte[] array = new byte[iconDir.idEntries[l].bytesInRes];
			stream.Read(array, 0, array.Length);
			BinaryReader binaryReader2 = new BinaryReader(new MemoryStream(array));
			iconHeader.biSize = binaryReader2.ReadUInt32();
			iconHeader.biWidth = binaryReader2.ReadInt32();
			iconHeader.biHeight = binaryReader2.ReadInt32();
			iconHeader.biPlanes = binaryReader2.ReadUInt16();
			iconHeader.biBitCount = binaryReader2.ReadUInt16();
			iconHeader.biCompression = binaryReader2.ReadUInt32();
			iconHeader.biSizeImage = binaryReader2.ReadUInt32();
			iconHeader.biXPelsPerMeter = binaryReader2.ReadInt32();
			iconHeader.biYPelsPerMeter = binaryReader2.ReadInt32();
			iconHeader.biClrUsed = binaryReader2.ReadUInt32();
			iconHeader.biClrImportant = binaryReader2.ReadUInt32();
			iconImage.iconHeader = iconHeader;
			int num4 = iconHeader.biBitCount switch
			{
				1 => 2, 
				4 => 16, 
				8 => 256, 
				_ => 0, 
			};
			iconImage.iconColors = new uint[num4];
			for (int m = 0; m < num4; m++)
			{
				iconImage.iconColors[m] = binaryReader2.ReadUInt32();
			}
			int num5 = iconHeader.biHeight / 2;
			int num6 = (iconHeader.biWidth * iconHeader.biPlanes * iconHeader.biBitCount + 31 >> 5 << 2) * num5;
			iconImage.iconXOR = new byte[num6];
			int num7 = binaryReader2.Read(iconImage.iconXOR, 0, num6);
			if (num7 != num6)
			{
				throw new ArgumentException(global::Locale.GetText("{0} data length expected {1}, read {2}", "XOR", num6, num7), "stream");
			}
			int num8 = (((iconHeader.biWidth + 31) & -32) >> 3) * num5;
			iconImage.iconAND = new byte[num8];
			num7 = binaryReader2.Read(iconImage.iconAND, 0, num8);
			if (num7 != num8)
			{
				throw new ArgumentException(global::Locale.GetText("{0} data length expected {1}, read {2}", "AND", num8, num7), "stream");
			}
			imageData[l] = iconImage;
			binaryReader2.Dispose();
		}
		binaryReader.Dispose();
	}
}
