using System;
using System.Text;

namespace Apparance.Net;

public static class Engine
{
	public class MonoPInvokeCallback : Attribute
	{
		public MonoPInvokeCallback(Type t)
		{
		}
	}

	private static string m_ProcedureDir;

	private static ILogging m_Logging;

	private static int m_SynthesiserCount;

	private static int m_BufferSize;

	private static int m_LiveEditing;

	private static LogMessageFn m_LogCallback;

	public static bool IsRunning => Interop.ApparanceIsRunning() != 0;

	public static void Start(string procedure_dir, ILogging logging_output = null, int synthesiser_count = 4, int buffer_size = 100, bool live_editing = true)
	{
		m_ProcedureDir = procedure_dir;
		m_Logging = logging_output;
		m_SynthesiserCount = synthesiser_count;
		m_BufferSize = buffer_size;
		m_LiveEditing = (live_editing ? 1 : 0);
		m_LogCallback = LogHandler;
		if (Interop.ApparanceInitialise(procedure_dir, m_LogCallback, m_SynthesiserCount, m_BufferSize, m_LiveEditing) == 0)
		{
			throw new ApplicationException("Apparance Engine failed to start");
		}
	}

	public static string AssetRequestPoll(out int entity_context, out int asset_id)
	{
		return Interop.ApparanceGetNextAssetRequest(out entity_context, out asset_id);
	}

	public static void AssetRequestResponse(int entity_context, string asset_name, int asset_id, bool bounds_available, Vector3 min_extents, Vector3 max_extents, int variant_count)
	{
		Frame frame = default(Frame);
		frame.AxisX = new Vector3(1f, 0f, 0f);
		frame.AxisY = new Vector3(0f, 1f, 0f);
		frame.AxisZ = new Vector3(0f, 0f, 1f);
		frame.Origin = new Vector3(min_extents.X, min_extents.Y, min_extents.Z);
		frame.Size = new Vector3(max_extents.X - min_extents.X, max_extents.Y - min_extents.Y, max_extents.Z - min_extents.Z);
		Interop.ApparanceUpdateAsset(entity_context, asset_name, asset_id, bounds_available ? 1 : 0, frame.AsFloats(), variant_count);
	}

	public static void AssetCacheClear()
	{
		Interop.ApparanceUpdateAsset(0, null, 0, 0, null, 0);
	}

	public static void Update(float dt, Vector3 view_position)
	{
		Interop.ApparanceUpdate(dt, view_position);
		Entity.UpdateAll(dt);
	}

	public static Entity CreateEntity(int handle = 0)
	{
		return new Entity(handle);
	}

	public static void Stop()
	{
		Interop.ApparanceShutdown();
	}

	[MonoPInvokeCallback(typeof(LogMessageFn))]
	private static void LogHandler(string message, int type)
	{
		switch (type)
		{
		case 0:
			LogMessage(message);
			break;
		case 1:
			LogWarning(message);
			break;
		case 2:
			LogError(message);
			break;
		}
	}

	internal static void LogMessage(string message)
	{
		if (m_Logging != null)
		{
			m_Logging.LogMessage(message);
		}
	}

	internal static void LogWarning(string message)
	{
		if (m_Logging != null)
		{
			m_Logging.LogWarning(message);
		}
	}

	internal static void LogError(string message)
	{
		if (m_Logging != null)
		{
			m_Logging.LogError(message);
		}
	}

	internal static void LogBytes(byte[] data, int start = 0, int count = -1, string prefix = "")
	{
		int num = data.Length;
		int i = start;
		if (count < 0)
		{
			count = num;
		}
		int num2 = Math.Min(start + count, num);
		StringBuilder stringBuilder = new StringBuilder();
		int num3;
		for (; i < num2; i += num3)
		{
			num3 = Math.Min(num2 - i, 16);
			stringBuilder.Clear();
			stringBuilder.Append(prefix);
			stringBuilder.Append(i.ToString("X04"));
			stringBuilder.Append("  ");
			int j;
			for (j = 0; j < num3; j++)
			{
				int num4 = data[i + j];
				stringBuilder.Append(' ');
				stringBuilder.Append(num4.ToString("X02"));
			}
			for (; j < 16; j++)
			{
				stringBuilder.Append("   ");
			}
			stringBuilder.Append("  ");
			for (j = 0; j < num3; j++)
			{
				char c = (char)data[i + j];
				if (c >= ' ')
				{
					if (c < '\u0080')
					{
						stringBuilder.Append(c);
					}
					else
					{
						stringBuilder.Append('.');
					}
				}
				else
				{
					stringBuilder.Append('.');
				}
			}
			LogMessage(stringBuilder.ToString());
		}
	}
}
