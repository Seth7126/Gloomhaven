using Photon.Bolt.Collections;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

[Documentation]
public abstract class Command : NetworkObj_Root, IBoltListNode<Command>
{
	internal const int SEQ_BITS = 8;

	internal const int SEQ_SHIFT = 8;

	internal const int SEQ_MASK = 255;

	private NetworkStorage storage;

	internal new Command_Meta Meta;

	internal int SmoothFrameFrom;

	internal int SmoothFrameTo;

	internal NetworkStorage SmoothStorageFrom;

	internal NetworkStorage SmoothStorageTo;

	internal ushort Sequence;

	internal CommandFlags Flags;

	private bool shouldDeltaCompressInput = false;

	private bool shouldDeltaCompressResult = false;

	internal override NetworkStorage Storage => storage;

	internal NetworkCommand_Data InputObject => (NetworkCommand_Data)base.Objects[1];

	internal NetworkCommand_Data ResultObject => (NetworkCommand_Data)base.Objects[2];

	internal bool AssignedInputDeltaCompression { get; private set; }

	internal bool AssignedResultDeltaCompression { get; private set; }

	internal bool ShouldDeltaCompressInput
	{
		get
		{
			return shouldDeltaCompressInput;
		}
		set
		{
			shouldDeltaCompressInput = value;
			AssignedInputDeltaCompression = true;
		}
	}

	internal bool ShouldDeltaCompressResult
	{
		get
		{
			return shouldDeltaCompressResult;
		}
		set
		{
			shouldDeltaCompressResult = value;
			AssignedResultDeltaCompression = true;
		}
	}

	public int ServerFrame { get; internal set; }

	public bool IsFirstExecution => !(Flags & CommandFlags.HAS_EXECUTED);

	public object UserToken { get; set; }

	Command IBoltListNode<Command>.prev { get; set; }

	Command IBoltListNode<Command>.next { get; set; }

	object IBoltListNode<Command>.list { get; set; }

	internal Command(Command_Meta meta)
		: base(meta)
	{
		Meta = meta;
	}

	internal void InitNetworkStorage()
	{
		storage = AllocateStorage();
	}

	internal void VerifyCanSetInput()
	{
		if ((bool)(Flags & CommandFlags.HAS_EXECUTED))
		{
			throw new BoltException("You can not change the Data of a command after it has executed");
		}
	}

	internal void VerifyCanSetResult()
	{
		if ((bool)(Flags & CommandFlags.CORRECTION_RECEIVED))
		{
			throw new BoltException("You can not change the Data of a command after it has been corrected");
		}
	}

	internal void PackInput(BoltConnection connection, UdpPacket packet)
	{
		NetworkCommand_Data inputObject = InputObject;
		NetworkStorage networkStorage = Storage;
		for (int i = 0; i < inputObject.Meta.Properties.Length; i++)
		{
			inputObject.Meta.Properties[i].Property.Write(connection, inputObject, networkStorage, packet);
		}
	}

	internal void PackInputDiff(BoltConnection connection, UdpPacket packet, NetworkStorage other)
	{
		PackDiff(connection, packet, InputObject, Storage, other);
	}

	internal void ReadInput(BoltConnection connection, UdpPacket packet)
	{
		NetworkCommand_Data inputObject = InputObject;
		NetworkStorage networkStorage = Storage;
		for (int i = 0; i < inputObject.Meta.Properties.Length; i++)
		{
			inputObject.Meta.Properties[i].Property.Read(connection, inputObject, networkStorage, packet);
		}
	}

	internal void ReadInputDiff(BoltConnection connection, UdpPacket packet, NetworkStorage other)
	{
		ReadDiff(connection, packet, InputObject, Storage, null, other);
	}

	internal void PackResult(BoltConnection connection, UdpPacket packet)
	{
		NetworkCommand_Data resultObject = ResultObject;
		NetworkStorage networkStorage = Storage;
		for (int i = 0; i < resultObject.Meta.Properties.Length; i++)
		{
			resultObject.Meta.Properties[i].Property.Write(connection, resultObject, networkStorage, packet);
		}
	}

	internal void PackResultDiff(BoltConnection connection, UdpPacket packet, NetworkStorage other)
	{
		PackDiff(connection, packet, ResultObject, Storage, other);
	}

