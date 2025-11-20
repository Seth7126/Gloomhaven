using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Hydra.Sdk.Components.Telemetry.Core;

public static class TelemetrySerializer
{
	public static string Serialize<T>(T instance) where T : TelemetryEvent
	{
		DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings
		{
			EmitTypeInformation = EmitTypeInformation.Never
		});
		using MemoryStream memoryStream = new MemoryStream();
		dataContractJsonSerializer.WriteObject(memoryStream, instance);
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}
}
