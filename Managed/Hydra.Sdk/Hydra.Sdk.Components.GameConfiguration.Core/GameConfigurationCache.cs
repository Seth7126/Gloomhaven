using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hydra.Api.GameConfiguration;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Components.GameConfiguration.Core;

internal class GameConfigurationCache : IGameConfigurationCache
{
	private ConcurrentDictionary<string, ConfigurationCacheEntry> _cache;

	private bool _isLocked;

	private readonly int _bufferSize;

	private readonly IHydraSdkLogger _logger;

	public GameConfigurationCache(int bufferSizeInBytes, IHydraSdkLogger logger = null)
	{
		_bufferSize = bufferSizeInBytes;
		_logger = logger;
		_cache = new ConcurrentDictionary<string, ConfigurationCacheEntry>();
	}

	public async Task AwaitCache()
	{
		if (!_isLocked)
		{
			return;
		}
		DateTime endTime = DateTime.UtcNow.AddSeconds(5.0);
		while (_isLocked)
		{
			await Task.Delay(100);
			if (endTime < DateTime.UtcNow)
			{
				_logger?.Log(HydraLogType.Warning, this.GetLogCatWrn(), "Cache timeout!");
				_isLocked = false;
			}
		}
	}

	public void Lock()
	{
		_isLocked = true;
	}

	public void Release()
	{
		int num = _bufferSize - _cache.Values.Sum((ConfigurationCacheEntry x) => x.Size);
		if (num < 0)
		{
			IOrderedEnumerable<KeyValuePair<string, ConfigurationCacheEntry>> source = _cache.OrderBy((KeyValuePair<string, ConfigurationCacheEntry> o) => o.Value.Usages);
			while (num < 0)
			{
				int num2 = num;
				if (_cache.TryRemove(source.First().Key, out var value))
				{
					IHydraSdkLogger logger = _logger;
					if (logger != null)
					{
						string logCatInf = this.GetLogCatInf();
						int size = value.Size;
						logger.Log(HydraLogType.Information, logCatInf, "Cache cleared in bytes: " + size);
					}
					num += value.Size;
					if (num == num2)
					{
						_logger?.Log(HydraLogType.Error, this.GetLogCatErr(), "Clear cache cycle error.");
						break;
					}
					continue;
				}
				_logger?.Log(HydraLogType.Error, this.GetLogCatErr(), "Clear cache failed");
				break;
			}
		}
		_isLocked = false;
	}

	public bool TryAdd(string key, ComponentDataResult data)
	{
		return _cache.TryAdd(key, new ConfigurationCacheEntry(data));
	}

	public bool TryGetValue(string key, out ComponentDataResult data)
	{
		if (_cache.TryGetValue(key, out var value))
		{
			value.Usages++;
			data = value.Result;
			return true;
		}
		data = null;
		return false;
	}

	public bool IsCached(string key)
	{
		return _cache.ContainsKey(key);
	}
}
