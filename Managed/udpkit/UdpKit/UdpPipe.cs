#define DEBUG
namespace UdpKit;

internal class UdpPipe
{
	private struct SendInfo
	{
		public uint Sequence;

		public UdpRingBuffer<UdpPipeHandle> Window;
	}

	private struct RecvInfo
	{
		public int PendingAcks;

		public UdpRingBuffer<Ack> History;
	}

	private struct Ack
	{
		public readonly uint Time;

		public readonly bool Received;

		public readonly uint Sequence;

		private Ack(uint sequence, bool received, uint time)
		{
			Time = time;
			Sequence = sequence;
			Received = received;
		}

		internal static Ack Lost(uint sequence)
		{
			return new Ack(sequence, received: false, 0u);
		}

		internal static Ack Recv(uint sequence, uint time)
		{
			return new Ack(sequence, received: true, time);
		}
	}

	public const int PIPE_COMMAND = 1;

	public const int PIPE_PACKET = 3;

	public const int PIPE_STREAM = 4;

	public const int PIPE_STREAM_UNRELIABLE = 5;

	public const int PIPE_MASTERSERVER = 2;

	public const int PIPE_UNCONNECTED = 0;

	public UdpPipeConfig Config;

	public UdpConnection Connection;

	private SendInfo Send;

	private RecvInfo Recv;

	public UdpSocket Socket => Connection.Socket;

	public byte Id => Config.PipeId;

	public float FillRatio => Send.Window.FillRatio;

	public uint LastSend => Send.Window.LastOrDefault.Time;

	public UdpPipe(UdpConnection connection, UdpPipeConfig config)
	{
		Config = config;
		Connection = connection;
		Send.Window = new UdpRingBuffer<UdpPipeHandle>(Config.WindowSize)
		{
			AutoFree = false
		};
		Recv.History = new UdpRingBuffer<Ack>(Config.WindowSize)
		{
			AutoFree = true
		};
		UdpAssert.Assert(UdpMath.IsMultipleOf8(Config.HeaderSizeBits));
	}

	public void CheckTimeouts(uint now)
	{
		if (Config.Timeout != 0)
		{
			uint num = (uint)(Connection.NetworkPing * 1250f) + Config.Timeout;
			uint num2 = (uint)(Connection.NetworkPing * 1250f) + Config.Timeout / 2;
			while (!Send.Window.Empty && Send.Window.First.Time + num < now)
			{
				Lost(Send.Window.Dequeue());
			}
			if (Recv.PendingAcks > 0 && Send.Window.FirstOrDefault.Time + num2 < now)
			{
				SendAckPacket();
			}
		}
		else if (Recv.PendingAcks > Config.WindowSize / 2)
		{
			SendAckPacket();
		}
	}

	public void Disconnected()
	{
		while (!Send.Window.Empty)
		{
			Lost(Send.Window.Dequeue());
		}
	}

	public bool WriteHeader(byte[] buffer, object obj)
	{
		int offset = 0;
		if (Send.Window.Full)
		{
			return false;
		}
		UdpPipeHandle item = default(UdpPipeHandle);
		item.Time = Socket.GetCurrentTime();
		item.Sequence = (Send.Sequence = Config.NextSequence(Send.Sequence));
		item.Object = obj;
		Blit.PackByte(buffer, ref offset, Config.PipeId);
		Blit.PackU32(buffer, ref offset, item.Sequence, Config.SequenceBytes);
		if (Recv.History.Count > 0)
		{
			UdpAssert.Assert(Recv.History.Last.Received);
			uint value = UdpMath.Clamp(Socket.GetCurrentTime() - Recv.History.Last.Time, 0u, 65535u);
			Blit.PackU32(buffer, ref offset, value, 2);
			Blit.PackU32(buffer, ref offset, Recv.History.Last.Sequence, Config.SequenceBytes);
		}
		else
		{
			UdpAssert.Assert(!Recv.History.LastOrDefault.Received);
			Blit.PackU32(buffer, ref offset, 0u, 2);
			Blit.PackU32(buffer, ref offset, 0u, Config.SequenceBytes);
		}
		for (int i = 0; i < Config.AckBits; i++)
		{
			if (i < Recv.History.Count)
			{
				int index = Recv.History.Count - 1 - i;
				if (Recv.History[index].Received)
				{
					int num = offset + (i >> 3);
					byte b = (byte)(1 << i % 8);
					UdpAssert.Assert((buffer[num] & b) == 0);
					buffer[num] |= b;
				}
			}
		}
		Send.Window.Enqueue(item);
		Connection.UpdateSendTime();
		Recv.PendingAcks = 0;
		UdpAssert.Assert(offset * 8 + Config.AckBits == Config.HeaderSizeBits);
		return true;
	}

	public bool ReadHeader(byte[] buffer, int size)
	{
		if (size < Config.HeaderSize)
		{
			return false;
		}
		UdpAssert.Assert(buffer[0] == Config.PipeId);
		int offset = 1;
		uint num = Blit.ReadU32(buffer, ref offset, Config.SequenceBytes);
		int num2 = Config.Distance(num, Recv.History.LastOrDefault.Sequence);
		if (num2 > Config.WindowSize || num2 < -Config.WindowSize)
		{
			Connection.ConnectionError(UdpConnectionError.SequenceOutOfBounds);
			return false;
		}
		if (num2 <= 0)
		{
			return false;
		}
		for (int i = 1; i < num2; i++)
		{
			uint sequence = Config.NextSequence(Recv.History.LastOrDefault.Sequence);
			Recv.History.Enqueue(Ack.Lost(sequence));
		}
		Recv.History.Enqueue(Ack.Recv(num, Socket.GetCurrentTime()));
		Recv.PendingAcks++;
		return ReadAcks(buffer, offset);
	}

	private void SendAckPacket()
	{
		byte[] sendBuffer = Socket.GetSendBuffer();
		if (WriteHeader(sendBuffer, null))
		{
			Socket.Send(Connection.RemoteEndPoint, sendBuffer, Config.HeaderSize);
		}
	}

	private bool ReadAcks(byte[] buffer, int o)
	{
		uint num = Blit.ReadU32(buffer, ref o, 2);
		uint to = Blit.ReadU32(buffer, ref o, Config.SequenceBytes);
		while (!Send.Window.Empty)
		{
			int num2 = Config.Distance(Send.Window.First.Sequence, to);
			if (num2 > 0)
			{
				break;
			}
			UdpPipeHandle handle = Send.Window.Dequeue();
			if (num2 <= -Config.AckBits)
			{
				Lost(handle);
			}
			else
			{
				int num3 = o + -num2 / 8;
				int num4 = 1 << -num2 % 8;
				if ((buffer[num3] & num4) == num4)
				{
					Delivered(handle);
				}
				else
				{
					Lost(handle);
				}
			}
			if (Config.UpdatePing && num2 == 0 && num != 0)
			{
				Connection.UpdatePing(Connection.Socket.GetCurrentTime(), handle.Time, num);
			}
		}
		UdpAssert.Assert(o * 8 + Config.AckBits == Config.HeaderSizeBits);
		return true;
	}

	private void Lost(UdpPipeHandle handle)
	{
		Connection.Lost(this, handle.Object);
	}

	private void Delivered(UdpPipeHandle handle)
	{
		Connection.Delivered(this, handle.Object);
	}
}
