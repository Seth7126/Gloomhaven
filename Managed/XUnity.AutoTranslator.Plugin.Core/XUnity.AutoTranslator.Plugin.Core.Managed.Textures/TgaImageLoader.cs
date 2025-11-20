using System.IO;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Textures;

namespace XUnity.AutoTranslator.Plugin.Core.Managed.Textures;

internal class TgaImageLoader : ITextureLoader
{
	public void Load(Texture2D texture, byte[] data)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
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
		Color32[] array = (Color32[])(object)new Color32[num * num2];
		if ((int)format == 3)
		{
			if (num3 == 32)
			{
				for (int i = 0; i < num * num2; i++)
				{
					byte b = binaryReader.ReadByte();
					byte b2 = binaryReader.ReadByte();
					byte b3 = binaryReader.ReadByte();
					binaryReader.ReadByte();
					array[i] = Color32.op_Implicit(new Color((float)(int)b3, (float)(int)b2, (float)(int)b, 1f));
				}
			}
			else
			{
				for (int j = 0; j < num * num2; j++)
				{
					byte b4 = binaryReader.ReadByte();
					byte b5 = binaryReader.ReadByte();
					byte b6 = binaryReader.ReadByte();
					array[j] = Color32.op_Implicit(new Color((float)(int)b6, (float)(int)b5, (float)(int)b4, 1f));
				}
			}
		}
		else if (num3 == 32)
		{
			for (int k = 0; k < num * num2; k++)
			{
				byte b7 = binaryReader.ReadByte();
				byte b8 = binaryReader.ReadByte();
				byte b9 = binaryReader.ReadByte();
				byte b10 = binaryReader.ReadByte();
				array[k] = Color32.op_Implicit(new Color((float)(int)b9, (float)(int)b8, (float)(int)b7, (float)(int)b10));
			}
		}
		else
		{
			for (int l = 0; l < num * num2; l++)
			{
				byte b11 = binaryReader.ReadByte();
				byte b12 = binaryReader.ReadByte();
				byte b13 = binaryReader.ReadByte();
				array[l] = Color32.op_Implicit(new Color((float)(int)b13, (float)(int)b12, (float)(int)b11, 1f));
			}
		}
		texture.SetPixels32(array);
		texture.Apply();
	}

	public bool Verify()
	{
		Load(null, null);
		return true;
	}
}
