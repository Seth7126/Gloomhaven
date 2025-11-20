using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class Ranking
{
	[StructLayout(LayoutKind.Sequential)]
	public class SetScoreRequest : RequestBase
	{
		public const int NP_SCORE_COMMENT_MAXLEN = 63;

		public const int NP_SCORE_GAMEINFO_MAXSIZE = 189;

		[MarshalAs(UnmanagedType.I8)]
		internal long score;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		internal string utf8Comment;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 189)]
		internal byte[] gameInfoData;

		[MarshalAs(UnmanagedType.U8)]
		internal ulong dataLength;

		[MarshalAs(UnmanagedType.U4)]
		internal uint boardId;

		[MarshalAs(UnmanagedType.I4)]
		internal int pcId;

		public long Score
		{
			get
			{
				return score;
			}
			set
			{
				score = value;
			}
		}

		public string Comment
		{
			get
			{
				return utf8Comment;
			}
			set
			{
				if (value.Length > 63)
				{
					throw new NpToolkitException("The size of the comment string is more than " + 63 + " characters.");
				}
				utf8Comment = value;
			}
		}

		public byte[] GameInfoData
		{
			get
			{
				if (dataLength == 0)
				{
					return null;
				}
				byte[] array = new byte[dataLength];
				Array.Copy(gameInfoData, array, (int)dataLength);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 189)
					{
						throw new NpToolkitException("The size of the game data is more than " + 189 + " bytes.");
					}
					value.CopyTo(gameInfoData, 0);
					dataLength = (uint)value.Length;
				}
				else
				{
					dataLength = 0uL;
				}
			}
		}

		public uint BoardId
		{
			get
			{
				return boardId;
			}
			set
			{
				if (value > 1000)
				{
					throw new NpToolkitException("The BoardId can't be more than " + 1000);
				}
				boardId = value;
			}
		}

		public int PcId
		{
			get
			{
				return pcId;
			}
			set
			{
				pcId = value;
			}
		}

		public SetScoreRequest()
			: base(ServiceTypes.Ranking, FunctionTypes.RankingSetScore)
		{
			gameInfoData = new byte[189];
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetRangeOfRanksRequest : RequestBase
	{
		[MarshalAs(UnmanagedType.U4)]
		internal uint boardId;

		[MarshalAs(UnmanagedType.U4)]
		internal uint startRank;

		[MarshalAs(UnmanagedType.U4)]
		internal uint range;

		[MarshalAs(UnmanagedType.I1)]
		internal bool isCrossSaveInformation;

		public uint BoardId
		{
			get
			{
				return boardId;
			}
			set
			{
				if (value > 1000)
				{
					throw new NpToolkitException("The BoardId can't be more than " + 1000);
				}
				boardId = value;
			}
		}

		public uint StartRank
		{
			get
			{
				return startRank;
			}
			set
			{
				if (value < 1)
				{
					throw new NpToolkitException("The StartRank can't be less than " + 1);
				}
				startRank = value;
			}
		}

		public uint Range
		{
			get
			{
				return range;
			}
			set
			{
				if (value < 1 || value > 100)
				{
					throw new NpToolkitException("The Range must be between " + 1 + " and " + 100);
				}
				range = value;
			}
		}

		public bool IsCrossSaveInformation
		{
			get
			{
				return isCrossSaveInformation;
			}
			set
			{
				isCrossSaveInformation = value;
			}
		}

		public GetRangeOfRanksRequest()
			: base(ServiceTypes.Ranking, FunctionTypes.RankingGetRangeOfRanks)
		{
			IsCrossSaveInformation = false;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetFriendsRanksRequest : RequestBase
	{
		[MarshalAs(UnmanagedType.U4)]
		internal uint boardId;

		[MarshalAs(UnmanagedType.U4)]
		internal uint startRank;

		[MarshalAs(UnmanagedType.U4)]
		internal uint friendsWithPcId;

		[MarshalAs(UnmanagedType.I1)]
		internal bool isCrossSaveInformation;

		[MarshalAs(UnmanagedType.I1)]
		internal bool addCallingUserRank;

		public uint BoardId
		{
			get
			{
				return boardId;
			}
			set
			{
				boardId = value;
			}
		}

		public uint StartRank
		{
			get
			{
				return startRank;
			}
			set
			{
				startRank = value;
			}
		}

		public uint FriendsWithPcId
		{
			get
			{
				return friendsWithPcId;
			}
			set
			{
				friendsWithPcId = value;
			}
		}

		public bool IsCrossSaveInformation
		{
			get
			{
				return isCrossSaveInformation;
			}
			set
			{
				isCrossSaveInformation = value;
			}
		}

		public bool AddCallingUserRank
		{
			get
			{
				return addCallingUserRank;
			}
			set
			{
				addCallingUserRank = value;
			}
		}

		public GetFriendsRanksRequest()
			: base(ServiceTypes.Ranking, FunctionTypes.RankingGetFriendsRanks)
		{
			IsCrossSaveInformation = false;
			addCallingUserRank = true;
		}
	}

	public struct ScoreAccountIdPcId
	{
		public Core.NpAccountId accountId;

		public int pcId;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetUsersRanksRequest : RequestBase
	{
		public const int MAX_NUM_USERS = 101;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 101)]
		internal ScoreAccountIdPcId[] users;

		[MarshalAs(UnmanagedType.U4)]
		internal uint numUsers;

		[MarshalAs(UnmanagedType.U4)]
		internal uint boardId;

		[MarshalAs(UnmanagedType.I1)]
		internal bool isCrossSaveInformation;

		[MarshalAs(UnmanagedType.I1)]
		internal bool ignorePcIds;

		public ScoreAccountIdPcId[] Users
		{
			get
			{
				if (numUsers == 0)
				{
					return null;
				}
				ScoreAccountIdPcId[] array = new ScoreAccountIdPcId[numUsers];
				Array.Copy(users, array, numUsers);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 101)
					{
						throw new NpToolkitException("The size of the Users array is more than " + 101);
					}
					if (!IgnorePcIds)
					{
						for (int i = 0; i < value.Length; i++)
						{
							if (value[i].pcId < 0 || value[i].pcId > 9)
							{
								throw new NpToolkitException("The pcId in Users[" + i + "] is outside the range of MIN_PCID/MAX_PCID");
							}
						}
					}
					value.CopyTo(users, 0);
					numUsers = (uint)value.Length;
				}
				else
				{
					numUsers = 0u;
				}
			}
		}

		public uint BoardId
		{
			get
			{
				return boardId;
			}
			set
			{
				boardId = value;
			}
		}

		public bool IsCrossSaveInformation
		{
			get
			{
				return isCrossSaveInformation;
			}
			set
			{
				isCrossSaveInformation = value;
			}
		}

		public bool IgnorePcIds
		{
			get
			{
				return ignorePcIds;
			}
			set
			{
				ignorePcIds = value;
			}
		}

		public GetUsersRanksRequest()
			: base(ServiceTypes.Ranking, FunctionTypes.RankingGetUsersRanks)
		{
			users = new ScoreAccountIdPcId[101];
			IsCrossSaveInformation = false;
			ignorePcIds = true;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SetGameDataRequest : RequestBase
	{
		internal uint boardId;

		internal int idOfPrevChunk;

		internal long score;

		internal ulong totalSize;

		[MarshalAs(UnmanagedType.LPArray)]
		internal byte[] data;

		internal ulong byteOffset;

		internal ulong chunkDataSize;

		internal int pcId;

		public uint BoardId
		{
			get
			{
				return boardId;
			}
			set
			{
				boardId = value;
			}
		}

		public int IdOfPrevChunk
		{
			get
			{
				return idOfPrevChunk;
			}
			set
			{
				idOfPrevChunk = value;
			}
		}

		public long Score
		{
			get
			{
				return score;
			}
			set
			{
				score = value;
			}
		}

		public ulong TotalSize => totalSize;

		public byte[] Data => data;

		public ulong StartIndex => byteOffset;

		public int PcId
		{
			get
			{
				return pcId;
			}
			set
			{
				pcId = value;
			}
		}

		public void SetDataChunk(byte[] data, ulong startIndex, ulong chunkSize)
		{
			SetDataChunk(data, startIndex, chunkSize, 0uL);
		}

		public void SetDataChunk(byte[] data, ulong startIndex, ulong chunkSize, ulong totalSize)
		{
			if (startIndex + chunkSize > (ulong)data.Length)
			{
				throw new NpToolkitException("The start Index and chunk size go off the end of the data array.");
			}
			this.data = data;
			byteOffset = startIndex;
			chunkDataSize = chunkSize;
			if (totalSize == 0)
			{
				this.totalSize = (ulong)data.Length;
			}
			else
			{
				this.totalSize = totalSize;
			}
		}

		public SetGameDataRequest()
			: base(ServiceTypes.Ranking, FunctionTypes.RankingSetGameData)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetGameDataRequest : RequestBase
	{
		internal uint boardId;

		internal int idOfPrevChunk;

		internal Core.NpAccountId accountId;

		[MarshalAs(UnmanagedType.LPArray)]
		internal byte[] rcvData;

		internal ulong byteOffset;

		internal ulong chunkToRcvDataSize;

		internal int pcId;

		public uint BoardId
		{
			get
			{
				return boardId;
			}
			set
			{
				boardId = value;
			}
		}

		public int IdOfPrevChunk
		{
			get
			{
				return idOfPrevChunk;
			}
			set
			{
				idOfPrevChunk = value;
			}
		}

		public Core.NpAccountId AccountId
		{
			get
			{
				return accountId;
			}
			set
			{
				accountId = value;
			}
		}

		public byte[] RcvData => rcvData;

		public int PcId
		{
			get
			{
				return pcId;
			}
			set
			{
				pcId = value;
			}
		}

		public void SetRcvDataChunk(byte[] data, ulong startIndex, ulong chunkSize)
		{
			if (startIndex + chunkSize > (ulong)data.Length)
			{
				throw new NpToolkitException("The start Index and chunk size go off the end of the data array.");
			}
			rcvData = data;
			byteOffset = startIndex;
			chunkToRcvDataSize = chunkSize;
		}

		public GetGameDataRequest()
			: base(ServiceTypes.Ranking, FunctionTypes.RankingGetGameData)
		{
		}
	}

	public class TempRankResponse : ResponseBase
	{
		internal uint tempRank;

		public uint TempRank => tempRank;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TempRankBegin);
			tempRank = memoryBuffer.ReadUInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TempRankEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class ScoreRankDataBase
	{
		internal string utf8Comment;

		internal byte[] gameInfo;

		internal int pcId;

		internal uint serialRank;

		internal uint rank;

		internal uint highestRank;

		internal bool hasGameData;

		internal long scoreValue;

		internal DateTime recordDate;

		internal Core.NpAccountId accountId;

		public string Comment => utf8Comment;

		public byte[] GameInfo => gameInfo;

		public int PcId => pcId;

		public uint SerialRank => serialRank;

		public uint Rank => rank;

		public uint HighestRank => highestRank;

		public bool HasGameData => hasGameData;

		public long ScoreValue => scoreValue;

		public DateTime RecordDate => recordDate;

		public Core.NpAccountId AccountId => accountId;

		internal void ReadBase(MemoryBuffer buffer)
		{
			pcId = buffer.ReadInt32();
			serialRank = buffer.ReadUInt32();
			rank = buffer.ReadUInt32();
			highestRank = buffer.ReadUInt32();
			hasGameData = buffer.ReadBool();
			scoreValue = buffer.ReadInt64();
			recordDate = Core.ReadRtcTick(buffer);
			accountId.Read(buffer);
		}

		internal void ReadAdditionalData(MemoryBuffer buffer)
		{
			buffer.ReadString(ref utf8Comment);
			buffer.ReadData(ref gameInfo);
		}
	}

	public class ScoreRankData : ScoreRankDataBase
	{
		internal Core.OnlineID onlineId;

		public Core.OnlineID OnlineId => onlineId;

		internal void ReadData(MemoryBuffer buffer)
		{
			ReadBase(buffer);
			onlineId = new Core.OnlineID();
			onlineId.Read(buffer);
		}
	}

	public class ScoreRankDataForCrossSave : ScoreRankDataBase
	{
		internal Core.NpId npId;

		public Core.NpId NpId => npId;

		internal void ReadData(MemoryBuffer buffer)
		{
			ReadBase(buffer);
			npId.Read(buffer);
		}
	}

	public class ScorePlayerRankData : ScoreRankData
	{
		internal bool hasData;

		public bool HasData => hasData;

		internal void Read(MemoryBuffer buffer)
		{
			hasData = buffer.ReadBool();
			if (hasData)
			{
				ReadData(buffer);
			}
		}
	}

	public class ScorePlayerRankDataForCrossSave : ScoreRankDataForCrossSave
	{
		internal bool hasData;

		public bool HasData => hasData;

		internal void Read(MemoryBuffer buffer)
		{
			hasData = buffer.ReadBool();
			if (hasData)
			{
				ReadData(buffer);
			}
		}
	}

	public class RangeOfRanksResponse : ResponseBase
	{
		internal ScoreRankData[] scoreRankData;

		internal ScoreRankDataForCrossSave[] scoreRankDataForCrossSave;

		internal bool isCrossSaveInformation;

		internal ulong numValidEntries;

		internal DateTime updateTime;

		internal uint totalEntriesOnBoard;

		internal uint boardId;

		internal int startRank;

		public ScoreRankData[] RankData
		{
			get
			{
				if (isCrossSaveInformation)
				{
					throw new NpToolkitException("RankData isn't valid unless 'IsCrossSaveInformation' is set to false.");
				}
				return scoreRankData;
			}
		}

		public ScoreRankDataForCrossSave[] RankDataForCrossSave
		{
			get
			{
				if (!isCrossSaveInformation)
				{
					throw new NpToolkitException("RankDataForCrossSave isn't valid unless 'IsCrossSaveInformation' is set to true.");
				}
				return scoreRankDataForCrossSave;
			}
		}

		public bool IsCrossSaveInformation => isCrossSaveInformation;

		public DateTime UpdateTime => updateTime;

		public uint TotalEntriesOnBoard => totalEntriesOnBoard;

		public uint BoardId => boardId;

		public ulong NumValidEntries => numValidEntries;

		public int StartRank => startRank;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RangeOfRanksBegin);
			isCrossSaveInformation = memoryBuffer.ReadBool();
			ulong num = memoryBuffer.ReadUInt64();
			scoreRankData = null;
			scoreRankDataForCrossSave = null;
			if (num != 0)
			{
				if (isCrossSaveInformation)
				{
					scoreRankDataForCrossSave = new ScoreRankDataForCrossSave[num];
					for (ulong num2 = 0uL; num2 < num; num2++)
					{
						scoreRankDataForCrossSave[num2] = new ScoreRankDataForCrossSave();
						scoreRankDataForCrossSave[num2].ReadData(memoryBuffer);
						scoreRankDataForCrossSave[num2].ReadAdditionalData(memoryBuffer);
					}
				}
				else
				{
					scoreRankData = new ScoreRankData[num];
					for (ulong num2 = 0uL; num2 < num; num2++)
					{
						scoreRankData[num2] = new ScoreRankData();
						scoreRankData[num2].ReadData(memoryBuffer);
						scoreRankData[num2].ReadAdditionalData(memoryBuffer);
					}
				}
			}
			numValidEntries = memoryBuffer.ReadUInt64();
			updateTime = Core.ReadRtcTick(memoryBuffer);
			totalEntriesOnBoard = memoryBuffer.ReadUInt32();
			boardId = memoryBuffer.ReadUInt32();
			startRank = memoryBuffer.ReadInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RangeOfRanksEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class FriendsRanksResponse : ResponseBase
	{
		internal ScoreRankData[] scoreRankData;

		internal ScoreRankDataForCrossSave[] scoreRankDataForCrossSave;

		internal bool isCrossSaveInformation;

		internal ulong numFriends;

		internal DateTime updateTime;

		internal uint boardId;

		internal uint totalEntriesOnBoard;

		internal uint totalFriendsOnBoard;

		internal int friendsWithPcId;

		public ScoreRankData[] RankData
		{
			get
			{
				if (isCrossSaveInformation)
				{
					throw new NpToolkitException("RankData isn't valid unless 'IsCrossSaveInformation' is set to false.");
				}
				return scoreRankData;
			}
		}

		public ScoreRankDataForCrossSave[] RankDataForCrossSave
		{
			get
			{
				if (!isCrossSaveInformation)
				{
					throw new NpToolkitException("RankDataForCrossSave isn't valid unless 'IsCrossSaveInformation' is set to true.");
				}
				return scoreRankDataForCrossSave;
			}
		}

		public bool IsCrossSaveInformation => isCrossSaveInformation;

		public ulong NumFriends => numFriends;

		public DateTime UpdateTime => updateTime;

		public uint BoardId => boardId;

		public uint TotalEntriesOnBoard => totalEntriesOnBoard;

		public uint TotalFriendsOnBoard => totalFriendsOnBoard;

		public int FriendsWithPcId => friendsWithPcId;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.FriendsRanksBegin);
			isCrossSaveInformation = memoryBuffer.ReadBool();
			numFriends = memoryBuffer.ReadUInt64();
			scoreRankData = null;
			scoreRankDataForCrossSave = null;
			if (isCrossSaveInformation)
			{
				scoreRankDataForCrossSave = new ScoreRankDataForCrossSave[numFriends];
				for (ulong num = 0uL; num < numFriends; num++)
				{
					scoreRankDataForCrossSave[num] = new ScoreRankDataForCrossSave();
					scoreRankDataForCrossSave[num].ReadData(memoryBuffer);
					scoreRankDataForCrossSave[num].ReadAdditionalData(memoryBuffer);
				}
			}
			else
			{
				scoreRankData = new ScoreRankData[numFriends];
				for (ulong num = 0uL; num < numFriends; num++)
				{
					scoreRankData[num] = new ScoreRankData();
					scoreRankData[num].ReadData(memoryBuffer);
					scoreRankData[num].ReadAdditionalData(memoryBuffer);
				}
			}
			updateTime = Core.ReadRtcTick(memoryBuffer);
			boardId = memoryBuffer.ReadUInt32();
			totalEntriesOnBoard = memoryBuffer.ReadUInt32();
			totalFriendsOnBoard = memoryBuffer.ReadUInt32();
			friendsWithPcId = memoryBuffer.ReadInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.FriendsRanksEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class UsersRanksResponse : ResponseBase
	{
		internal ScorePlayerRankData[] users;

		internal ScorePlayerRankDataForCrossSave[] usersForCrossSave;

		internal bool isCrossSaveInformation;

		internal ulong numUsers;

		internal ulong numValidUsers;

		internal DateTime updateTime;

		internal uint boardId;

		internal uint totalEntriesOnBoard;

		public ScorePlayerRankData[] Users
		{
			get
			{
				if (isCrossSaveInformation)
				{
					throw new NpToolkitException("RankData isn't valid unless 'IsCrossSaveInformation' is set to false.");
				}
				return users;
			}
		}

		public ScorePlayerRankDataForCrossSave[] UsersForCrossSave
		{
			get
			{
				if (!isCrossSaveInformation)
				{
					throw new NpToolkitException("RankDataForCrossSave isn't valid unless 'IsCrossSaveInformation' is set to true.");
				}
				return usersForCrossSave;
			}
		}

		public bool IsCrossSaveInformation => isCrossSaveInformation;

		public ulong NumUsers => numUsers;

		public ulong NumValidUsers => numValidUsers;

		public DateTime UpdateTime => updateTime;

		public uint BoardId => boardId;

		public uint TotalEntriesOnBoard => totalEntriesOnBoard;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.UsersRanksBegin);
			isCrossSaveInformation = memoryBuffer.ReadBool();
			numUsers = memoryBuffer.ReadUInt64();
			numValidUsers = memoryBuffer.ReadUInt64();
			users = null;
			usersForCrossSave = null;
			if (isCrossSaveInformation)
			{
				usersForCrossSave = new ScorePlayerRankDataForCrossSave[numUsers];
				for (ulong num = 0uL; num < numUsers; num++)
				{
					usersForCrossSave[num] = new ScorePlayerRankDataForCrossSave();
					usersForCrossSave[num].Read(memoryBuffer);
					if (usersForCrossSave[num].HasData)
					{
						usersForCrossSave[num].ReadAdditionalData(memoryBuffer);
					}
				}
			}
			else
			{
				users = new ScorePlayerRankData[numUsers];
				for (ulong num = 0uL; num < numUsers; num++)
				{
					users[num] = new ScorePlayerRankData();
					users[num].Read(memoryBuffer);
					if (users[num].HasData)
					{
						users[num].ReadAdditionalData(memoryBuffer);
					}
				}
			}
			updateTime = Core.ReadRtcTick(memoryBuffer);
			boardId = memoryBuffer.ReadUInt32();
			totalEntriesOnBoard = memoryBuffer.ReadUInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.UsersRanksEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class SetGameDataResultResponse : ResponseBase
	{
		internal int chunkId;

		public int ChunkId => chunkId;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SetGameDataBegin);
			chunkId = memoryBuffer.ReadInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SetGameDataEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class GetGameDataResultResponse : ResponseBase
	{
		internal ulong totalSize;

		internal ulong rcvDataSize;

		internal ulong rcvDataValidSize;

		internal ulong startIndex;

		internal int chunkId;

		internal byte[] rcvData;

		public ulong TotalSize => totalSize;

		public ulong RcvDataSize => rcvDataSize;

		public ulong RcvDataValidSize => rcvDataValidSize;

		public ulong StartIndex => startIndex;

		public byte[] RcvData => rcvData;

		public int ChunkId => chunkId;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GetGameDataBegin);
			totalSize = memoryBuffer.ReadUInt64();
			rcvDataSize = memoryBuffer.ReadUInt64();
			rcvDataValidSize = memoryBuffer.ReadUInt64();
			chunkId = memoryBuffer.ReadInt32();
			GetGameDataRequest getGameDataRequest = request as GetGameDataRequest;
			rcvData = getGameDataRequest.rcvData;
			startIndex = getGameDataRequest.byteOffset;
			memoryBuffer.ReadData(ref rcvData, (uint)startIndex);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GetGameDataEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public const int MAX_NUM_BOARDS = 1000;

	public const int MIN_PCID = 0;

	public const int MAX_PCID = 9;

	public const int MAX_RANGE = 100;

	public const int MIN_RANGE = 1;

	public const int FIRST_RANK = 1;

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSetScore(SetScoreRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetRangeOfRanks(GetRangeOfRanksRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetFriendsRanks(GetFriendsRanksRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetUsersRanks(GetUsersRanksRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSetGameData(SetGameDataRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetGameData(GetGameDataRequest request, out APIResult result);

	public static int SetScore(SetScoreRequest request, TempRankResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSetScore(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetRangeOfRanks(GetRangeOfRanksRequest request, RangeOfRanksResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetRangeOfRanks(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetFriendsRanks(GetFriendsRanksRequest request, FriendsRanksResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetFriendsRanks(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetUsersRanks(GetUsersRanksRequest request, UsersRanksResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetUsersRanks(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SetGameData(SetGameDataRequest request, SetGameDataResultResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSetGameData(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetGameData(GetGameDataRequest request, GetGameDataResultResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetGameData(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
