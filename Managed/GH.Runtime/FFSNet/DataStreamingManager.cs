using Photon.Bolt;

namespace FFSNet;

[BoltGlobalBehaviour]
public class DataStreamingManager : Singleton<DataStreamingManager>
{
	private ByteChunkifier chunkifier;

	public void CompressAndSendDataInChunks(DataActionType dataActionType, byte[] data, NetworkTargets targets, BoltConnection connection = null)
	{
		switch (targets)
		{
		case NetworkTargets.None:
			Console.LogError("ERROR_MULTIPLAYER_00020", "Network target for sending custom data uninitialized. Returning.");
			return;
		case NetworkTargets.TargetClient:
			if (connection == null)
			{
				Console.LogError("ERROR_MULTIPLAYER_00021", "Trying to send data to null connection. Returning.");
				return;
			}
			break;
		}
		data = Utility.CompressData(data);
		int num = 600;
		ByteChunkifier byteChunkifier = new ByteChunkifier(data, num);
		int num2 = 0;
		Console.LogInfo("Started streaming data to " + ((targets == NetworkTargets.TargetClient) ? connection.ToString() : targets.ToString()));
		byte[] buffer;
		while (byteChunkifier.ReadChunk(out buffer))
		{
			GameDataEvent gameDataEvent = ((targets != NetworkTargets.TargetClient) ? GameDataEvent.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered) : GameDataEvent.Create(connection, ReliabilityModes.ReliableOrdered));
			gameDataEvent.DataActionID = (int)dataActionType;
			gameDataEvent.ChunkSize = num;
			gameDataEvent.ChunkIndex = num2;
			gameDataEvent.TotalSize = data.Length;
			gameDataEvent.Complete = !byteChunkifier.HasBytesLeft();
			gameDataEvent.BinaryData = buffer;
			gameDataEvent.Send();
			Console.LogInfo("Sending Data: " + num2 + ", " + (num2 + 1) * num + " / " + data.Length);
			num2++;
		}
	}

	public byte[] ReassembleData(GameDataEvent evnt)
	{
		if (chunkifier == null)
		{
			chunkifier = new ByteChunkifier(evnt.TotalSize, evnt.ChunkSize);
		}
		chunkifier.WriteChunk(evnt.BinaryData, evnt.ChunkIndex);
		Console.LogInfo("Receiving Data: " + evnt.ChunkIndex + ", " + (evnt.ChunkIndex + 1) * evnt.ChunkSize + " / " + evnt.TotalSize);
		if (evnt.Complete)
		{
			byte[] result = Utility.DecompressData(chunkifier.Data);
			chunkifier = null;
			Console.LogInfo("All data received.");
			return result;
		}
		return null;
	}
}
