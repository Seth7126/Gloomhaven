using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Apparance.Net;

public class Entity : IDisposable
{
	private int m_Handle;

	private uint m_ProcedureID;

	private ParameterCollection m_Parameters = new ParameterCollection();

	private bool m_DynamicDetail;

	[NonSerialized]
	private bool m_bRebuildRequest;

	[NonSerialized]
	private bool m_bIsBuilding;

	private bool m_DebugDrawing;

	private static int m_PrevDataSize = 0;

	private static Dictionary<int, KeyValuePair<int, int>> m_ObjectSet = new Dictionary<int, KeyValuePair<int, int>>();

	private static int m_NextInstanceID = 1;

	private static ReusableBuffer<Vector3> tempPos = new ReusableBuffer<Vector3>(65536);

	private static ReusableBuffer<Vector3> tempNorm = new ReusableBuffer<Vector3>(65536);

	private static ReusableBuffers<uint> tempCol = new ReusableBuffers<uint>(65536);

	private static ReusableBuffers<Vector2> tempUVs = new ReusableBuffers<Vector2>(65536);

	private static ReusableBuffers<int> tempIndices = new ReusableBuffers<int>(65536);

	private static Dictionary<Entity, object> s_EntitySet = new Dictionary<Entity, object>();

	private static List<Entity> s_EntityList = new List<Entity>();

	private static Dictionary<int, Entity> s_EntityLookup = new Dictionary<int, Entity>();

	private static List<Entity> s_DeadEntities = new List<Entity>();

	private static int s_EntityUpdateCursor = 0;

	private static double s_UpdateTimeoutMS = 5.0;

	public uint Procedure
	{
		get
		{
			return m_ProcedureID;
		}
		set
		{
			if (value != m_ProcedureID)
			{
				m_ProcedureID = value;
				m_bRebuildRequest = true;
				m_bIsBuilding = true;
			}
		}
	}

	public ParameterCollection Parameters => m_Parameters;

	public bool DynamicDetail
	{
		get
		{
			return m_DynamicDetail;
		}
		set
		{
			if (value != m_DynamicDetail)
			{
				m_DynamicDetail = value;
				m_bRebuildRequest = true;
				m_bIsBuilding = true;
			}
		}
	}

	public int Handle => m_Handle;

	public bool IsBusy => m_bIsBuilding;

	public IObjectPlacement ObjectPlacementHandler { get; set; }

	public IDebugDrawing DebugDrawingHandler { get; set; }

	public IDebugDisplay DebugDisplayHandler { get; set; }

	public void SetParameters(ParameterCollection p)
	{
		m_Parameters = p;
	}

	public void Refresh()
	{
		m_bRebuildRequest = true;
		m_bIsBuilding = true;
	}

	public Entity(int handle)
	{
		m_Handle = Interop.ApparanceCreateEntity(handle);
		if (!s_EntitySet.ContainsKey(this))
		{
			s_EntitySet.Add(this, null);
			s_EntityList.Add(this);
			s_EntityLookup[m_Handle] = this;
		}
	}

	public void Dispose()
	{
		s_DeadEntities.Add(this);
		Interop.ApparanceDestroyEntity(m_Handle);
		m_Handle = 0;
		ObjectPlacementHandler = null;
		DebugDrawingHandler = null;
		DebugDisplayHandler = null;
	}

	public static void Destroy(int handle)
	{
		Interop.ApparanceDestroyEntity(handle);
	}

