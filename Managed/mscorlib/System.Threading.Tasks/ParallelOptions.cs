namespace System.Threading.Tasks;

/// <summary>Stores options that configure the operation of methods on the <see cref="T:System.Threading.Tasks.Parallel" /> class.</summary>
public class ParallelOptions
{
	private TaskScheduler _scheduler;

	private int _maxDegreeOfParallelism;

	private CancellationToken _cancellationToken;

	/// <summary>Gets or sets the <see cref="T:System.Threading.Tasks.TaskScheduler" /> associated with this <see cref="T:System.Threading.Tasks.ParallelOptions" /> instance. Setting this property to null indicates that the current scheduler should be used.</summary>
	/// <returns>The task scheduler that is associated with this instance.</returns>
	public TaskScheduler TaskScheduler
	{
		get
		{
			return _scheduler;
		}
		set
		{
			_scheduler = value;
		}
	}

	internal TaskScheduler EffectiveTaskScheduler
	{
		get
		{
			if (_scheduler == null)
			{
				return TaskScheduler.Current;
			}
			return _scheduler;
		}
	}

	/// <summary>Gets or sets the maximum degree of parallelism enabled by this ParallelOptions instance.</summary>
	/// <returns>An integer that represents the maximum degree of parallelism.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when this <see cref="P:System.Threading.Tasks.ParallelOptions.MaxDegreeOfParallelism" /> is set to 0 or some value less than -1.</exception>
	public int MaxDegreeOfParallelism
	{
		get
		{
			return _maxDegreeOfParallelism;
		}
		set
		{
			if (value == 0 || value < -1)
			{
				throw new ArgumentOutOfRangeException("MaxDegreeOfParallelism");
			}
			_maxDegreeOfParallelism = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Threading.CancellationToken" /> associated with this <see cref="T:System.Threading.Tasks.ParallelOptions" /> instance.</summary>
	/// <returns>The token that is associated with this instance.</returns>
	public CancellationToken CancellationToken
	{
		get
		{
			return _cancellationToken;
		}
		set
		{
			_cancellationToken = value;
		}
	}

	internal int EffectiveMaxConcurrencyLevel
	{
		get
		{
			int num = MaxDegreeOfParallelism;
			int maximumConcurrencyLevel = EffectiveTaskScheduler.MaximumConcurrencyLevel;
			if (maximumConcurrencyLevel > 0 && maximumConcurrencyLevel != int.MaxValue)
			{
				num = ((num == -1) ? maximumConcurrencyLevel : Math.Min(maximumConcurrencyLevel, num));
			}
			return num;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.ParallelOptions" /> class.</summary>
	public ParallelOptions()
	{
		_scheduler = TaskScheduler.Default;
		_maxDegreeOfParallelism = -1;
		_cancellationToken = CancellationToken.None;
	}
}
