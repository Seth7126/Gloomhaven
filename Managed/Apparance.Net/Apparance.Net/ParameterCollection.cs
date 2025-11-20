using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Apparance.Net;

public class ParameterCollection
{
	protected enum State
	{
		Empty,
		Populated,
		Writing,
		Reading,
		Adding,
		Getting
	}

	[Flags]
	private enum SchemaFlags
	{
		None = 0,
		UsingCount = 1,
		UsingID = 2,
		UsingName = 4,
		UsingPeriod = 8
	}

	protected List<Parameter> m_Params;

	private SchemaFlags m_Schema;

	private int m_Period;

	private Dictionary<int, Dictionary<string, object>> m_Metadata;

	protected byte[] m_Data;

	protected State m_State;

	protected bool IsReady
	{
		get
		{
			if (m_State != State.Empty)
			{
				return m_State == State.Populated;
			}
			return true;
		}
	}

	protected bool IsBusy => !IsReady;

	public int Count
	{
		get
		{
			Expand();
			return m_Params.Count;
		}
	}

	public bool UsingNames => (m_Schema & SchemaFlags.UsingName) != 0;

	public bool UsingIdentifiers => (m_Schema & SchemaFlags.UsingID) != 0;

	protected bool IsCompacted => m_Data != null;

	protected bool IsExpanded => m_Params != null;

	internal ParameterCollection()
	{
	}

	public static ParameterCollection CreateEmpty()
	{
		return new ParameterCollection();
	}

	private void Clear()
	{
		ClearExpanded();
		ClearCompacted();
	}

	private void ClearExpanded()
	{
		m_Params = null;
		m_Metadata = null;
	}

	private void ClearCompacted()
	{
		m_Data = null;
	}

	private void StoreMetadata(int id, Dictionary<string, object> metadata)
	{
		if (metadata != null)
		{
			if (m_Metadata == null)
			{
				m_Metadata = new Dictionary<int, Dictionary<string, object>>();
			}
			m_Metadata[id] = metadata;
		}
	}

	public Dictionary<string, object> GetParameterMetadata(int id)
	{
		Dictionary<string, object> value = null;
		if (m_Metadata != null)
		{
			m_Metadata.TryGetValue(id, out value);
		}
		return value;
	}

	public object GetMetadataValue(int id, string name)
	{
		Dictionary<string, object> parameterMetadata = GetParameterMetadata(id);
		object value = null;
		parameterMetadata?.TryGetValue(name, out value);
		return value;
	}

	public void BeginWrite()
	{
		Clear();
		m_State = State.Writing;
		m_Params = new List<Parameter>();
	}

	public void WriteInteger(int integer_value, int id = 0, string name = null, Dictionary<string, object> metadata = null)
	{
		Parameter item = new Parameter
		{
			ID = id,
			Name = name,
			Type = 'i',
			Value = integer_value
		};
		m_Params.Add(item);
		StoreMetadata(id, metadata);
	}

	public void WriteFloat(float float_value, int id = 0, string name = null, Dictionary<string, object> metadata = null)
	{
		Parameter item = new Parameter
		{
			ID = id,
			Name = name,
			Type = 'f',
			Value = float_value
		};
		m_Params.Add(item);
		StoreMetadata(id, metadata);
	}

	public void WriteBool(bool bool_value, int id = 0, string name = null, Dictionary<string, object> metadata = null)
	{
		Parameter item = new Parameter
		{
			ID = id,
			Name = name,
			Type = 'b',
			Value = bool_value
		};
		m_Params.Add(item);
		StoreMetadata(id, metadata);
	}

	public void WriteVector3(Vector3 vector_value, int id = 0, string name = null, Dictionary<string, object> metadata = null)
	{
		Parameter item = new Parameter
		{
			ID = id,
			Name = name,
			Type = '3',
			Value = vector_value
		};
		m_Params.Add(item);
		StoreMetadata(id, metadata);
	}

	public void WriteColour(Colour colour_value, int id = 0, string name = null, Dictionary<string, object> metadata = null)
	{
		Parameter item = new Parameter
		{
			ID = id,
			Name = name,
			Type = 'C',
			Value = colour_value
		};
		m_Params.Add(item);
		StoreMetadata(id, metadata);
	}

	public void WriteString(string string_value, int id = 0, string name = null, Dictionary<string, object> metadata = null)
	{
		Parameter item = new Parameter
		{
			ID = id,
			Name = name,
			Type = '$',
			Value = string_value
		};
		m_Params.Add(item);
		StoreMetadata(id, metadata);
	}

