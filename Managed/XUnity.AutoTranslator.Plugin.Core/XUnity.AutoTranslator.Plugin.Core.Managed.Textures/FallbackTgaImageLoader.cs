using System.IO;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Textures;

namespace XUnity.AutoTranslator.Plugin.Core.Managed.Textures;

internal class FallbackTgaImageLoader : ITextureLoader
{
	public void Load(Texture2D texture, byte[] data)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)texture == (Object)null && data == null)
		{
			return;
		}
		TextureFormat format = texture.format;
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader binaryReader = new BinaryReader(input);
		binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
		short num = binaryReader.ReadInt16();
		short num2 = binaryReader.ReadInt16();
		int num3 = binaryReader.ReadByte();
		binaryReader.BaseStream.Seek(1L, SeekOrigin.Current);
		Color[] array = (Color[])(object)new Color[num * num2];
		if ((int)format == 3)
		{
			if (num3 == 32)
			{
				for (int i = 0; i < num * num2; i++)
				{
					float num4 = (float)(int)binaryReader.ReadByte() / 255f;
					float num5 = (float)(int)binaryReader.ReadByte() / 255f;
					float num6 = (float)(int)binaryReader.ReadByte() / 255f;
					binaryReader.ReadByte();
					array[i] = new Color(num6, num5, num4, 1f);
				}
			}
			else
			{
				for (int j = 0; j < num * num2; j++)
				{
					float num7 = (float)(int)binaryReader.ReadByte() / 255f;
					float num8 = (float)(int)binaryReader.ReadByte() / 255f;
					float num9 = (float)(int)binaryReader.ReadByte() / 255f;
					array[j] = new Color(num9, num8, num7, 1f);
				}
			}
		}
		else if (num3 == 32)
		{
			for (int k = 0; k < num * num2; k++)
			{
				float num10 = (float)(int)binaryReader.ReadByte() / 255f;
				float num11 = (float)(int)binaryReader.ReadByte() / 255f;
				float num12 = (float)(int)binaryReader.ReadByte() / 255f;
				float num13 = (float)(int)binaryReader.ReadByte() / 255f;
				array[k] = new Color(num12, num11, num10, num13);
			}
		}
		else
		{
			for (int l = 0; l < num * num2; l++)
			{
				float num14 = (float)(int)binaryReader.ReadByte() / 255f;
				float num15 = (float)(int)binaryReader.ReadByte() / 255f;
				float num16 = (float)(int)binaryReader.ReadByte() / 255f;
				array[l] = new Color(num16, num15, num14, 1f);
			}
		}
		texture.SetPixels(array);
		texture.Apply();
	}

	public bool Verify()
	{
		Load(null, null);
		return true;
	}
}
