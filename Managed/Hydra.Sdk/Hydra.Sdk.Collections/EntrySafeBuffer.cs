using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Collections;

public class EntrySafeBuffer<T> where T : IHydraEntry
{
	public delegate void OnBufferExceededDelegate();

	private List<T> _entries;

	private readonly int _maxBufferSize;

	private int _bufferSize;

	private readonly object _entriesLock = new object();

	private readonly Func<IEnumerable<T>, Task<bool>> _onFlush;

	public OnBufferExceededDelegate OnBufferExceeded;

	public int Count
	{
		get
		{
			lock (_entriesLock)
			{
				return _entries.Count;
			}
		}
	}

	public EntrySafeBuffer(int maxBufferSize, Func<IEnumerable<T>, Task<bool>> onFlush)
	{
		_entries = new List<T>();
		_maxBufferSize = maxBufferSize;
		_onFlush = onFlush;
	}

	public void Add(T entry)
	{
		lock (_entriesLock)
		{
			if (_bufferSize + entry.GetSize() > _maxBufferSize)
			{
				OnBufferExceeded?.Invoke();
			}
			AddInternal(entry, _entries);
		}
	}

	private void AddInternal(T entry, List<T> entries)
	{
		_bufferSize += entry.GetSize();
		entries.Add(entry);
	}

	public async Task Flush(bool forceFlush = false)
	{
		List<T> entries;
		lock (_entriesLock)
		{
			entries = _entries.ToList();
			Clear();
		}
		bool isSuccess = false;
		try
		{
			isSuccess = await _onFlush(entries);
			entries.Clear();
		}
		catch (Exception)
		{
			isSuccess = false;
		}
		finally
		{
			lock (_entriesLock)
			{
				List<T> result = new List<T>();
				if (!isSuccess && !forceFlush)
				{
					entries.ForEach(delegate(T e)
					{
						AddInternal(e, result);
					});
				}
				_entries.ForEach(delegate(T e)
				{
					AddInternal(e, result);
				});
				_entries = result;
			}
		}
	}

	private void Clear()
	{
		_entries.Clear();
		_bufferSize = 0;
	}
}
