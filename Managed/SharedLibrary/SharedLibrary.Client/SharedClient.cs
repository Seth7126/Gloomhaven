using System;
using SharedLibrary.Logger;
using SharedLibrary.Validation;
using SharedLibrary.YML;

namespace SharedLibrary.Client;

public class SharedClient
{
	public delegate void DLLDebugHandlerCallback(DLLDebug.CDLLDebugLog_MessageData message);

	private static DLLDebugHandlerCallback s_DLLDebugHandler;

	public static DLLDebugHandlerCallback DLLDebugHandler => s_DLLDebugHandler;

	public static YMLValidationRecord ValidationRecord { get; set; }

	public static string CriticalYMLFailure { get; set; }

	public static string ApplicationPersistentDataPath { get; private set; }

	public static string RulebaseDataRoot { get; private set; }

	public static bool IsInitialised { get; private set; }

	public static Random GlobalRNG { get; private set; }

	public static void Initialise(string applicationPersistenDataPath, string rulebaseDataRoot)
	{
		ApplicationPersistentDataPath = applicationPersistenDataPath;
		RulebaseDataRoot = rulebaseDataRoot;
		if (!IsInitialised)
		{
			GlobalRNG = new Random(SeedYML.LoadedYML.Seed);
			IsInitialised = true;
		}
		InitValidationRecord();
		CriticalYMLFailure = string.Empty;
	}

	public static void SetDLLDebugHandler(DLLDebugHandlerCallback messageHandler)
	{
		s_DLLDebugHandler = messageHandler;
	}

	public static void InitValidationRecord()
	{
		if (ValidationRecord == null)
		{
			ValidationRecord = new YMLValidationRecord();
		}
		else
		{
			ValidationRecord.ResetRecord();
		}
	}

	public static Guid GetGUIDBasedOnRNG(Random rng)
	{
		byte[] array = new byte[16];
		rng.NextBytes(array);
		return new Guid(array);
	}
}
