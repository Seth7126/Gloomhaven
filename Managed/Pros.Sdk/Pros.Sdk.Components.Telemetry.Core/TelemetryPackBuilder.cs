using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using Google.Protobuf;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Helpers;
using RedLynx.Api.Telemetry;

namespace Pros.Sdk.Components.Telemetry.Core;

internal class TelemetryPackBuilder
{
	private int _packNumber;

	private const int MAJOR_VERSION = 2;

	private const int MINOR_VERSION = 0;

	private static readonly Encoding _encoding = Encoding.ASCII;

	private TelemetryPackInfo _packInfo;

	public TelemetryPackBuilder()
	{
		PrepareNextPackInfo();
	}

	public virtual TelemetryPack Build(TelemetryPackOptions options, List<TelemetryEventEntry> entries, SdkSessionInfo sessionInfo, TimeSpan? finalizationTimeout, List<TelemetryContext> context)
	{
		TelemetryPackHeader telemetryPackHeader = new TelemetryPackHeader();
		telemetryPackHeader.InitTime = sessionInfo.InitTime.ToUnixMilliseconds();
		telemetryPackHeader.Options = options;
		telemetryPackHeader.Version = new TelemetryPackFormatVersion
		{
			Major = 2,
			Minor = 0
		};
		telemetryPackHeader.PackNumber = Interlocked.Increment(ref _packNumber);
		telemetryPackHeader.Format = TelemetryPackFormat.JsonBased;
		telemetryPackHeader.Context.Add(context);
		telemetryPackHeader.StartTime = _packInfo.StartTime;
		telemetryPackHeader.EndTime = (_packInfo.IsLocalTime ? DateTime.UtcNow.ToUnixMilliseconds() : TimeHelper.GetBackendTime().Time);
		telemetryPackHeader.Options.IsLocalTime = _packInfo.IsLocalTime;
		byte[][] array = BuildData(entries, compression: false);
		PrepareNextPackInfo();
		return new TelemetryPack(telemetryPackHeader, array[0], array[1]);
	}

	private void PrepareNextPackInfo()
	{
		(bool, long) backendTime = TimeHelper.GetBackendTime();
		_packInfo = new TelemetryPackInfo
		{
			StartTime = backendTime.Item2,
			IsLocalTime = backendTime.Item1
		};
	}

	private byte[][] BuildData(List<TelemetryEventEntry> entries, bool compression)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using MemoryStream memoryStream2 = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream, _encoding);
		using BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream2, _encoding);
		int num = 0;
		foreach (TelemetryEventEntry entry in entries)
		{
			int num2 = entry.Data.CalculateSize();
			binaryWriter.Write(entry.PackTimeOffset);
			binaryWriter.Write(num2);
			binaryWriter.Write(num);
			binaryWriter2.Write(entry.Data.ToByteArray());
			num += num2;
		}
		return new byte[2][]
		{
			compression ? Compress(memoryStream.ToArray()) : memoryStream.ToArray(),
			compression ? Compress(memoryStream2.ToArray()) : memoryStream2.ToArray()
		};
	}

	private byte[] Compress(byte[] input)
	{
		using MemoryStream memoryStream = new MemoryStream(input);
		using MemoryStream memoryStream2 = new MemoryStream();
		using (GZipStream destination = new GZipStream(memoryStream2, CompressionMode.Compress))
		{
			memoryStream.CopyTo(destination);
		}
		return memoryStream2.ToArray();
	}
}
