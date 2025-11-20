using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Hydra.Api.Facts;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Helpers;

namespace Hydra.Sdk.Components.Facts.Core;

internal class FactPackBuilder
{
	private int _packNumber;

	private const int MAJOR_VERSION = 2;

	private const int MINOR_VERSION = 0;

	private static readonly Encoding _encoding = Encoding.ASCII;

	private DateTime _previousPackTime = DateTime.UtcNow;

	public virtual FactPack Build(FactsPackOptions options, List<FactEntry> entries, SdkSessionInfo sessionInfo, TimeSpan? finalizationTimeout, List<FactsContext> context)
	{
		entries.Add(new FactEntry("SDK/Hydra/" + FactConstants.ServiceName + "/WriteBinaryPack", null, "Entries: {0}, Total size: {1}", entries.Count, entries.Sum((FactEntry e) => e.Size)));
		FactsPackHeader factsPackHeader = new FactsPackHeader
		{
			InitTime = sessionInfo.InitTime.ToUnixMilliseconds(),
			StartTime = _previousPackTime.ToUnixMilliseconds(),
			EndTime = DateTime.UtcNow.ToUnixMilliseconds(),
			Options = options,
			Version = new FactsPackFormatVersion
			{
				Major = 2,
				Minor = 0
			},
			PackNumber = Interlocked.Increment(ref _packNumber),
			Format = FactsPackFormat.NetClientPack,
			Context = { (IEnumerable<FactsContext>)context }
		};
		byte[][] array = BuildData(entries, sessionInfo.StartTime, factsPackHeader.Options.IsCompressed);
		_previousPackTime = DateTime.UtcNow;
		return new FactPack(factsPackHeader, array[0], array[1]);
	}

	public static string GetContextValue(FactsContext context)
	{
		if (context == null)
		{
			return null;
		}
		return context.PropertyName + "=" + context.PropertyValue;
	}

	public static void WriteArgument(object arg, BinaryWriter argsWriter)
	{
		if (arg == null)
		{
			return;
		}
		Type type = arg.GetType();
		if (type.GetTypeInfo().IsEnum)
		{
			argsWriter.Write((byte)18);
			argsWriter.Write(EntrySizeCalculator.EnumToString(arg, type));
			return;
		}
		TypeCode typeCode = Type.GetTypeCode(type);
		argsWriter.Write((byte)typeCode);
		switch (typeCode)
		{
		case TypeCode.Boolean:
			argsWriter.Write((bool)arg);
			break;
		case TypeCode.SByte:
			argsWriter.Write((sbyte)arg);
			break;
		case TypeCode.Byte:
			argsWriter.Write((byte)arg);
			break;
		case TypeCode.Char:
			argsWriter.Write((char)arg);
			break;
		case TypeCode.Int16:
			argsWriter.Write((short)arg);
			break;
		case TypeCode.UInt16:
			argsWriter.Write((ushort)arg);
			break;
		case TypeCode.Single:
			argsWriter.Write((float)arg);
			break;
		case TypeCode.Int32:
			argsWriter.Write((int)arg);
			break;
		case TypeCode.UInt32:
			argsWriter.Write((uint)arg);
			break;
		case TypeCode.Double:
			argsWriter.Write((double)arg);
			break;
		case TypeCode.Int64:
			argsWriter.Write((long)arg);
			break;
		case TypeCode.UInt64:
			argsWriter.Write((ulong)arg);
			break;
		case TypeCode.Decimal:
			argsWriter.Write((decimal)arg);
			break;
		case TypeCode.String:
			argsWriter.Write((string)arg);
			break;
		case TypeCode.DateTime:
			argsWriter.Write(((DateTime)arg).ToUnixMilliseconds());
			break;
		case TypeCode.Object:
			if (arg is Guid)
			{
				argsWriter.Write(((Guid)arg).ToByteArray());
				break;
			}
			argsWriter.Seek(-1, SeekOrigin.Current);
			argsWriter.Write((byte)18);
			argsWriter.Write(arg.ToString());
			break;
		default:
			throw new ArgumentException("Invalid argument type. Argument could be of a primitive type, System.DateTime, System.Decimal, System.Guid or System.String");
		}
	}

	public static void WriteString(Dictionary<string, int> mapping, string value, Stream argsStream, BinaryWriter argsWriter, BinaryWriter dataWriter)
	{
		int value2 = -1;
		if (!string.IsNullOrEmpty(value) && !mapping.TryGetValue(value, out value2))
		{
			value2 = (int)argsStream.Position;
			argsWriter.Write(value);
			mapping.Add(value, value2);
		}
		dataWriter.Write(value2);
	}

	public static byte[] Compress(byte[] input)
	{
		using MemoryStream memoryStream = new MemoryStream(input);
		using MemoryStream memoryStream2 = new MemoryStream();
		using (GZipStream destination = new GZipStream(memoryStream2, CompressionMode.Compress))
		{
			memoryStream.CopyTo(destination);
		}
		return memoryStream2.ToArray();
	}

	private byte[][] BuildData(List<FactEntry> entries, DateTime packTime, bool compression)
	{
		Dictionary<string, int> mapping = new Dictionary<string, int>();
		using MemoryStream memoryStream = new MemoryStream();
		using MemoryStream memoryStream2 = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream, _encoding);
		using BinaryWriter argsWriter = new BinaryWriter(memoryStream2, _encoding);
		foreach (FactEntry entry in entries)
		{
			binaryWriter.Write((int)(entry.Time - packTime).TotalMilliseconds);
			WriteString(mapping, entry.Category, memoryStream2, argsWriter, binaryWriter);
			WriteString(mapping, GetContextValue(entry.Context), memoryStream2, argsWriter, binaryWriter);
			WriteString(mapping, entry.Description, memoryStream2, argsWriter, binaryWriter);
			binaryWriter.Write(entry.Args.Length);
			if (entry.Args.Length != 0)
			{
				binaryWriter.Write((int)memoryStream2.Position);
			}
			object[] args = entry.Args;
			foreach (object arg in args)
			{
				WriteArgument(arg, argsWriter);
			}
		}
		return new byte[2][]
		{
			compression ? Compress(memoryStream.ToArray()) : memoryStream.ToArray(),
			compression ? Compress(memoryStream2.ToArray()) : memoryStream2.ToArray()
		};
	}
}
