using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Photon.Bolt.Matchmaking;
using SevenZip.Compression.LZMA;
using UdpKit.Platform.Photon;

namespace FFSNet;

public static class Utility
{
	public static readonly IEnumerable<(PhotonRegion.Regions, char)> RegionCodePrefixes = new List<(PhotonRegion.Regions, char)>
	{
		(PhotonRegion.Regions.BEST_REGION, 'A'),
		(PhotonRegion.Regions.ASIA, 'B'),
		(PhotonRegion.Regions.AU, 'C'),
		(PhotonRegion.Regions.CAE, 'D'),
		(PhotonRegion.Regions.EU, 'F'),
		(PhotonRegion.Regions.IN, 'G'),
		(PhotonRegion.Regions.JP, 'H'),
		(PhotonRegion.Regions.RU, 'J'),
		(PhotonRegion.Regions.RUE, 'K'),
		(PhotonRegion.Regions.ZA, 'L'),
		(PhotonRegion.Regions.SA, 'M'),
		(PhotonRegion.Regions.KR, 'N'),
		(PhotonRegion.Regions.US, 'P'),
		(PhotonRegion.Regions.USW, 'Q')
	};

	public static string GetRandomCode(int codeLength = 8, bool includeRegionBasedPrefix = false)
	{
		string text = "ABCDEFGHJKLMNPQRSTUVWXY3456789";
		if (codeLength < 3)
		{
			codeLength = 3;
		}
		char[] array = new char[codeLength];
		Random random = new Random();
		for (int i = 0; i < array.Length; i++)
		{
			if (i == 0 && includeRegionBasedPrefix && BoltMatchmaking.CurrentMetadata.TryGetValue("Region", out var region))
			{
				Console.LogInfo("Applying the code prefix for region: " + (string)region);
				array[i] = RegionCodePrefixes.First(((PhotonRegion.Regions, char) x) => x.Item1.ToString().ToLower() == (string)region).Item2;
			}
			else
			{
				array[i] = text[random.Next(text.Length)];
			}
		}
		return new string(array);
	}

	public static int GetBitsRequired(int count)
	{
		if (count <= 0)
		{
			return 1;
		}
		return (int)Math.Log(count, 2.0) + 1;
	}

	public static byte[] CompressData(byte[] data)
	{
		Console.LogInfo("Compressing data.");
		Console.LogInfo("Original data is " + data.Length + " bytes.");
		byte[] array = SevenZipHelper.Compress(data);
		Console.LogInfo("Compressed data is " + array.Length + " bytes.");
		return array;
	}

	public static byte[] DecompressData(byte[] data)
	{
		Console.LogInfo("Decompressing data.");
		Console.LogInfo("Original data is " + data.Length + " bytes.");
		byte[] array = SevenZipHelper.Decompress(data);
		Console.LogInfo("Decompressed data is " + array.Length + " bytes.");
		return array;
	}

	public static byte[] ObjectToByteArray(object obj)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, obj);
		return memoryStream.ToArray();
	}

	public static object ByteArrayToObject(byte[] byteArray)
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		memoryStream.Write(byteArray, 0, byteArray.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return binaryFormatter.Deserialize(memoryStream);
	}
}