	internal void ReadResult(BoltConnection connection, UdpPacket packet)
	{
		NetworkCommand_Data resultObject = ResultObject;
		NetworkStorage networkStorage = Storage;
		for (int i = 0; i < resultObject.Meta.Properties.Length; i++)
		{
			NetworkStorage networkStorage2 = ((SmoothStorageTo == null || !resultObject.Meta.Properties[i].Property.Interpolation.Enabled) ? networkStorage : SmoothStorageTo);
			resultObject.Meta.Properties[i].Property.Read(connection, resultObject, networkStorage2, packet);
		}
	}

	internal void ReadResultDiff(BoltConnection connection, UdpPacket packet, NetworkStorage other)
	{
		ReadDiff(connection, packet, ResultObject, Storage, SmoothStorageTo, other);
	}

	internal int GetDiffDistance(NetworkCommand_Data data, NetworkStorage other)
	{
		int num = 0;
		for (int i = 0; i < data.Meta.Properties.Length; i++)
		{
			NetworkProperty property = data.Meta.Properties[i].Property;
			if (property.SupportsDeltaCompression() && !property.Equals(data, Storage, other))
			{
				num += property.BitCount(data);
			}
		}
		return num;
	}

	internal void BeginSmoothing()
	{
		SmoothStorageFrom = DuplicateStorage(Storage);
		SmoothStorageTo = DuplicateStorage(Storage);
		SmoothFrameFrom = BoltCore.frame;
		SmoothFrameTo = SmoothFrameFrom + Meta.SmoothFrames;
	}

	internal void SmoothCorrection()
	{
		if (SmoothStorageFrom == null || SmoothStorageTo == null)
		{
			return;
		}
		float num = SmoothFrameTo - SmoothFrameFrom;
		float num2 = BoltCore.frame - SmoothFrameFrom;
		float t = Mathf.Clamp01(num2 / num);
		NetworkCommand_Data resultObject = ResultObject;
		NetworkStorage networkStorage = Storage;
		for (int i = 0; i < resultObject.Meta.Properties.Length; i++)
		{
			if (resultObject.Meta.Properties[i].Property.Interpolation.Enabled)
			{
				resultObject.Meta.Properties[i].Property.SmoothCommandCorrection(resultObject, SmoothStorageFrom, SmoothStorageTo, networkStorage, t);
			}
		}
	}

	internal void Free()
	{
		FreeStorage(storage);
		if (SmoothStorageTo != null)
		{
			FreeStorage(SmoothStorageTo);
		}
		if (SmoothStorageFrom != null)
		{
			FreeStorage(SmoothStorageFrom);
		}
		SmoothStorageFrom = null;
		SmoothStorageTo = null;
		Flags = CommandFlags.ZERO;
		Sequence = 0;
		SmoothFrameFrom = 0;
		SmoothFrameTo = 0;
		InputObject.Token.Release();
		ResultObject.Token.Release();
		InputObject.Token = null;
		ResultObject.Token = null;
		(Meta as IFactory)?.Return(this);
	}

	private static void ReadDiff(BoltConnection connection, UdpPacket packet, NetworkCommand_Data data, NetworkStorage storage, NetworkStorage smoothStorage, NetworkStorage other)
	{
		for (int i = 0; i < data.Meta.Properties.Length; i++)
		{
			NetworkProperty property = data.Meta.Properties[i].Property;
			NetworkStorage networkStorage = ((smoothStorage == null || !property.Interpolation.Enabled) ? storage : smoothStorage);
			if (!property.SupportsDeltaCompression())
			{
				property.Read(connection, data, networkStorage, packet);
			}
			else if (packet.ReadBool())
			{
				if (other != null)
				{
					property.Read(connection, data, networkStorage, other);
				}
			}
			else
			{
				property.Read(connection, data, networkStorage, packet);
			}
		}
	}

	private static void PackDiff(BoltConnection connection, UdpPacket packet, NetworkCommand_Data data, NetworkStorage storage, NetworkStorage other)
	{
		for (int i = 0; i < data.Meta.Properties.Length; i++)
		{
			NetworkProperty property = data.Meta.Properties[i].Property;
			if (!property.SupportsDeltaCompression())
			{
				property.Write(connection, data, storage, packet);
				continue;
			}
			if (property.Equals(data, storage, other))
			{
				packet.WriteBool(value: true);
				continue;
			}
			packet.WriteBool(value: false);
			property.Write(connection, data, storage, packet);
		}
	}

	public static implicit operator bool(Command command)
	{
		return command != null;
	}
}
