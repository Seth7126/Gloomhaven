using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hydra.Api.Facts;
using Hydra.Sdk.Collections;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.Facts.Core;

internal class FactWriter
{
	private readonly StateObserver<SdkSessionInfo> _sessionInfo;

	private readonly StateObserver<SdkState> _sdkState;

	private readonly TimeSpan _flushDelay;

	private readonly FactGlobalContext _globalContext;

	private readonly FactContext _context;

	private readonly EntrySafeBuffer<FactEntry> _buffer;

	private readonly FactPackBuilder _packBuilder;

	private readonly FactsPackOptions _options;

	private DateTime _nextFlush;

	private readonly Task _flushLoop;

	private Task _flushTask;

	private readonly Func<FactPack, Task<bool>> _onFlush;

	private readonly CancellationTokenSource _tokenSource;

	private readonly IHydraSdkLogger _logger;

	public FactWriter(int maxBufferSize, TimeSpan flushDelay, StateObserver<SdkSessionInfo> sessionInfo, StateObserver<SdkState> sdkState, Func<FactPack, Task<bool>> onFlush, IHydraSdkLogger logger = null)
	{
		_globalContext = new FactGlobalContext();
		_context = new FactContext();
		_onFlush = onFlush;
		_buffer = new EntrySafeBuffer<FactEntry>(maxBufferSize, (IEnumerable<FactEntry> entries) => entries.Any() ? Flush(entries) : Task.FromResult(result: true));
		EntrySafeBuffer<FactEntry> buffer = _buffer;
		buffer.OnBufferExceeded = (EntrySafeBuffer<FactEntry>.OnBufferExceededDelegate)Delegate.Combine(buffer.OnBufferExceeded, new EntrySafeBuffer<FactEntry>.OnBufferExceededDelegate(BufferExceeded));
		_packBuilder = new FactPackBuilder();
		_options = new FactsPackOptions
		{
			IsCompressed = true,
			IsLocalTime = true
		};
		_flushDelay = flushDelay;
		_sdkState = sdkState;
		_sessionInfo = sessionInfo;
		_logger = logger;
		_tokenSource = new CancellationTokenSource();
		_flushLoop = Task.Run((Func<Task>)FlushLoop, _tokenSource.Token);
	}

	private void BufferExceeded()
	{
		_nextFlush = DateTime.UtcNow;
	}

	public void LogMessage(string category, string description, params object[] args)
	{
		FactEntry entry = new FactEntry(category, _context.Get(category), description, args);
		AddEntryToBuffer(entry);
	}

	public void LogInformation(string category, string description, params object[] args)
	{
		FactEntry entry = new FactEntry(category, new FactsContext
		{
			PropertyName = "INFO",
			PropertyValue = string.Empty
		}, description, args);
		AddEntryToBuffer(entry);
	}

	public void LogWarning(string category, string description, params object[] args)
	{
		FactEntry entry = new FactEntry(category, new FactsContext
		{
			PropertyName = "WARNING",
			PropertyValue = string.Empty
		}, description, args);
		AddEntryToBuffer(entry);
	}

	public void LogError(string category, string description, params object[] args)
	{
		FactEntry entry = new FactEntry(category, new FactsContext
		{
			PropertyName = "ERROR",
			PropertyValue = string.Empty
		}, description, args);
		AddEntryToBuffer(entry);
	}

	public void SetContext(string category, string name, string value)
	{
		_context.Set(category, new FactsContext
		{
			PropertyName = name,
			PropertyValue = value
		});
	}

	public void RemoveContext(string category)
	{
		_context.Remove(category);
	}

	public void RemoveAllContext()
	{
		_context.RemoveAll();
	}

	public void UpdateGlobalContext(string name, string value)
	{
		if (_globalContext.TryAdd(name, value))
		{
			FactEntry entry = new FactEntry("ADF/GLOBAL_CONTEXT/UPDATE", null, "'{0}' {1}", name, value);
			AddEntryToBuffer(entry);
		}
	}

	private void AddEntryToBuffer(FactEntry entry)
	{
		_buffer.Add(entry);
	}

	private async Task FlushLoop()
	{
		_nextFlush = DateTime.UtcNow.Add(_flushDelay);
		while (!_tokenSource.IsCancellationRequested)
		{
			if (_sdkState.State.State == OnlineState.Offline || _sdkState.State.Suspended)
			{
				_nextFlush = DateTime.UtcNow.Add(_flushDelay);
			}
			else if (_nextFlush - DateTime.UtcNow < TimeSpan.Zero)
			{
				try
				{
					if (_flushTask == null)
					{
						goto IL_0184;
					}
					if (_flushTask.IsCompleted)
					{
						_flushTask = null;
						goto IL_0184;
					}
					await _flushTask;
					goto end_IL_00d5;
					IL_0184:
					if (_buffer.Count > 0)
					{
						_flushTask = Task.Run(() => _buffer.Flush());
					}
					_nextFlush = DateTime.UtcNow.Add(_flushDelay);
					goto IL_0236;
					end_IL_00d5:;
				}
				catch (Exception ex)
				{
					Exception ex2 = ex;
					_logger?.Log(HydraLogType.Error, this.GetLogCatErr(), "Facts flush loop error: {0}", ex2.GetErrorMessage());
					goto IL_0236;
				}
				continue;
			}
			goto IL_0236;
			IL_0236:
			await Task.Delay(1000).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private Task<bool> Flush(IEnumerable<FactEntry> entries)
	{
		_nextFlush = DateTime.UtcNow.Add(_flushDelay);
		FactPack arg = _packBuilder.Build(_options, entries.ToList(), _sessionInfo.State, _flushDelay, _globalContext.Get());
		return _onFlush(arg);
	}

	public async Task Shutdown()
	{
		_tokenSource.Cancel();
		try
		{
			if (_flushTask != null)
			{
				if (!_flushTask.IsCompleted)
				{
					await _flushTask;
				}
				else
				{
					_flushTask = null;
				}
			}
			await _flushLoop;
			if (_sessionInfo.State?.SessionId != null)
			{
				_options.IsFinal = true;
				await _buffer.Flush();
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger?.Log(HydraLogType.Error, this.GetLogCatErr(), "{0}", ex2.Message);
		}
	}
}
