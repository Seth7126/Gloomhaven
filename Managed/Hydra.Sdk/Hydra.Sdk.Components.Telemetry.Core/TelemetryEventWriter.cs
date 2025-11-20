using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hydra.Api.Telemetry;
using Hydra.Sdk.Collections;
using Hydra.Sdk.Communication.States;
using Hydra.Sdk.Communication.States.Core;
using Hydra.Sdk.Enums;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.Telemetry.Core;

public class TelemetryEventWriter
{
	private StateObserver<SdkState> _sdkState;

	private StateObserver<SdkSessionInfo> _sessionInfo;

	private readonly TelemetryPackBuilder _packBuilder;

	private readonly TelemetryPackOptions _options;

	private readonly TimeSpan _flushDelay;

	private readonly Task _flushLoop;

	private DateTime _nextFlush;

	private Task _flushTask;

	private EntrySafeBuffer<TelemetryEventEntry> _buffer;

	private Func<TelemetryPack, Task<bool>> _onFlush;

	private CancellationTokenSource _cancellationSource;

	private IHydraSdkLogger _logger;

	private Stopwatch _eventTimer;

	public TelemetryEventWriter(StateObserver<SdkState> sdkState, StateObserver<SdkSessionInfo> sessionInfo, int maxBufferSize, TimeSpan flushDelay, Func<TelemetryPack, Task<bool>> onFlush, IHydraSdkLogger logger = null)
	{
		_sdkState = sdkState;
		_sessionInfo = sessionInfo;
		_onFlush = onFlush;
		_flushDelay = flushDelay;
		_logger = logger;
		_buffer = new EntrySafeBuffer<TelemetryEventEntry>(maxBufferSize, (IEnumerable<TelemetryEventEntry> entries) => entries.Any() ? Flush(entries) : Task.FromResult(result: true));
		EntrySafeBuffer<TelemetryEventEntry> buffer = _buffer;
		buffer.OnBufferExceeded = (EntrySafeBuffer<TelemetryEventEntry>.OnBufferExceededDelegate)Delegate.Combine(buffer.OnBufferExceeded, new EntrySafeBuffer<TelemetryEventEntry>.OnBufferExceededDelegate(BufferExceeded));
		_packBuilder = new TelemetryPackBuilder();
		_eventTimer = Stopwatch.StartNew();
		_options = new TelemetryPackOptions
		{
			Compression = TelemetryPackCompression.Gzip
		};
		_cancellationSource = new CancellationTokenSource();
		_flushLoop = Task.Run((Func<Task>)FlushLoop, _cancellationSource.Token);
	}

	private void BufferExceeded()
	{
		_nextFlush = DateTime.UtcNow;
	}

	public void WriteEvent(TelemetryEventBaseEntry rawEventData)
	{
		_buffer.Add(new TelemetryEventEntry((int)_eventTimer.ElapsedMilliseconds, rawEventData, null));
	}

	private Task<bool> Flush(IEnumerable<TelemetryEventEntry> entries)
	{
		_nextFlush = DateTime.UtcNow.Add(_flushDelay);
		TelemetryPack arg = _packBuilder.Build(_options, entries.ToList(), _sessionInfo.State, _flushDelay, new List<TelemetryContext>());
		_eventTimer.Restart();
		return _onFlush(arg);
	}

	private async Task FlushLoop()
	{
		_nextFlush = DateTime.UtcNow.Add(_flushDelay);
		while (!_cancellationSource.IsCancellationRequested)
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
					_logger?.Log(HydraLogType.Error, this.GetLogCatErr(), "Telemetry flush loop error: {0}", ex2.GetErrorMessage());
					goto IL_0236;
				}
				continue;
			}
			goto IL_0236;
			IL_0236:
			await Task.Delay(1000).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public async Task Shutdown()
	{
		_cancellationSource.Cancel();
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
			await _buffer.Flush();
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger?.Log(HydraLogType.Error, this.GetLogCatErr(), "{0}", ex2.Message);
		}
	}
}
