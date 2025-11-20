using UnityEngine;

namespace DynamicFogAndMist;

[ExecuteInEditMode]
public class DynamicFogOfWar : MonoBehaviour
{
	public int fogOfWarTextureSize = 512;

	private Material fogMat;

	private static DynamicFogOfWar _instance;

	private Texture2D fogOfWarTexture;

	private Color32[] fogOfWarColorBuffer;

	public static DynamicFogOfWar instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<DynamicFogOfWar>();
			}
			return _instance;
		}
	}

	private void OnEnable()
	{
		fogMat = GetComponent<MeshRenderer>().sharedMaterial;
		UpdateFogOfWarTexture();
	}

	private void OnDisable()
	{
		if (fogOfWarTexture != null)
		{
			Object.DestroyImmediate(fogOfWarTexture);
			fogOfWarTexture = null;
		}
	}

	private void Update()
	{
		fogMat.SetVector("_FogOfWarData", new Vector4(base.transform.position.x, base.transform.position.z, base.transform.localScale.x, base.transform.localScale.y));
	}

	private void UpdateFogOfWarTexture()
	{
		int scaledSize = GetScaledSize(fogOfWarTextureSize, 1f);
		fogOfWarTexture = new Texture2D(scaledSize, scaledSize, TextureFormat.ARGB32, mipChain: false);
		fogOfWarTexture.hideFlags = HideFlags.DontSave;
		fogOfWarTexture.filterMode = FilterMode.Bilinear;
		fogOfWarTexture.wrapMode = TextureWrapMode.Clamp;
		fogMat.mainTexture = fogOfWarTexture;
		ResetFogOfWar();
	}

	private int GetScaledSize(int size, float factor)
	{
		size = (int)((float)size / factor);
		size /= 4;
		if (size < 1)
		{
			size = 1;
		}
		return size * 4;
	}

	public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha)
	{
		if (fogOfWarTexture == null)
		{
			return;
		}
		float num = (worldPosition.x - base.transform.position.x) / base.transform.localScale.x + 0.5f;
		if (num < 0f || num > 1f)
		{
			return;
		}
		float num2 = (worldPosition.z - base.transform.position.z) / base.transform.localScale.y + 0.5f;
		if (num2 < 0f || num2 > 1f)
		{
			return;
		}
		int width = fogOfWarTexture.width;
		int height = fogOfWarTexture.height;
		int num3 = (int)(num * (float)width);
		int num4 = (int)(num2 * (float)height);
		int num5 = num4 * width + num3;
		byte b = (byte)(fogNewAlpha * 255f);
		Color32 color = fogOfWarColorBuffer[num5];
		if (b == color.a)
		{
			return;
		}
		float num6 = radius / base.transform.localScale.y;
		int num7 = Mathf.FloorToInt((float)height * num6);
		for (int i = num4 - num7; i <= num4 + num7; i++)
		{
			if (i < 0 || i >= height)
			{
				continue;
			}
			for (int j = num3 - num7; j <= num3 + num7; j++)
			{
				if (j >= 0 && j < width)
				{
					int num8 = (int)Mathf.Sqrt((num4 - i) * (num4 - i) + (num3 - j) * (num3 - j));
					if (num8 <= num7)
					{
						num5 = i * width + j;
						Color32 color2 = fogOfWarColorBuffer[num5];
						color2.a = (byte)Mathf.Lerp((int)b, (int)color2.a, (float)num8 / (float)num7);
						fogOfWarColorBuffer[num5] = color2;
						fogOfWarTexture.SetPixel(j, i, color2);
					}
				}
			}
		}
		fogOfWarTexture.Apply();
	}

	public void ResetFogOfWar()
	{
		if (!(fogOfWarTexture == null))
		{
			int height = fogOfWarTexture.height;
			int width = fogOfWarTexture.width;
			int num = height * width;
			if (fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length != num)
			{
				fogOfWarColorBuffer = new Color32[num];
			}
			Color32 color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			for (int i = 0; i < num; i++)
			{
				fogOfWarColorBuffer[i] = color;
			}
			fogOfWarTexture.SetPixels32(fogOfWarColorBuffer);
			fogOfWarTexture.Apply();
		}
	}

	public void SetFogOfWarTerrainBoundary(Terrain terrain, float borderWidth)
	{
		TerrainData terrainData = terrain.terrainData;
		int heightmapResolution = terrainData.heightmapResolution;
		int heightmapResolution2 = terrainData.heightmapResolution;
		float y = terrainData.size.y;
		float[,] heights = terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution2);
		float num = base.transform.position.y - 1f;
		float num2 = base.transform.position.y + 10f;
		Vector3 position = terrain.GetPosition();
		for (int i = 0; i < heightmapResolution2; i++)
		{
			for (int j = 0; j < heightmapResolution; j++)
			{
				float num3 = heights[i, j] * y + terrain.transform.position.y;
				if (num3 > num && num3 < num2)
				{
					Vector3 worldPosition = position + new Vector3(terrainData.size.x * ((float)j + 0.5f) / (float)heightmapResolution, 0f, terrainData.size.z * ((float)i + 0.5f) / (float)heightmapResolution2);
					SetFogOfWarAlpha(worldPosition, borderWidth, 0f);
				}
			}
		}
	}
}
