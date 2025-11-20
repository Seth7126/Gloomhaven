using System.IO;
using System.Reflection;

namespace System.Drawing;

/// <summary>Allows you to specify an icon to represent a control in a container, such as the Microsoft Visual Studio Form Designer.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Class)]
public class ToolboxBitmapAttribute : Attribute
{
	private Image smallImage;

	private Image bigImage;

	/// <summary>A <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object that has its small image and its large image set to null.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly ToolboxBitmapAttribute Default = new ToolboxBitmapAttribute();

	private ToolboxBitmapAttribute()
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object with an image from a specified file.</summary>
	/// <param name="imageFile">The name of a file that contains a 16 by 16 bitmap. </param>
	public ToolboxBitmapAttribute(string imageFile)
	{
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object based on a 16 x 16 bitmap that is embedded as a resource in a specified assembly.</summary>
	/// <param name="t">A <see cref="T:System.Type" /> whose defining assembly is searched for the bitmap resource. </param>
	public ToolboxBitmapAttribute(Type t)
	{
		smallImage = GetImageFromResource(t, null, large: false);
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object based on a 16 by 16 bitmap that is embedded as a resource in a specified assembly.</summary>
	/// <param name="t">A <see cref="T:System.Type" /> whose defining assembly is searched for the bitmap resource. </param>
	/// <param name="name">The name of the embedded bitmap resource. </param>
	public ToolboxBitmapAttribute(Type t, string name)
	{
		smallImage = GetImageFromResource(t, name, large: false);
	}

	/// <summary>Indicates whether the specified object is a <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object and is identical to this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</summary>
	/// <returns>This method returns true if <paramref name="value" /> is both a <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object and is identical to this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to test. </param>
	/// <filterpriority>1</filterpriority>
	public override bool Equals(object value)
	{
		if (!(value is ToolboxBitmapAttribute))
		{
			return false;
		}
		if (value == this)
		{
			return true;
		}
		return ((ToolboxBitmapAttribute)value).smallImage == smallImage;
	}

	/// <summary>Gets a hash code for this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</summary>
	/// <returns>The hash code for this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public override int GetHashCode()
	{
		return smallImage.GetHashCode() ^ bigImage.GetHashCode();
	}

	/// <summary>Gets the small <see cref="T:System.Drawing.Image" /> associated with this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</summary>
	/// <returns>The small <see cref="T:System.Drawing.Image" /> associated with this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</returns>
	/// <param name="component">If this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object does not already have a small image, this method searches for a bitmap resource in the assembly that defines the type of the object specified by the component parameter. For example, if you pass an object of type ControlA to the component parameter, then this method searches the assembly that defines ControlA. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Image GetImage(object component)
	{
		return GetImage(component.GetType(), null, large: false);
	}

	/// <summary>Gets the small or large <see cref="T:System.Drawing.Image" /> associated with this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</summary>
	/// <returns>An <see cref="T:System.Drawing.Image" /> object associated with this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</returns>
	/// <param name="component">If this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object does not already have a small image, this method searches for a bitmap resource in the assembly that defines the type of the object specified by the component parameter. For example, if you pass an object of type ControlA to the component parameter, then this method searches the assembly that defines ControlA. </param>
	/// <param name="large">Specifies whether this method returns a large image (true) or a small image (false). The small image is 16 by 16, and the large image is 32 by 32. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Image GetImage(object component, bool large)
	{
		return GetImage(component.GetType(), null, large);
	}

	/// <summary>Gets the small <see cref="T:System.Drawing.Image" /> associated with this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</summary>
	/// <returns>The small <see cref="T:System.Drawing.Image" /> associated with this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</returns>
	/// <param name="type">If this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object does not already have a small image, this method searches for a bitmap resource in the assembly that defines the type specified by the type parameter. For example, if you pass typeof(ControlA) to the type parameter, then this method searches the assembly that defines ControlA. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Image GetImage(Type type)
	{
		return GetImage(type, null, large: false);
	}

	/// <summary>Gets the small or large <see cref="T:System.Drawing.Image" /> associated with this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</summary>
	/// <returns>An <see cref="T:System.Drawing.Image" /> associated with this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</returns>
	/// <param name="type">If this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object does not already have a small image, this method searches for a bitmap resource in the assembly that defines the type specified by the component type. For example, if you pass typeof(ControlA) to the type parameter, then this method searches the assembly that defines ControlA. </param>
	/// <param name="large">Specifies whether this method returns a large image (true) or a small image (false). The small image is 16 by 16, and the large image is 32 by 32. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Image GetImage(Type type, bool large)
	{
		return GetImage(type, null, large);
	}

	/// <summary>Gets the small or large <see cref="T:System.Drawing.Image" /> associated with this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</summary>
	/// <returns>An <see cref="T:System.Drawing.Image" /> associated with this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object.</returns>
	/// <param name="type">If this <see cref="T:System.Drawing.ToolboxBitmapAttribute" /> object does not already have a small image, this method searches for an embedded bitmap resource in the assembly that defines the type specified by the component type. For example, if you pass typeof(ControlA) to the type parameter, then this method searches the assembly that defines ControlA. </param>
	/// <param name="imgName">The name of the embedded bitmap resource. </param>
	/// <param name="large">Specifies whether this method returns a large image (true) or a small image (false). The small image is 16 by 16, and the large image is 32 by 32. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Image GetImage(Type type, string imgName, bool large)
	{
		if (smallImage == null)
		{
			smallImage = GetImageFromResource(type, imgName, large: false);
		}
		if (large)
		{
			if (bigImage == null)
			{
				bigImage = new Bitmap(smallImage, 32, 32);
			}
			return bigImage;
		}
		return smallImage;
	}

	/// <summary>Returns an <see cref="T:System.Drawing.Image" /> object based on a bitmap resource that is embedded in an assembly.</summary>
	/// <returns>An <see cref="T:System.Drawing.Image" /> object based on the retrieved bitmap.</returns>
	/// <param name="t">This method searches for an embedded bitmap resource in the assembly that defines the type specified by the t parameter. For example, if you pass typeof(ControlA) to the t parameter, then this method searches the assembly that defines ControlA. </param>
	/// <param name="imageName">The name of the embedded bitmap resource. </param>
	/// <param name="large">Specifies whether this method returns a large image (true)or a small image (false). The small image is 16 by 16, and the large image is 32 x 32. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Image GetImageFromResource(Type t, string imageName, bool large)
	{
		if (imageName == null)
		{
			imageName = t.Name + ".bmp";
		}
		try
		{
			Bitmap bitmap;
			using (Stream stream = t.GetTypeInfo().Assembly.GetManifestResourceStream(t.Namespace + "." + imageName))
			{
				if (stream == null)
				{
					return null;
				}
				bitmap = new Bitmap(stream, useIcm: false);
			}
			if (large)
			{
				return new Bitmap(bitmap, 32, 32);
			}
			return bitmap;
		}
		catch
		{
			return null;
		}
	}
}
