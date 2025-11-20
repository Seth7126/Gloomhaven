using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.ExtProtocol;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.ExtProtocol;

public abstract class ExtProtocolEndpoint : IMonoBehaviour_Update, ITranslateEndpoint, IDisposable
{
	private static readonly Random Rng = new Random();

	private readonly Dictionary<Guid, ProtocolTransactionHandle> _transactionHandles = new Dictionary<Guid, ProtocolTransactionHandle>();

	private readonly object _sync = new object();

	private bool _disposed;

	private Process _process;

	private Thread _thread;

	private bool _startedThread;

	private bool _initializing;

	private bool _failed;

	private float _lastRequestTimestamp;

	private string _gameRoot;

	public abstract string Id { get; }

	public abstract string FriendlyName { get; }

	public virtual int MaxConcurrency => 1;

	public virtual int MaxTranslationsPerRequest => 1;

	protected string ExecutablePath { get; set; }

	protected string Arguments { get; set; }

	protected float MinDelay { get; set; }

	protected float MaxDelay { get; set; }

	protected virtual string ConfigurationSectionName => null;

	protected string ConfigForExternalProcess { get; set; }

	public virtual void Initialize(IInitializationContext context)
	{
		_gameRoot = Paths.GameRoot;
		string text = null;
		if (ConfigurationSectionName != null)
		{
			text = context.GetOrCreateSetting<string>(ConfigurationSectionName, "ExecutableLocation", null);
		}
		if (string.IsNullOrEmpty(text))
		{
			text = Path.Combine(context.TranslatorDirectory, "FullNET\\Common.ExtProtocol.Executor.exe");
		}
		if (!File.Exists(text))
		{
			throw new EndpointInitializationException("Could not find any executable at '" + text + "'");
		}
		ExecutablePath = text;
	}

	private void EnsureInitialized()
	{
		if (!_startedThread)
		{
			_startedThread = true;
			_initializing = true;
			_thread = new Thread(ReaderLoop);
			_thread.IsBackground = true;
			_thread.Start();
		}
	}

	private void ReaderLoop(object state)
	{
		try
		{
			if (_process == null)
			{
				string text = ExecutablePath;
				if (!Path.IsPathRooted(text))
				{
					try
					{
						text = Path.Combine(_gameRoot, ExecutablePath);
					}
					catch
					{
						text = Path.Combine(Environment.CurrentDirectory, ExecutablePath);
					}
				}
				_process = new Process();
				_process.StartInfo.FileName = text;
				_process.StartInfo.Arguments = Arguments;
				_process.StartInfo.WorkingDirectory = new FileInfo(ExecutablePath).Directory.FullName;
				_process.EnableRaisingEvents = false;
				_process.StartInfo.UseShellExecute = false;
				_process.StartInfo.CreateNoWindow = true;
				_process.StartInfo.RedirectStandardInput = true;
				_process.StartInfo.RedirectStandardOutput = true;
				_process.StartInfo.RedirectStandardError = true;
				_process.Start();
				_process.WaitForExit(2500);
			}
			if (!_process.HasExited)
			{
				SendConfigurationIfRequired();
				_initializing = false;
				while (!_disposed)
				{
					ProtocolMessage message = ExtProtocolConvert.Decode(_process.StandardOutput.ReadLine());
					HandleProtocolMessageResponse(message);
				}
			}
		}
		catch (Exception e)
		{
			_failed = true;
			_initializing = false;
			XuaLogger.AutoTranslator.Error(e, "Error occurred while reading standard output from external process.");
		}
	}

	public virtual void Update()
	{
		if (TimeSupport.Time.frameCount % 30 != 0)
		{
			return;
		}
		lock (_sync)
		{
			float realtimeSinceStartup = TimeSupport.Time.realtimeSinceStartup;
			List<Guid> list = null;
			foreach (KeyValuePair<Guid, ProtocolTransactionHandle> transactionHandle in _transactionHandles)
			{
				if (realtimeSinceStartup - transactionHandle.Value.StartTime > 60f)
				{
					if (list == null)
					{
						list = new List<Guid>();
					}
					list.Add(transactionHandle.Key);
					transactionHandle.Value.SetCompleted(null, "Request timed out.", StatusCode.Unknown);
				}
			}
			if (list == null)
			{
				return;
			}
			foreach (Guid item in list)
			{
				_transactionHandles.Remove(item);
			}
		}
	}