	public void WriteFrame(Frame frame_value, int id = 0, string name = null, Dictionary<string, object> metadata = null)
	{
		Parameter item = new Parameter
		{
			ID = id,
			Name = name,
			Type = 'F',
			Value = frame_value
		};
		m_Params.Add(item);
		StoreMetadata(id, metadata);
	}

	public ParameterCollection WriteListBegin(int id = 0, string name = null, Dictionary<string, object> metadata = null)
	{
		ParameterCollection parameterCollection = new ParameterCollection();
		Parameter item = new Parameter
		{
			ID = id,
			Name = name,
			Type = '[',
			Value = parameterCollection
		};
		m_Params.Add(item);
		parameterCollection.BeginWrite();
		StoreMetadata(id, metadata);
		return parameterCollection;
	}

	public void WriteListEnd()
	{
		ParameterCollection obj = (ParameterCollection)m_Params[m_Params.Count - 1].Value;
		obj.CalculatePeriod();
		obj.EndWriteInternal(is_root: false);
	}

	public void EndWrite()
	{
		EndWriteInternal(is_root: true);
	}

	private void EndWriteInternal(bool is_root)
	{
		if (is_root)
		{
			SetSchema(AccumulateSchema());
		}
		m_State = ((m_Params.Count > 0) ? State.Populated : State.Empty);
	}

	private void CalculatePeriod()
	{
		m_Period = 0;
		int count = m_Params.Count;
		if (count == 1)
		{
			m_Period = 1;
		}
		else if (count > 1)
		{
			int num = 0;
			int num2 = 0;
			int num3 = count - 1;
			while (num2 < num3)
			{
				num2++;
				char type = m_Params[num].Type;
				num = ((m_Params[num2].Type == type) ? (num + 1) : 0);
			}
			m_Period = num2 - num + 1;
		}
	}

	public void GetData(out byte[] buffer)
	{
		Compact();
		buffer = m_Data;
	}

	public void SetData(byte[] buffer)
	{
		Clear();
		m_Data = buffer;
	}

	public void BeginRead()
	{
		throw new NotImplementedException();
	}

	public bool ReadInteger(out int value, int id = 0)
	{
		throw new NotImplementedException();
	}

	public bool ReadFloat(out float value, int id = 0)
	{
		throw new NotImplementedException();
	}

	public bool ReadBool(out bool value, int id = 0)
	{
		throw new NotImplementedException();
	}

	public bool ReadVector3(out Vector3 value, int id = 0)
	{
		throw new NotImplementedException();
	}

	public bool ReadColour(out Colour value, int id = 0)
	{
		throw new NotImplementedException();
	}

	public bool ReadString(out string value, int id = 0)
	{
		throw new NotImplementedException();
	}

	public bool ReadFrame(out Frame value, int id = 0)
	{
		throw new NotImplementedException();
	}

	public ParameterCollection ReadListBegin(int id = 0)
	{
		throw new NotImplementedException();
	}

	public void ReadListEnd()
	{
		throw new NotImplementedException();
	}

	public void EndRead()
	{
		throw new NotImplementedException();
	}

	public Parameter GetParameter(int id)
	{
		Expand();
		for (int i = 0; i < m_Params.Count; i++)
		{
			if (m_Params[i].ID == id)
			{
				return m_Params[i];
			}
		}
		return null;
	}

	public Parameter GetParameter(string name)
	{
		int num = name.IndexOf('(');
		if (num >= 0)
		{
			name = name.Substring(num).Trim();
		}
		Expand();
		for (int i = 0; i < m_Params.Count; i++)
		{
			if (m_Params[i].Name == name)
			{
				return m_Params[i];
			}
		}
		return null;
	}

	public Parameter GetParameterAt(int index)
	{
		Expand();
		if (index < m_Params.Count)
		{
			return m_Params[index];
		}
		return null;
	}

	public void BeginSet()
	{
		ExpandForEdit();
	}

	public void EndSet(bool edited)
	{
		if (edited)
		{
			SetSchema(AccumulateSchema());
		}
	}

