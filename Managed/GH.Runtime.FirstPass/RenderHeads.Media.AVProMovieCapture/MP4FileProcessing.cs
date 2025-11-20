#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.IO;
using SM.Utils;

namespace RenderHeads.Media.AVProMovieCapture;

public class MP4FileProcessing
{
	private class Chunk
	{
		public uint id;

		public long size;

		public long offset;
	}

	private const int ChunkHeaderSize = 8;

	private const int CopyBufferSize = 65536;

	private static uint Atom_moov = ChunkId("moov");

	private static uint Atom_mdat = ChunkId("mdat");

	private static uint Atom_cmov = ChunkId("cmov");

	private static uint Atom_trak = ChunkId("trak");

	private static uint Atom_mdia = ChunkId("mdia");

	private static uint Atom_minf = ChunkId("minf");

	private static uint Atom_stbl = ChunkId("stbl");

	private static uint Atom_stco = ChunkId("stco");

	private static uint Atom_co64 = ChunkId("co64");

	private BinaryReader _reader;

	private Stream _writeFile;

	public static bool ApplyFastStart(string filePath, bool keepBackup)
	{
		if (!File.Exists(filePath))
		{
			LogUtils.LogError("File not found: " + filePath);
			return false;
		}
		string text = filePath + "-" + Guid.NewGuid().ToString() + ".temp";
		bool num = ApplyFastStart(filePath, text);
		if (num)
		{
			string text2 = filePath + "-" + Guid.NewGuid().ToString() + ".backup";
			File.Move(filePath, text2);
			File.Move(text, filePath);
			if (!keepBackup)
			{
				File.Delete(text2);
			}
		}
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		return num;
	}