	private void SendConfigurationIfRequired()
	{
		string configForExternalProcess = ConfigForExternalProcess;
		if (configForExternalProcess != null)
		{
			Guid id = Guid.NewGuid();
			try
			{
				string value = ExtProtocolConvert.Encode(new ConfigurationMessage
				{
					Id = id,
					Config = configForExternalProcess
				});
				_process.StandardInput.WriteLine(value);
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while sending configuration to external process for '" + GetType().Name + "'.");
			}
		}
	}

	public IEnumerator Translate(ITranslationContext context)
	{
		EnsureInitialized();
		while (_initializing && !_failed)
		{
			object obj = CoroutineHelper.CreateWaitForSecondsRealtime(0.2f);
			if (obj != null)
			{
				yield return obj;
			}
			else
			{
				yield return null;
			}
		}
		if (_failed)
		{
			context.Fail("External process failed.");
		}
		float num = ((float)Rng.Next((int)((MaxDelay - MinDelay) * 1000f)) + MinDelay * 1000f) / 1000f;
		float num2 = TimeSupport.Time.realtimeSinceStartup - _lastRequestTimestamp;
		if (num2 < num)
		{
			float num3 = num - num2;
			object obj2 = CoroutineHelper.CreateWaitForSecondsRealtime(num3);
			if (obj2 != null)
			{
				yield return obj2;
			}
			else
			{
				float realtimeSinceStartup = TimeSupport.Time.realtimeSinceStartup;
				float end = realtimeSinceStartup + num3;
				while (TimeSupport.Time.realtimeSinceStartup < end)
				{
					yield return null;
				}
			}
		}
		_lastRequestTimestamp = TimeSupport.Time.realtimeSinceStartup;
		ProtocolTransactionHandle result = new ProtocolTransactionHandle();
		Guid guid = Guid.NewGuid();
		lock (_sync)
		{
			_transactionHandles[guid] = result;
		}
		try
		{
			string value = ExtProtocolConvert.Encode(new TranslationRequest
			{
				Id = guid,
				SourceLanguage = context.SourceLanguage,
				DestinationLanguage = context.DestinationLanguage,
				UntranslatedTextInfos = context.UntranslatedTextInfos.Select((UntranslatedTextInfo x) => x.ToTransmittable()).ToArray()
			});
			_process.StandardInput.WriteLine(value);
		}
		catch (Exception ex)
		{
			result.SetCompleted(null, ex.Message, StatusCode.Unknown);
		}
		IEnumerator iterator = result.GetSupportedEnumerator();
		while (iterator.MoveNext())
		{
			yield return iterator.Current;
		}
		if (!result.Succeeded)
		{
			context.Fail("Error occurred while retrieving translation. " + result.Error);
		}
		context.Complete(result.Results);
	}

	private void HandleProtocolMessageResponse(ProtocolMessage message)
	{
		if (!(message is TranslationResponse message2))
		{
			if (message is TranslationError message3)
			{
				HandleTranslationError(message3);
			}
		}
		else
		{
			HandleTranslationResponse(message2);
		}
	}

	private void HandleTranslationResponse(TranslationResponse message)
	{
		lock (_sync)
		{
			if (_transactionHandles.TryGetValue(message.Id, out var value))
			{
				value.SetCompleted(message.TranslatedTexts, null, StatusCode.OK);
				_transactionHandles.Remove(message.Id);
			}
		}
	}

	private void HandleTranslationError(TranslationError message)
	{
		lock (_sync)
		{
			if (_transactionHandles.TryGetValue(message.Id, out var value))
			{
				value.SetCompleted(null, message.Reason, message.FailureCode);
				_transactionHandles.Remove(message.Id);
			}
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing && _process != null)
			{
				_process.Dispose();
				_thread.Abort();
			}
			_disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