	internal void Update(float dt)
	{
		if (m_bRebuildRequest)
		{
			m_bRebuildRequest = false;
			byte[] buffer = null;
			m_Parameters.GetData(out buffer);
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(buffer[0]) * buffer.Length);
			Marshal.Copy(buffer, 0, intPtr, buffer.Length);
			Interop.ApparanceEntityBuild(m_Handle, m_ProcedureID, buffer.Length, intPtr, m_DynamicDetail);
			Marshal.FreeHGlobal(intPtr);
			if (ObjectPlacementHandler != null && ObjectPlacementHandler.IsValid)
			{
				ObjectPlacementHandler.BeginContentUpdate();
			}
		}
	}

	public unsafe static bool CheckTasks()
	{
		while (true)
		{
			int data_size;
			IntPtr data_bytes;
			int num = Interop.ApparancePopEntityTask(out data_size, out data_bytes);
			if (num == 0 || data_size == 0)
			{
				break;
			}
			if (data_size != m_PrevDataSize)
			{
				m_PrevDataSize = data_size;
			}
			Entity value = null;
			if (s_EntityLookup.TryGetValue(num, out value))
			{
				UnmanagedMemoryStream unmanagedMemoryStream = new UnmanagedMemoryStream((byte*)data_bytes.ToPointer(), data_size, data_size, FileAccess.Read);
				BinaryReader binaryReader = new BinaryReader(unmanagedMemoryStream);
				value.HandleTask(binaryReader);
				binaryReader.Close();
				unmanagedMemoryStream.Close();
				return true;
			}
		}
		return false;
	}

	private void HandleTask(BinaryReader reader)
	{
		bool flag = true;
		bool debugDrawing = m_DebugDrawing;
		bool flag2 = false;
		m_DebugDrawing = false;
		while (flag && reader.BaseStream.Position < reader.BaseStream.Length)
		{
			int num = reader.ReadInt32();
			switch (num)
			{
			case 1094795585:
				DecodeAddObject(reader);
				flag2 = true;
				break;
			case 1381126738:
				DecodeRemoveObject(reader);
				break;
			case 1279872581:
				if (!m_DebugDrawing && DebugDrawingHandler != null)
				{
					DebugDrawingHandler.ResetDrawing();
				}
				m_DebugDrawing = true;
				DecodeDrawDebugLine(reader);
				break;
			case 1179795789:
				if (!m_DebugDrawing && DebugDrawingHandler != null)
				{
					DebugDrawingHandler.ResetDrawing();
				}
				m_DebugDrawing = true;
				DecodeDrawDebugBox(reader);
				break;
			case 1397770309:
				if (!m_DebugDrawing && DebugDrawingHandler != null)
				{
					DebugDrawingHandler.ResetDrawing();
				}
				m_DebugDrawing = true;
				DecodeDrawDebugSphere(reader);
				break;
			case 1145328177:
			{
				int key3 = reader.ReadInt32();
				if (DebugDisplayHandler != null && m_ObjectSet.TryGetValue(key3, out var value3))
				{
					for (int k = value3.Key; k < value3.Value; k++)
					{
						DebugDisplayHandler.DisplayControl(k, enable: true);
					}
				}
				break;
			}
			case 1145328176:
			{
				int key5 = reader.ReadInt32();
				if (DebugDisplayHandler != null && m_ObjectSet.TryGetValue(key5, out var value5))
				{
					for (int m = value5.Key; m < value5.Value; m++)
					{
						DebugDisplayHandler.DisplayControl(m, enable: false);
					}
				}
				break;
			}
			case 1397247831:
			{
				int key2 = reader.ReadInt32();
				if (DebugDisplayHandler != null && m_ObjectSet.TryGetValue(key2, out var value2))
				{
					for (int j = value2.Key; j < value2.Value; j++)
					{
						DebugDisplayHandler.DisplayHide(j, hide: false);
					}
				}
				break;
			}
			case 1212761157:
			{
				int key6 = reader.ReadInt32();
				if (DebugDisplayHandler != null && m_ObjectSet.TryGetValue(key6, out var value6))
				{
					for (int n = value6.Key; n < value6.Value; n++)
					{
						DebugDisplayHandler.DisplayHide(n, hide: true);
					}
				}
				break;
			}
			case 1129075249:
			{
				int key4 = reader.ReadInt32();
				Colour c = Colour.Stream(reader);
				if (DebugDisplayHandler != null && m_ObjectSet.TryGetValue(key4, out var value4))
				{
					for (int l = value4.Key; l < value4.Value; l++)
					{
						DebugDisplayHandler.DisplayColour(l, enable: true, c);
					}
				}
				break;
			}
			case 1129075248:
			{
				int key = reader.ReadInt32();
				if (DebugDisplayHandler != null && m_ObjectSet.TryGetValue(key, out var value))
				{
					for (int i = value.Key; i < value.Value; i++)
					{
						DebugDisplayHandler.DisplayColour(i, enable: false, default(Colour));
					}
				}
				break;
			}
			default:
				Engine.LogError("Unknown data code: " + num.ToString("X8"));
				flag = false;
				break;
			}
		}
		if (flag2)
		{
			m_bIsBuilding = false;
			if (ObjectPlacementHandler != null && ObjectPlacementHandler.IsValid)
			{
				ObjectPlacementHandler.EndContentUpdate();
			}
		}
		if (debugDrawing && !m_DebugDrawing && DebugDrawingHandler != null)
		{
			DebugDrawingHandler.ResetDrawing();
		}
	}

	private static void CheckSection(BinaryReader stream, int section, string name)
	{
		if (stream.ReadInt32() != section)
		{
			Engine.LogError($"Expecting {name} section (marker {section}) when decoding new object to place");
		}
	}

	internal void DecodeAddObject(BinaryReader stream)
	{
		if (ObjectPlacementHandler != null && !ObjectPlacementHandler.IsValid)
		{
			ObjectPlacementHandler = null;
		}
		int key = stream.ReadInt32();
		int tier = stream.ReadInt32();
		Vector3 offset = Vector3.Stream(stream);
		CheckSection(stream, 1330597711, "Objects");
		int num = stream.ReadInt32();
		int nextInstanceID = m_NextInstanceID;
		for (int i = 0; i < num; i++)
		{
			bool flag = false;
			int num2 = stream.ReadInt32();
			string text = null;
			if (num2 > 0)
			{
				text = Helpers.ReadString(stream);
				if (string.IsNullOrWhiteSpace(text))
				{
					text = "Group";
				}
			}
			int num3 = stream.ReadInt32();
			Frame frame = default(Frame);
			ParameterCollection parameterCollection = null;
			if (num3 != 0)
			{
				int num4 = stream.ReadInt32();
				if (num4 > 0)
				{
					parameterCollection = new ParameterCollection();
					byte[] data = stream.ReadBytes(num4);
					parameterCollection.SetData(data);
				}
				if (num4 > 0)
				{
					Parameter parameter = parameterCollection.GetParameter(0);
					if (parameter != null && parameter.Type == 'F')
					{
						frame = (Frame)parameter.Value;
					}
				}
				flag = true;
			}
			if (ObjectPlacementHandler != null)
			{
				if (flag)
				{
					ObjectPlacementHandler.CreateObject(m_NextInstanceID + i, tier, offset, num3, frame, parameterCollection, text, num2);
				}
				else
				{
					ObjectPlacementHandler.CreateGroup(m_NextInstanceID + i, tier, text, num2);
				}
			}
		}
		m_NextInstanceID += num;
		CheckSection(stream, 1195853639, "Geometry");
		int num5 = stream.ReadInt32();
		if (num5 > 0)
		{
			CheckSection(stream, 1448498774, "Vertices");
			Vector3[] array = tempPos.Buffer(num5);
			array.Stream(stream, num5);
			CheckSection(stream, 1313754702, "Normals");
			Vector3[] array2 = tempNorm.Buffer(num5);
			array2.Stream(stream, num5);
			CheckSection(stream, 1128481603, "Colours");
			int num6 = stream.ReadInt32();
			List<uint[]> list = new List<uint[]>();
			for (int j = 0; j < num6; j++)
			{
				uint[] array3 = tempCol.Buffer(j, num5);
				array3.Stream(stream, num5);
				list.Add(array3);
			}
			CheckSection(stream, 1431655765, "UVs");
			int num7 = stream.ReadInt32();
			List<Vector2[]> list2 = new List<Vector2[]>();
			for (int k = 0; k < num7; k++)
			{
				Vector2[] array4 = tempUVs.Buffer(k, num5);
				array4.Stream(stream, num5);
				list2.Add(array4);
			}
			CheckSection(stream, 1347440720, "Parts");
			int num8 = stream.ReadInt32();
			MeshPart[] array5 = new MeshPart[num8];
			for (int l = 0; l < num8; l++)
			{
				CheckSection(stream, 1414812756, "Triangles");
				MeshPart meshPart = new MeshPart
				{
					Material = stream.ReadInt32(),
					MinBounds = Vector3.Stream(stream),
					MaxBounds = Vector3.Stream(stream),
					BaseVertex = stream.ReadInt32()
				};
				int num9 = (meshPart.NumIndices = stream.ReadInt32());
				meshPart.Indices = tempIndices.Buffer(l, num9);
				meshPart.Indices.Stream(stream, num9);
				array5[l] = meshPart;
			}
			if (ObjectPlacementHandler != null)
			{
				ObjectPlacementHandler.CreateMesh(m_NextInstanceID, tier, offset, num5, array, array2, list, list2, array5);
				m_NextInstanceID++;
			}
		}
		int nextInstanceID2 = m_NextInstanceID;
		if (nextInstanceID2 > nextInstanceID)
		{
			m_ObjectSet[key] = new KeyValuePair<int, int>(nextInstanceID, nextInstanceID2);
		}
	}

	internal void DecodeRemoveObject(BinaryReader stream)
	{
		if (ObjectPlacementHandler != null && !ObjectPlacementHandler.IsValid)
		{
			ObjectPlacementHandler = null;
		}
		int key = stream.ReadInt32();
		if (m_ObjectSet.TryGetValue(key, out var value) && ObjectPlacementHandler != null)
		{
			for (int i = value.Key; i < value.Value; i++)
			{
				ObjectPlacementHandler.DestroyObject(i);
			}
		}
	}

	internal void DecodeDrawDebugLine(BinaryReader stream)
	{
		Vector3 start = Vector3.Stream(stream);
		Vector3 end = Vector3.Stream(stream);
		Colour colour = Colour.Stream(stream);
		if (DebugDrawingHandler != null)
		{
			DebugDrawingHandler.DrawLine(start, end, colour);
		}
	}

	internal void DecodeDrawDebugBox(BinaryReader stream)
	{
		Frame frame = Frame.Stream(stream);
		Colour colour = Colour.Stream(stream);
		if (DebugDrawingHandler != null)
		{
			DebugDrawingHandler.DrawBox(frame, colour);
		}
	}

	internal void DecodeDrawDebugSphere(BinaryReader stream)
	{
		Vector3 centre = Vector3.Stream(stream);
		float radius = stream.ReadSingle();
		Colour colour = Colour.Stream(stream);
		if (DebugDrawingHandler != null)
		{
			DebugDrawingHandler.DrawSphere(centre, radius, colour);
		}
	}

	internal static void UpdateAll(float dt)
	{
		DateTime utcNow = DateTime.UtcNow;
		bool flag = false;
		while (CheckTasks())
		{
			if ((DateTime.UtcNow - utcNow).TotalMilliseconds > s_UpdateTimeoutMS)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			int count = s_EntityList.Count;
			for (int i = 0; i < count; i++)
			{
				s_EntityUpdateCursor++;
				if (s_EntityUpdateCursor >= count)
				{
					s_EntityUpdateCursor = 0;
				}
				s_EntityList[s_EntityUpdateCursor].Update(dt);
				if ((DateTime.UtcNow - utcNow).TotalMilliseconds > s_UpdateTimeoutMS)
				{
					flag = true;
					break;
				}
			}
		}
		if (s_DeadEntities.Count == 0)
		{
			return;
		}
		foreach (Entity s_DeadEntity in s_DeadEntities)
		{
			s_EntitySet.Remove(s_DeadEntity);
			s_EntityList.Remove(s_DeadEntity);
			s_EntityLookup.Remove(s_DeadEntity.Handle);
		}
		s_DeadEntities.Clear();
	}
}
