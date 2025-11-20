using System;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Debugging;

internal class ConsoleEncoding : Encoding
{
	private byte[] _byteBuffer = new byte[256];

	private char[] _charBuffer = new char[256];

	private byte[] _zeroByte = new byte[0];

	private char[] _zeroChar = new char[0];

	private readonly uint _codePage;

	public override int CodePage => (int)_codePage;

	private ConsoleEncoding(uint codePage)
	{
		_codePage = codePage;
	}

	public static ConsoleEncoding GetEncoding(uint codePage)
	{
		return new ConsoleEncoding(codePage);
	}

	public override int GetByteCount(char[] chars, int index, int count)
	{
		WriteCharBuffer(chars, index, count);
		return Kernel32.WideCharToMultiByte(_codePage, 0u, _charBuffer, count, _zeroByte, 0, IntPtr.Zero, IntPtr.Zero);
	}

	public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		int maxByteCount = GetMaxByteCount(charCount);
		WriteCharBuffer(chars, charIndex, charCount);
		ExpandByteBuffer(maxByteCount);
		int result = Kernel32.WideCharToMultiByte(_codePage, 0u, chars, charCount, _byteBuffer, maxByteCount, IntPtr.Zero, IntPtr.Zero);
		ReadByteBuffer(bytes, byteIndex, maxByteCount);
		return result;
	}

	public override int GetCharCount(byte[] bytes, int index, int count)
	{
		WriteByteBuffer(bytes, index, count);
		return Kernel32.MultiByteToWideChar(_codePage, 0u, bytes, count, _zeroChar, 0);
	}

	public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
	{
		int maxCharCount = GetMaxCharCount(byteCount);
		WriteByteBuffer(bytes, byteIndex, byteCount);
		ExpandCharBuffer(maxCharCount);
		int result = Kernel32.MultiByteToWideChar(_codePage, 0u, bytes, byteCount, _charBuffer, maxCharCount);
		ReadCharBuffer(chars, charIndex, maxCharCount);
		return result;
	}

	public override int GetMaxByteCount(int charCount)
	{
		return charCount * 2;
	}

	public override int GetMaxCharCount(int byteCount)
	{
		return byteCount;
	}

	private void ExpandByteBuffer(int count)
	{
		if (_byteBuffer.Length < count)
		{
			_byteBuffer = new byte[count];
		}
	}

	private void ExpandCharBuffer(int count)
	{
		if (_charBuffer.Length < count)
		{
			_charBuffer = new char[count];
		}
	}

	private void ReadByteBuffer(byte[] bytes, int index, int count)
	{
		for (int i = 0; i < count; i++)
		{
			bytes[index + i] = _byteBuffer[i];
		}
	}

	private void ReadCharBuffer(char[] chars, int index, int count)
	{
		for (int i = 0; i < count; i++)
		{
			chars[index + i] = _charBuffer[i];
		}
	}

	private void WriteByteBuffer(byte[] bytes, int index, int count)
	{
		ExpandByteBuffer(count);
		for (int i = 0; i < count; i++)
		{
			_byteBuffer[i] = bytes[index + i];
		}
	}

	private void WriteCharBuffer(char[] chars, int index, int count)
	{
		ExpandCharBuffer(count);
		for (int i = 0; i < count; i++)
		{
			_charBuffer[i] = chars[index + i];
		}
	}
}