	public static bool ApplyFastStart(string srcPath, string dstPath)
	{
		if (!File.Exists(srcPath))
		{
			LogUtils.LogError("File not found: " + srcPath);
			return false;
		}
		using Stream srcStream = new FileStream(srcPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using Stream dstStream = new FileStream(dstPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		MP4FileProcessing mP4FileProcessing = new MP4FileProcessing();
		bool result = mP4FileProcessing.Open(srcStream, dstStream);
		mP4FileProcessing.Close();
		return result;
	}

	public bool Open(Stream srcStream, Stream dstStream)
	{
		Close();
		_reader = new BinaryReader(srcStream);
		List<Chunk> list = ReadChildChunks(null);
		Chunk firstChunkOfType = GetFirstChunkOfType(Atom_moov, list);
		Chunk firstChunkOfType2 = GetFirstChunkOfType(Atom_mdat, list);
		bool flag = firstChunkOfType != null && firstChunkOfType2 != null;
		bool flag2 = false;
		if (flag)
		{
			flag2 = firstChunkOfType.offset > firstChunkOfType2.offset;
			if (flag2)
			{
				if (ChunkContainsChildChunkWithId(firstChunkOfType, Atom_cmov))
				{
					LogUtils.LogError("moov chunk is compressed - unsupported");
					flag = false;
				}
			}
			else
			{
				LogUtils.Log("No reordering needed");
			}
		}
		else
		{
			LogUtils.LogWarning("no chunk tags found - incorrect file format?");
		}
		if (flag && flag2)
		{
			LogUtils.Assert(firstChunkOfType.offset > firstChunkOfType2.offset, string.Empty);
			ulong size = (ulong)firstChunkOfType.size;
			_writeFile = dstStream;
			foreach (Chunk item in list)
			{
				if (item == firstChunkOfType2)
				{
					WriteChunk_moov(firstChunkOfType, size);
					WriteChunk(item);
				}
				else if (item != firstChunkOfType)
				{
					WriteChunk(item);
				}
			}
			return true;
		}
		return false;
	}

	public void Close()
	{
		if (_reader != null)
		{
			_reader.Close();
			_reader = null;
		}
	}

	private static Chunk GetFirstChunkOfType(uint id, List<Chunk> chunks)
	{
		Chunk result = null;
		foreach (Chunk chunk in chunks)
		{
			if (chunk.id == id)
			{
				result = chunk;
				break;
			}
		}
		return result;
	}

	private List<Chunk> ReadChildChunks(Chunk parentChunk)
	{
		long offset = 0L;
		if (parentChunk != null)
		{
			offset = parentChunk.offset + 8;
		}
		_reader.BaseStream.Seek(offset, SeekOrigin.Begin);
		long num = _reader.BaseStream.Length;
		if (parentChunk != null)
		{
			num = parentChunk.offset + parentChunk.size;
		}
		List<Chunk> list = new List<Chunk>();
		if (_reader.BaseStream.Position < num)
		{
			Chunk chunk = ReadChunkHeader();
			while (chunk != null && _reader.BaseStream.Position < num)
			{
				list.Add(chunk);
				_reader.BaseStream.Seek(chunk.offset + chunk.size, SeekOrigin.Begin);
				chunk = ReadChunkHeader();
			}
		}
		return list;
	}

	private Chunk ReadChunkHeader()
	{
		Chunk chunk = null;
		if (_reader.BaseStream.Length - _reader.BaseStream.Position >= 8)
		{
			chunk = new Chunk();
			chunk.offset = _reader.BaseStream.Position;
			chunk.size = ReadUInt32();
			chunk.id = _reader.ReadUInt32();
			if (chunk.size == 1)
			{
				chunk.size = (long)ReadUInt64();
			}
			if (chunk.size == 0L)
			{
				chunk.size = _reader.BaseStream.Length - chunk.offset;
			}
		}
		return chunk;
	}

	private bool ChunkContainsChildChunkWithId(Chunk chunk, uint id)
	{
		bool result = false;
		long num = chunk.size + chunk.offset;
		_reader.BaseStream.Seek(chunk.offset, SeekOrigin.Begin);
		Chunk chunk2 = ReadChunkHeader();
		while (chunk2 != null && _reader.BaseStream.Position < num)
		{
			if (chunk2.id == id)
			{
				result = true;
				break;
			}
			_reader.BaseStream.Seek(chunk2.offset + chunk2.size, SeekOrigin.Begin);
			chunk2 = ReadChunkHeader();
		}
		return result;
	}

	private void WriteChunk(Chunk chunk)
	{
		_reader.BaseStream.Seek(chunk.offset, SeekOrigin.Begin);
		CopyBytes(chunk.size);
	}

	private void WriteChunkHeader(Chunk chunk)
	{
		_reader.BaseStream.Seek(chunk.offset, SeekOrigin.Begin);
		CopyBytes(8L);
	}

	private void CopyBytes(long numBytes)
	{
		byte[] array = new byte[65536];
		long num = numBytes;
		Stream baseStream = _reader.BaseStream;
		while (num > 0)
		{
			int num2 = array.Length;
			if (num < array.Length)
			{
				num2 = (int)num;
			}
			baseStream.Read(array, 0, num2);
			_writeFile.Write(array, 0, num2);
			num -= num2;
		}
	}

	private void WriteChunk_moov(Chunk parentChunk, ulong byteOffset)
	{
		WriteChunkHeader(parentChunk);
		List<Chunk> list = ReadChildChunks(parentChunk);
		_reader.BaseStream.Seek(parentChunk.offset + 8, SeekOrigin.Begin);
		foreach (Chunk item in list)
		{
			if (item.id == Atom_stco)
			{
				WriteChunkHeader(item);
				CopyBytes(4L);
				uint num = ReadUInt32();
				WriteUInt32(num);
				for (int i = 0; i < num; i++)
				{
					uint num2 = ReadUInt32();
					num2 += (uint)(int)byteOffset;
					WriteUInt32(num2);
				}
			}
			else if (item.id == Atom_co64)
			{
				WriteChunkHeader(item);
				CopyBytes(4L);
				uint num3 = ReadUInt32();
				WriteUInt32(num3);
				for (int j = 0; j < num3; j++)
				{
					ulong num4 = ReadUInt64();
					num4 += byteOffset;
					WriteUInt64(num4);
				}
			}
			else if (item.id == Atom_trak || item.id == Atom_mdia || item.id == Atom_minf || item.id == Atom_stbl)
			{
				WriteChunk_moov(item, byteOffset);
			}
			else
			{
				WriteChunk(item);
			}
		}
	}

	private uint ReadUInt32()
	{
		byte[] array = _reader.ReadBytes(4);
		Array.Reverse(array);
		return BitConverter.ToUInt32(array, 0);
	}

	private ulong ReadUInt64()
	{
		byte[] array = _reader.ReadBytes(8);
		Array.Reverse(array);
		return BitConverter.ToUInt64(array, 0);
	}

	private void WriteUInt32(uint value, bool isBigEndian = true)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		if (isBigEndian)
		{
			Array.Reverse(bytes);
		}
		_writeFile.Write(bytes, 0, bytes.Length);
	}

	private void WriteUInt64(ulong value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse(bytes);
		_writeFile.Write(bytes, 0, bytes.Length);
	}

	private static string ChunkType(uint id)
	{
		char c = (char)(id & 0xFF);
		char c2 = (char)((id >> 8) & 0xFF);
		char c3 = (char)((id >> 16) & 0xFF);
		char c4 = (char)((id >> 24) & 0xFF);
		return $"{c}{c2}{c3}{c4}";
	}

	private static uint ChunkId(string id)
	{
		char num = id[3];
		uint num2 = id[2];
		uint num3 = id[1];
		uint num4 = id[0];
		return ((uint)num << 24) | (num2 << 16) | (num3 << 8) | num4;
	}
}