	public void SetParameter(int id, object value)
	{
		if (id == 0)
		{
			throw new ArgumentOutOfRangeException("Parameter can't be set with invalid ID 0");
		}
		Parameter parameter = null;
		for (int i = 0; i < m_Params.Count; i++)
		{
			Parameter parameter2 = m_Params[i];
			if (parameter2 != null && parameter2.ID == id)
			{
				parameter = parameter2;
				break;
			}
		}
		if (parameter == null)
		{
			char c = Helpers.cTypeIDFromObjectType(value);
			if (c == '\0')
			{
				throw new ArgumentException("Can't set parameter of unknown type: " + ((value != null) ? value.GetType().Name : "<null>"));
			}
			parameter = new Parameter
			{
				ID = id,
				Type = c,
				Value = value
			};
			m_Params.Add(parameter);
		}
		else
		{
			parameter.Set(value);
		}
	}

	public void SetParameter(string name, object value)
	{
		int num = name.IndexOf('(');
		if (num >= 0)
		{
			name = name.Substring(num).Trim();
		}
		Parameter parameter = null;
		for (int i = 0; i < m_Params.Count; i++)
		{
			Parameter parameter2 = m_Params[i];
			if (parameter2 != null && parameter2.Name == name)
			{
				parameter = parameter2;
				break;
			}
		}
		if (parameter == null)
		{
			char c = Helpers.cTypeIDFromObjectType(value);
			if (c == '\0')
			{
				throw new ArgumentException("Can't set parameter of unknown type " + value.GetType().Name);
			}
			parameter = new Parameter
			{
				Name = name,
				Type = c,
				Value = value
			};
			m_Params.Add(parameter);
		}
		else
		{
			parameter.Set(value);
		}
	}

