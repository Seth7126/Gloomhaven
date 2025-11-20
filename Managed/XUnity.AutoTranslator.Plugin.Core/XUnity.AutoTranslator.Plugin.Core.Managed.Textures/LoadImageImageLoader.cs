using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Textures;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Managed.Textures;

internal class LoadImageImageLoader : ITextureLoader
{
	public void Load(Texture2D texture, byte[] data)
	{
		if (UnityTypes.ImageConversion_Methods.LoadImage != null)
		{
			UnityTypes.ImageConversion_Methods.LoadImage(texture, data, arg3: false);
		}
		else if (UnityTypes.Texture2D_Methods.LoadImage != null)
		{
			UnityTypes.Texture2D_Methods.LoadImage(texture, data);
		}
	}

	public bool Verify()
	{
		if (UnityTypes.Texture2D_Methods.LoadImage == null)
		{
			return UnityTypes.ImageConversion_Methods.LoadImage != null;
		}
		return true;
	}
}
