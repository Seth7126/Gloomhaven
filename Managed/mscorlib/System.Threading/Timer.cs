using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Threading;

/// <summary>Provides a mechanism for executing a method at specified intervals. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[ComVisible(true)]
public sealed class Timer : MarshalByRefObject, IDisposable, IAsyncDisposable
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct TimerComparer : IComparer, IComparer<Timer>
	{
		int IComparer.Compare(object x, object y)
		{
			if (x == y)
			{
				return 0;
			}
			if (!(x is Timer tx))
			{
				return -1;
			}
			if (!(y is Timer ty))
			{
				return 1;
			}
			return Compare(tx, ty);
		}

		public int Compare(Timer tx, Timer ty)
		{
			return Math.Sign(tx.next_run - ty.next_run);
		}
	}

	private sealed class Scheduler
	{
		private static readonly Scheduler instance = new Scheduler();

		private volatile bool needReSort = true;

		private List<Timer> list;

		private long current_next_run = long.MaxValue;

		private ManualResetEvent changed;

		public static Scheduler Instance => instance;

		private void InitScheduler()
		{
			changed = new ManualResetEvent(initialState: false);
			Thread thread = new Thread(SchedulerThread);
			thread.IsBackground = true;
			thread.Start();
		}

		private void WakeupScheduler()
		{
			changed.Set();
		}

		private void SchedulerThread()
		{
			Thread.CurrentThread.Name = "Timer-Scheduler";
			while (true)
			{
				int millisecondsTimeout = -1;
				lock (this)
				{
					changed.Reset();
					millisecondsTimeout = RunSchedulerLoop();
				}
				changed.WaitOne(millisecondsTimeout);
			}
		}

		private Scheduler()
		{
			list = new List<Timer>(1024);
			InitScheduler();
		}

		public void Remove(Timer timer)
		{
			lock (this)
			{
				InternalRemove(timer);
			}
		}

		public void Change(Timer timer, long new_next_run)
		{
			if (timer.is_dead)
			{
				timer.is_dead = false;
			}
			bool flag = false;
			lock (this)
			{
				needReSort = true;
				if (!timer.is_added)
				{
					timer.next_run = new_next_run;
					Add(timer);
					flag = current_next_run > new_next_run;
				}
				else
				{
					if (new_next_run == long.MaxValue)
					{
						timer.next_run = new_next_run;
						InternalRemove(timer);
						return;
					}
					if (!timer.disposed)
					{
						timer.next_run = new_next_run;
						flag = current_next_run > new_next_run;
					}
				}
			}
			if (flag)
			{
				WakeupScheduler();
			}
		}

		private void Add(Timer timer)
		{
			timer.is_added = true;
			needReSort = true;
			list.Add(timer);
			if (list.Count == 1)
			{
				WakeupScheduler();
			}
		}

		private void InternalRemove(Timer timer)
		{
			timer.is_dead = true;
			needReSort = true;
		}

		private static void TimerCB(object o)
		{
			Timer timer = (Timer)o;
			timer.callback(timer.state);
		}

		private void FireTimer(Timer timer)
		{
			long period_ms = timer.period_ms;
			long due_time_ms = timer.due_time_ms;
			if (period_ms == -1 || ((period_ms == 0L || period_ms == -1) && due_time_ms != -1))
			{
				timer.next_run = long.MaxValue;
				timer.is_dead = true;
			}
			else
			{
				timer.next_run = GetTimeMonotonic() + 10000 * timer.period_ms;
				timer.is_dead = false;
			}
			ThreadPool.UnsafeQueueUserWorkItem(TimerCB, timer);
		}

		private int RunSchedulerLoop()
		{
			int num = -1;
			long timeMonotonic = GetTimeMonotonic();
			TimerComparer timerComparer = default(TimerComparer);
			if (needReSort)
			{
				list.Sort(timerComparer);
				needReSort = false;
			}
			long num2 = long.MaxValue;
			for (int i = 0; i < list.Count; i++)
			{
				Timer timer = list[i];
				if (!timer.is_dead)
				{
					if (timer.next_run <= timeMonotonic)
					{
						FireTimer(timer);
					}
					num2 = Math.Min(num2, timer.next_run);
					if (timer.next_run > timeMonotonic && timer.next_run < long.MaxValue)
					{
						timer.is_dead = false;
					}
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				Timer timer2 = list[i];
				if (timer2.is_dead)
				{
					timer2.is_added = false;
					needReSort = true;
					list[i] = list[list.Count - 1];
					i--;
					list.RemoveAt(list.Count - 1);
					if (list.Count == 0)
					{
						break;
					}
				}
			}
			if (needReSort)
			{
				list.Sort(timerComparer);
				needReSort = false;
			}
			num = -1;
			current_next_run = num2;
			if (num2 != long.MaxValue)
			{
				long num3 = (num2 - GetTimeMonotonic()) / 10000;
				if (num3 > int.MaxValue)
				{
					num = 2147483646;
				}
				else
				{
					num = (int)num3;
					if (num < 0)
					{
						num = 0;
					}
				}
			}
			return num;
		}
	}

	private TimerCallback callback;

	private object state;

	private long due_time_ms;

	private long period_ms;

	private long next_run;

	private bool disposed;

	private bool is_dead;

	private bool is_added;

	private const long MaxValue = 4294967294L;

	private static Scheduler scheduler => Scheduler.Instance;

	/// <summary>Initializes a new instance of the Timer class, using a 32-bit signed integer to specify the time interval.</summary>
	/// <param name="callback">A <see cref="T:System.Threading.TimerCallback" /> delegate representing a method to be executed. </param>
	/// <param name="state">An object containing information to be used by the callback method, or null. </param>
	/// <param name="dueTime">The amount of time to delay before <paramref name="callback" /> is invoked, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from starting. Specify zero (0) to start the timer immediately. </param>
	/// <param name="period">The time interval between invocations of <paramref name="callback" />, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="callback" /> parameter is null. </exception>
	public Timer(TimerCallback callback, object state, int dueTime, int period)
	{
		Init(callback, state, dueTime, period);
	}

	/// <summary>Initializes a new instance of the Timer class, using 64-bit signed integers to measure time intervals.</summary>
	/// <param name="callback">A <see cref="T:System.Threading.TimerCallback" /> delegate representing a method to be executed. </param>
	/// <param name="state">An object containing information to be used by the callback method, or null. </param>
	/// <param name="dueTime">The amount of time to delay before <paramref name="callback" /> is invoked, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from starting. Specify zero (0) to start the timer immediately. </param>
	/// <param name="period">The time interval between invocations of <paramref name="callback" />, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is greater than 4294967294. </exception>
	public Timer(TimerCallback callback, object state, long dueTime, long period)
	{
		Init(callback, state, dueTime, period);
	}

	/// <summary>Initializes a new instance of the Timer class, using <see cref="T:System.TimeSpan" /> values to measure time intervals.</summary>
	/// <param name="callback">A <see cref="T:System.Threading.TimerCallback" /> delegate representing a method to be executed. </param>
	/// <param name="state">An object containing information to be used by the callback method, or null. </param>
	/// <param name="dueTime">The <see cref="T:System.TimeSpan" /> representing the amount of time to delay before the <paramref name="callback" /> parameter invokes its methods. Specify negative one (-1) milliseconds to prevent the timer from starting. Specify zero (0) to start the timer immediately. </param>
	/// <param name="period">The time interval between invocations of the methods referenced by <paramref name="callback" />. Specify negative one (-1) milliseconds to disable periodic signaling. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of milliseconds in the value of <paramref name="dueTime" /> or <paramref name="period" /> is negative and not equal to <see cref="F:System.Threading.Timeout.Infinite" />, or is greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="callback" /> parameter is null. </exception>
	public Timer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
	{
		Init(callback, state, (long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);
	}

	/// <summary>Initializes a new instance of the Timer class, using 32-bit unsigned integers to measure time intervals.</summary>
	/// <param name="callback">A <see cref="T:System.Threading.TimerCallback" /> delegate representing a method to be executed. </param>
	/// <param name="state">An object containing information to be used by the callback method, or null. </param>
	/// <param name="dueTime">The amount of time to delay before <paramref name="callback" /> is invoked, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from starting. Specify zero (0) to start the timer immediately. </param>
	/// <param name="period">The time interval between invocations of <paramref name="callback" />, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="callback" /> parameter is null. </exception>
	[CLSCompliant(false)]
	public Timer(TimerCallback callback, object state, uint dueTime, uint period)
	{
		Init(callback, state, (dueTime == uint.MaxValue) ? (-1L) : ((long)dueTime), (period == uint.MaxValue) ? (-1L) : ((long)period));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Timer" /> class with an infinite period and an infinite due time, using the newly created <see cref="T:System.Threading.Timer" /> object as the state object. </summary>
	/// <param name="callback">A <see cref="T:System.Threading.TimerCallback" /> delegate representing a method to be executed.</param>
	public Timer(TimerCallback callback)
	{
		Init(callback, this, -1L, -1L);
	}

	private void Init(TimerCallback callback, object state, long dueTime, long period)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		this.callback = callback;
		this.state = state;
		is_dead = false;
		is_added = false;
		Change(dueTime, period, first: true);
	}

	/// <summary>Changes the start time and the interval between method invocations for a timer, using 32-bit signed integers to measure time intervals.</summary>
	/// <returns>true if the timer was successfully updated; otherwise, false.</returns>
	/// <param name="dueTime">The amount of time to delay before the invoking the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from restarting. Specify zero (0) to restart the timer immediately. </param>
	/// <param name="period">The time interval between invocations of the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Timer" /> has already been disposed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <filterpriority>2</filterpriority>
	public bool Change(int dueTime, int period)
	{
		return Change(dueTime, period, first: false);
	}

	/// <summary>Changes the start time and the interval between method invocations for a timer, using <see cref="T:System.TimeSpan" /> values to measure time intervals.</summary>
	/// <returns>true if the timer was successfully updated; otherwise, false.</returns>
	/// <param name="dueTime">A <see cref="T:System.TimeSpan" /> representing the amount of time to delay before invoking the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed. Specify negative one (-1) milliseconds to prevent the timer from restarting. Specify zero (0) to restart the timer immediately. </param>
	/// <param name="period">The time interval between invocations of the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed. Specify negative one (-1) milliseconds to disable periodic signaling. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Timer" /> has already been disposed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter, in milliseconds, is less than -1. </exception>
	/// <exception cref="T:System.NotSupportedException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter, in milliseconds, is greater than 4294967294. </exception>
	/// <filterpriority>2</filterpriority>
	public bool Change(TimeSpan dueTime, TimeSpan period)
	{
		return Change((long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds, first: false);
	}

	/// <summary>Changes the start time and the interval between method invocations for a timer, using 32-bit unsigned integers to measure time intervals.</summary>
	/// <returns>true if the timer was successfully updated; otherwise, false.</returns>
	/// <param name="dueTime">The amount of time to delay before the invoking the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from restarting. Specify zero (0) to restart the timer immediately. </param>
	/// <param name="period">The time interval between invocations of the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Timer" /> has already been disposed. </exception>
	/// <filterpriority>2</filterpriority>
	[CLSCompliant(false)]
	public bool Change(uint dueTime, uint period)
	{
		long dueTime2 = ((dueTime == uint.MaxValue) ? (-1L) : ((long)dueTime));
		long period2 = ((period == uint.MaxValue) ? (-1L) : ((long)period));
		return Change(dueTime2, period2, first: false);
	}

	/// <summary>Releases all resources used by the current instance of <see cref="T:System.Threading.Timer" />.</summary>
	/// <filterpriority>2</filterpriority>
	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			scheduler.Remove(this);
		}
	}

	/// <summary>Changes the start time and the interval between method invocations for a timer, using 64-bit signed integers to measure time intervals.</summary>
	/// <returns>true if the timer was successfully updated; otherwise, false.</returns>
	/// <param name="dueTime">The amount of time to delay before the invoking the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from restarting. Specify zero (0) to restart the timer immediately. </param>
	/// <param name="period">The time interval between invocations of the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Timer" /> has already been disposed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is less than -1. </exception>
	/// <exception cref="T:System.NotSupportedException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is greater than 4294967294. </exception>
	/// <filterpriority>2</filterpriority>
	public bool Change(long dueTime, long period)
	{
		return Change(dueTime, period, first: false);
	}

	private bool Change(long dueTime, long period, bool first)
	{
		if (dueTime > 4294967294u)
		{
			throw new ArgumentOutOfRangeException("dueTime", "Due time too large");
		}
		if (period > 4294967294u)
		{
			throw new ArgumentOutOfRangeException("period", "Period too large");
		}
		if (dueTime < -1)
		{
			throw new ArgumentOutOfRangeException("dueTime");
		}
		if (period < -1)
		{
			throw new ArgumentOutOfRangeException("period");
		}
		if (disposed)
		{
			throw new ObjectDisposedException(null, Environment.GetResourceString("Cannot access a disposed object."));
		}
		due_time_ms = dueTime;
		period_ms = period;
		long new_next_run;
		if (dueTime == 0L)
		{
			new_next_run = 0L;
		}
		else if (dueTime < 0)
		{
			new_next_run = long.MaxValue;
			if (first)
			{
				next_run = new_next_run;
				return true;
			}
		}
		else
		{
			new_next_run = dueTime * 10000 + GetTimeMonotonic();
		}
		scheduler.Change(this, new_next_run);
		return true;
	}

	/// <summary>Releases all resources used by the current instance of <see cref="T:System.Threading.Timer" /> and signals when the timer has been disposed of.</summary>
	/// <returns>true if the function succeeds; otherwise, false.</returns>
	/// <param name="notifyObject">The <see cref="T:System.Threading.WaitHandle" /> to be signaled when the Timer has been disposed of. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="notifyObject" /> parameter is null. </exception>
	/// <filterpriority>2</filterpriority>
	public bool Dispose(WaitHandle notifyObject)
	{
		if (notifyObject == null)
		{
			throw new ArgumentNullException("notifyObject");
		}
		Dispose();
		NativeEventCalls.SetEvent(notifyObject.SafeWaitHandle);
		return true;
	}

	public ValueTask DisposeAsync()
	{
		Dispose();
		return new ValueTask(Task.FromResult<object>(null));
	}

	internal void KeepRootedWhileScheduled()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern long GetTimeMonotonic();
}