	public void SetParameterAt(int index, object value)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("Can't set parameter at negative index " + index);
		}
		while (index >= m_Params.Count)
		{
			m_Params.Add(null);
		}
		Parameter parameter = m_Params[index];
		if (parameter == null)
		{
			char c = Helpers.cTypeIDFromObjectType(value);
			if (c == '\0')
			{
				throw new ArgumentException("Can't set parameter of unknown type " + value.GetType().Name);
			}
			parameter = new Parameter
			{
				Type = c,
				Value = value
			};
			m_Params[index] = parameter;
		}
		else
		{
			parameter.Set(value);
		}
	}

	private void Expand()
	{
		if (!IsExpanded)
		{
			if (m_Data != null && m_Data.Length != 0)
			{
				BinaryReader binaryReader = new BinaryReader(new MemoryStream(m_Data));
				SchemaFlags schemaFlags = (SchemaFlags)binaryReader.ReadByte();
				Read(binaryReader, schemaFlags);
				SetSchema(schemaFlags);
			}
			else
			{
				m_Params = new List<Parameter>();
			}
		}
	}

	private void ExpandForEdit()
	{
		Expand();
		ClearCompacted();
	}

	private void Compact()
	{
		if (!IsCompacted)
		{
			SetSchema(AccumulateSchema());
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write((byte)m_Schema);
			Write(binaryWriter, m_Schema);
			m_Data = memoryStream.ToArray();
		}
	}

	private SchemaFlags AccumulateSchema()
	{
		SchemaFlags schemaFlags = SchemaFlags.None;
		if (m_Params != null && m_Params.Count > 0)
		{
			schemaFlags |= SchemaFlags.UsingCount;
			foreach (Parameter item in m_Params)
			{
				if (item.ID != 0)
				{
					schemaFlags |= SchemaFlags.UsingID;
				}
				if (item.Name != null)
				{
					schemaFlags |= SchemaFlags.UsingName;
				}
				if (item.Type == '[')
				{
					schemaFlags |= ((ParameterCollection)item.Value).AccumulateSchema();
				}
			}
			if (m_Period > 0)
			{
				schemaFlags |= SchemaFlags.UsingPeriod;
			}
		}
		return schemaFlags;
	}

	private void SetSchema(SchemaFlags schema)
	{
		m_Schema = schema;
		if (m_Params == null || m_Params.Count <= 0)
		{
			return;
		}
		foreach (Parameter item in m_Params)
		{
			if (item.Type == '[')
			{
				((ParameterCollection)item.Value).SetSchema(schema);
			}
		}
	}

	private void Write(BinaryWriter w, SchemaFlags schema_flags)
	{
		if ((schema_flags & SchemaFlags.UsingCount) == 0)
		{
			return;
		}
		w.Write((m_Params != null) ? m_Params.Count : 0);
		if ((schema_flags & SchemaFlags.UsingPeriod) != SchemaFlags.None)
		{
			w.Write(m_Period);
		}
		if (m_Params == null)
		{
			return;
		}
		foreach (Parameter item in m_Params)
		{
			if ((schema_flags & SchemaFlags.UsingID) != SchemaFlags.None)
			{
				w.Write(item.ID);
			}
			if ((schema_flags & SchemaFlags.UsingName) != SchemaFlags.None)
			{
				w.Write((item.Name != null) ? item.Name : "");
			}
			w.Write(item.Type);
			switch (item.Type)
			{
			case 'i':
				w.Write((int)item.Value);
				break;
			case 'f':
				w.WriteNoAlloc((float)item.Value);
				break;
			case 'b':
				w.Write(((bool)item.Value) ? 'T' : 'F');
				break;
			case '3':
				WriteVector3(w, (Vector3)item.Value);
				break;
			case 'C':
			{
				Colour colour = (Colour)item.Value;
				w.WriteNoAlloc(colour.R);
				w.WriteNoAlloc(colour.G);
				w.WriteNoAlloc(colour.B);
				w.WriteNoAlloc(colour.A);
				break;
			}
			case '$':
			{
				byte[] bytes = Encoding.UTF8.GetBytes((string)item.Value);
				w.Write(bytes.Length);
				w.Write(bytes);
				break;
			}
			case 'F':
			{
				Frame frame = (Frame)item.Value;
				WriteVector3(w, frame.AxisX);
				WriteVector3(w, frame.AxisY);
				WriteVector3(w, frame.AxisZ);
				WriteVector3(w, frame.Origin);
				WriteVector3(w, frame.Size);
				break;
			}
			case '[':
				((ParameterCollection)item.Value).Write(w, schema_flags);
				break;
			default:
				throw new NotSupportedException("Unsupported type code '" + item.Type + "'");
			}
		}
	}

	private void WriteVector3(BinaryWriter w, Vector3 value)
	{
		w.WriteNoAlloc(value.X);
		w.WriteNoAlloc(value.Y);
		w.WriteNoAlloc(value.Z);
	}

	private Vector3 ReadVector3(BinaryReader r)
	{
		return new Vector3(r.ReadSingleNoAlloc(), r.ReadSingleNoAlloc(), r.ReadSingleNoAlloc());
	}

	private string ReadString(BinaryReader r)
	{
		int count = r.ReadInt32();
		byte[] bytes = r.ReadBytes(count);
		return Encoding.UTF8.GetString(bytes);
	}

	private void WriteString(BinaryWriter w, string value)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		w.Write(bytes.Length);
		for (int i = 0; i < bytes.Length; i++)
		{
			w.Write(bytes[i]);
		}
	}

	private void Read(BinaryReader r, SchemaFlags schema_flags)
	{
		if ((schema_flags & SchemaFlags.UsingCount) != SchemaFlags.None)
		{
			int num = r.ReadInt32();
			m_Params = new List<Parameter>(num);
			if ((schema_flags & SchemaFlags.UsingPeriod) != SchemaFlags.None)
			{
				m_Period = r.ReadInt32();
			}
			for (int i = 0; i < num; i++)
			{
				Parameter parameter = new Parameter();
				m_Params.Add(parameter);
				if ((schema_flags & SchemaFlags.UsingID) != SchemaFlags.None)
				{
					parameter.ID = r.ReadInt32();
				}
				if ((schema_flags & SchemaFlags.UsingName) != SchemaFlags.None)
				{
					parameter.Name = r.ReadString();
				}
				parameter.Type = r.ReadChar();
				switch (parameter.Type)
				{
				case 'i':
					parameter.Value = r.ReadInt32();
					break;
				case 'f':
					parameter.Value = r.ReadSingleNoAlloc();
					break;
				case 'b':
					parameter.Value = r.ReadChar() != 'F';
					break;
				case '3':
					parameter.Value = ReadVector3(r);
					break;
				case 'C':
					parameter.Value = new Colour(r.ReadSingleNoAlloc(), r.ReadSingleNoAlloc(), r.ReadSingleNoAlloc(), r.ReadSingleNoAlloc());
					break;
				case '$':
					parameter.Value = ReadString(r);
					break;
				case 'F':
					parameter.Value = new Frame
					{
						AxisX = ReadVector3(r),
						AxisY = ReadVector3(r),
						AxisZ = ReadVector3(r),
						Origin = ReadVector3(r),
						Size = ReadVector3(r)
					};
					break;
				case '[':
				{
					ParameterCollection parameterCollection = new ParameterCollection();
					parameterCollection.Read(r, schema_flags);
					parameter.Value = parameterCollection;
					break;
				}
				default:
					throw new NotSupportedException("Unsupported type code '" + parameter.Type + "'");
				}
			}
		}
		else
		{
			m_Params = new List<Parameter>(0);
		}
	}
}
