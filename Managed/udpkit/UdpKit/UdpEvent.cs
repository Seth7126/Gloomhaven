using System.Reflection;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
public struct UdpEvent
{
	internal const int INTERNAL_START = 1;

	internal const int INTERNAL_CONNECT = 3;

	internal const int INTERNAL_CONNECT_CANCEL = 5;

	internal const int INTERNAL_ACCEPT = 7;

	internal const int INTERNAL_REFUSE = 9;

	internal const int INTERNAL_DISCONNECT = 11;

	internal const int INTERNAL_CLOSE = 13;

	internal const int INTERNAL_SEND = 15;

	internal const int INTERNAL_LANBROADCAST_ENABLE = 17;

	internal const int INTERNAL_LANBROADCAST_DISABLE = 19;

	internal const int INTERNAL_FORGETSESSIONS = 21;

	internal const int INTERNAL_FORGETSESSIONS_ALL = 23;

	internal const int INTERNAL_STREAM_CREATECHANNEL = 25;

	internal const int INTERNAL_STREAM_QUEUE = 27;

	internal const int INTERNAL_STREAM_SETBANDWIDTH = 29;

	internal const int INTERNAL_SESSION_CONNECT = 31;

	internal const int INTERNAL_SESSION_CONNECT_RANDOM = 45;

	internal const int INTERNAL_SESSION_HOST_SETINFO = 33;

	internal const int INTERNAL_MASTERSERVER_CONNECT = 35;

	internal const int INTERNAL_MASTERSERVER_DISCONNECT = 37;

	internal const int INTERNAL_MASTERSERVER_SESSIONLISTREQUEST = 39;

	internal const int INTERNAL_MASTERSERVER_INFOREQUEST = 43;

	internal const int PUBLIC_CONNECT_REQUEST = 2;

	internal const int PUBLIC_CONNECT_FAILED = 4;

	internal const int PUBLIC_CONNECT_REFUSED = 6;

	internal const int PUBLIC_CONNECTED = 8;

	internal const int PUBLIC_CONNECT_ATTEMPT = 10;

	internal const int PUBLIC_DISCONNECTED = 12;

	internal const int PUBLIC_PACKET_LOST = 14;

	internal const int PUBLIC_PACKET_RECEIVED = 16;

	internal const int PUBLIC_PACKET_DELIVERED = 18;

	internal const int PUBLIC_STREAM_DATA_PROGRESS = 44;

	internal const int PUBLIC_STREAM_DATA_STARTED = 42;

	internal const int PUBLIC_STREAM_DATA_RECEIVED = 20;

	internal const int PUBLIC_STREAM_DATA_ABORT = 46;

	internal const int PUBLIC_START_DONE = 22;

	internal const int PUBLIC_START_FAILED = 24;

	internal const int PUBLIC_MASTERSERVER_CONNECTED = 26;

	internal const int PUBLIC_MASTERSERVER_DISCONNECTED = 28;

	internal const int PUBLIC_MASTERSERVER_NATPROBE_RESULT = 32;

	internal const int PUBLIC_MASTERSERVER_CONNECTFAILED = 34;

	internal const int PUBLIC_SESSION_LISTUPDATED = 30;

	internal const int PUBLIC_SESSION_CONNECTFAILED = 36;

	internal const int PUBLIC_SESSION_CONNECTED = 40;

	internal const int PUBLIC_SESSION_CREATED = 38;

	internal int Type;

	internal object Object0;

	internal object Object1;

	internal bool IsInternal => (Type & 1) == 1;

	public UdpEventType EventType => (UdpEventType)Type;

	public T As<T>()
	{
		return (T)Object0;
	}

	public void As<T0, T1>(out T0 a, out T1 b)
	{
		a = (T0)Object0;
		b = (T1)Object1;
	}
}
