using System.Diagnostics;
using Unity;

namespace System.Threading.Tasks;

/// <summary>Enables iterations of <see cref="T:System.Threading.Tasks.Parallel" /> loops to interact with other iterations. An instance of this class is provided by the Parallel class to each loop; you can not create instances in your user code.</summary>
[DebuggerDisplay("ShouldExitCurrentIteration = {ShouldExitCurrentIteration}")]
public class ParallelLoopState
{
	private readonly ParallelLoopStateFlags _flagsBase;

	internal virtual bool InternalShouldExitCurrentIteration
	{
		get
		{
			throw new NotSupportedException("This method is not supported.");
		}
	}

	/// <summary>Gets whether the current iteration of the loop should exit based on requests made by this or other iterations.</summary>
	/// <returns>true if the current iteration should exit; otherwise false.</returns>
	public bool ShouldExitCurrentIteration => InternalShouldExitCurrentIteration;

	/// <summary>Gets whether any iteration of the loop has called <see cref="M:System.Threading.Tasks.ParallelLoopState.Stop" />.</summary>
	/// <returns>true if any iteration has stopped the loop; otherwise false.</returns>
	public bool IsStopped => (_flagsBase.LoopStateFlags & 4) != 0;

	/// <summary>Gets whether any iteration of the loop has thrown an exception that went unhandled by that iteration.</summary>
	/// <returns>True if an unhandled exception was thrown; otherwise false.</returns>
	public bool IsExceptional => (_flagsBase.LoopStateFlags & 1) != 0;

	internal virtual long? InternalLowestBreakIteration
	{
		get
		{
			throw new NotSupportedException("This method is not supported.");
		}
	}

	/// <summary>Gets the lowest iteration of the loop from which <see cref="M:System.Threading.Tasks.ParallelLoopState.Break" /> was called. </summary>
	/// <returns>An integer that represents the lowest iteration from which Break was called. In the case of a <see cref="M:System.Threading.Tasks.Parallel.ForEach``1(System.Collections.Concurrent.Partitioner{``0},System.Action{``0})" /> loop, the value is based on an internally-generated index.</returns>
	public long? LowestBreakIteration => InternalLowestBreakIteration;

	internal ParallelLoopState(ParallelLoopStateFlags fbase)
	{
		_flagsBase = fbase;
	}

	/// <summary>Communicates that the <see cref="T:System.Threading.Tasks.Parallel" /> loop should cease execution at the system's earliest convenience.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Threading.Tasks.ParallelLoopState.Break" /> method was previously called. <see cref="M:System.Threading.Tasks.ParallelLoopState.Break" /> and <see cref="M:System.Threading.Tasks.ParallelLoopState.Stop" /> may not be used in combination by iterations of the same loop.</exception>
	public void Stop()
	{
		_flagsBase.Stop();
	}

	internal virtual void InternalBreak()
	{
		throw new NotSupportedException("This method is not supported.");
	}

	/// <summary>Communicates that the <see cref="T:System.Threading.Tasks.Parallel" /> loop should cease execution at the system's earliest convenience of iterations beyond the current iteration.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Threading.Tasks.ParallelLoopState.Stop" /> method was previously called. <see cref="M:System.Threading.Tasks.ParallelLoopState.Break" /> and <see cref="M:System.Threading.Tasks.ParallelLoopState.Stop" /> may not be used in combination by iterations of the same loop.</exception>
	public void Break()
	{
		InternalBreak();
	}

	internal static void Break(int iteration, ParallelLoopStateFlags32 pflags)
	{
		int oldState = 0;
		if (!pflags.AtomicLoopStateUpdate(2, 13, ref oldState))
		{
			if ((oldState & 4) != 0)
			{
				throw new InvalidOperationException("Break was called after Stop was called.");
			}
			return;
		}
		int lowestBreakIteration = pflags._lowestBreakIteration;
		if (iteration >= lowestBreakIteration)
		{
			return;
		}
		SpinWait spinWait = default(SpinWait);
		while (Interlocked.CompareExchange(ref pflags._lowestBreakIteration, iteration, lowestBreakIteration) != lowestBreakIteration)
		{
			spinWait.SpinOnce();
			lowestBreakIteration = pflags._lowestBreakIteration;
			if (iteration > lowestBreakIteration)
			{
				break;
			}
		}
	}

	internal static void Break(long iteration, ParallelLoopStateFlags64 pflags)
	{
		int oldState = 0;
		if (!pflags.AtomicLoopStateUpdate(2, 13, ref oldState))
		{
			if ((oldState & 4) != 0)
			{
				throw new InvalidOperationException("Break was called after Stop was called.");
			}
			return;
		}
		long lowestBreakIteration = pflags.LowestBreakIteration;
		if (iteration >= lowestBreakIteration)
		{
			return;
		}
		SpinWait spinWait = default(SpinWait);
		while (Interlocked.CompareExchange(ref pflags._lowestBreakIteration, iteration, lowestBreakIteration) != lowestBreakIteration)
		{
			spinWait.SpinOnce();
			lowestBreakIteration = pflags.LowestBreakIteration;
			if (iteration > lowestBreakIteration)
			{
				break;
			}
		}
	}

	internal ParallelLoopState()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
