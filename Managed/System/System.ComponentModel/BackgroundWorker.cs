using System.Threading;
using System.Threading.Tasks;

namespace System.ComponentModel;

/// <summary>Executes an operation on a separate thread.</summary>
[DefaultEvent("DoWork")]
public class BackgroundWorker : Component
{
	private bool _canCancelWorker;

	private bool _workerReportsProgress;

	private bool _cancellationPending;

	private bool _isRunning;

	private AsyncOperation _asyncOperation;

	private readonly SendOrPostCallback _operationCompleted;

	private readonly SendOrPostCallback _progressReporter;

	/// <summary>Gets a value indicating whether the application has requested cancellation of a background operation.</summary>
	/// <returns>true if the application has requested cancellation of a background operation; otherwise, false. The default is false.</returns>
	public bool CancellationPending => _cancellationPending;

	/// <summary>Gets a value indicating whether the <see cref="T:System.ComponentModel.BackgroundWorker" /> is running an asynchronous operation.</summary>
	/// <returns>true, if the <see cref="T:System.ComponentModel.BackgroundWorker" /> is running an asynchronous operation; otherwise, false.</returns>
	public bool IsBusy => _isRunning;

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.ComponentModel.BackgroundWorker" /> can report progress updates.</summary>
	/// <returns>true if the <see cref="T:System.ComponentModel.BackgroundWorker" /> supports progress updates; otherwise false. The default is false.</returns>
	public bool WorkerReportsProgress
	{
		get
		{
			return _workerReportsProgress;
		}
		set
		{
			_workerReportsProgress = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.ComponentModel.BackgroundWorker" /> supports asynchronous cancellation.</summary>
	/// <returns>true if the <see cref="T:System.ComponentModel.BackgroundWorker" /> supports cancellation; otherwise false. The default is false.</returns>
	public bool WorkerSupportsCancellation
	{
		get
		{
			return _canCancelWorker;
		}
		set
		{
			_canCancelWorker = value;
		}
	}

	/// <summary>Occurs when <see cref="M:System.ComponentModel.BackgroundWorker.RunWorkerAsync" /> is called.</summary>
	public event DoWorkEventHandler DoWork;

	/// <summary>Occurs when <see cref="M:System.ComponentModel.BackgroundWorker.ReportProgress(System.Int32)" /> is called.</summary>
	public event ProgressChangedEventHandler ProgressChanged;

	/// <summary>Occurs when the background operation has completed, has been canceled, or has raised an exception.</summary>
	public event RunWorkerCompletedEventHandler RunWorkerCompleted;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.BackgroundWorker" /> class.</summary>
	public BackgroundWorker()
	{
		_operationCompleted = AsyncOperationCompleted;
		_progressReporter = ProgressReporter;
	}

	private void AsyncOperationCompleted(object arg)
	{
		_isRunning = false;
		_cancellationPending = false;
		OnRunWorkerCompleted((RunWorkerCompletedEventArgs)arg);
	}

	/// <summary>Requests cancellation of a pending background operation.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.ComponentModel.BackgroundWorker.WorkerSupportsCancellation" /> is false. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void CancelAsync()
	{
		if (!WorkerSupportsCancellation)
		{
			throw new InvalidOperationException("This BackgroundWorker states that it doesn't support cancellation. Modify WorkerSupportsCancellation to state that it does support cancellation.");
		}
		_cancellationPending = true;
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.BackgroundWorker.DoWork" /> event. </summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnDoWork(DoWorkEventArgs e)
	{
		this.DoWork?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.BackgroundWorker.RunWorkerCompleted" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
	{
		this.RunWorkerCompleted?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.BackgroundWorker.ProgressChanged" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
	{
		this.ProgressChanged?.Invoke(this, e);
	}

	private void ProgressReporter(object arg)
	{
		OnProgressChanged((ProgressChangedEventArgs)arg);
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.BackgroundWorker.ProgressChanged" /> event.</summary>
	/// <param name="percentProgress">The percentage, from 0 to 100, of the background operation that is complete. </param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.ComponentModel.BackgroundWorker.WorkerReportsProgress" /> property is set to false. </exception>
	public void ReportProgress(int percentProgress)
	{
		ReportProgress(percentProgress, null);
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.BackgroundWorker.ProgressChanged" /> event.</summary>
	/// <param name="percentProgress">The percentage, from 0 to 100, of the background operation that is complete.</param>
	/// <param name="userState">The state object passed to <see cref="M:System.ComponentModel.BackgroundWorker.RunWorkerAsync(System.Object)" />.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.ComponentModel.BackgroundWorker.WorkerReportsProgress" /> property is set to false. </exception>
	public void ReportProgress(int percentProgress, object userState)
	{
		if (!WorkerReportsProgress)
		{
			throw new InvalidOperationException("This BackgroundWorker states that it doesn't report progress. Modify WorkerReportsProgress to state that it does report progress.");
		}
		ProgressChangedEventArgs e = new ProgressChangedEventArgs(percentProgress, userState);
		if (_asyncOperation != null)
		{
			_asyncOperation.Post(_progressReporter, e);
		}
		else
		{
			_progressReporter(e);
		}
	}

	/// <summary>Starts execution of a background operation.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.ComponentModel.BackgroundWorker.IsBusy" /> is true.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void RunWorkerAsync()
	{
		RunWorkerAsync(null);
	}

	/// <summary>Starts execution of a background operation.</summary>
	/// <param name="argument">A parameter for use by the background operation to be executed in the <see cref="E:System.ComponentModel.BackgroundWorker.DoWork" /> event handler. </param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.ComponentModel.BackgroundWorker.IsBusy" /> is true. </exception>
	public void RunWorkerAsync(object argument)
	{
		if (_isRunning)
		{
			throw new InvalidOperationException("This BackgroundWorker is currently busy and cannot run multiple tasks concurrently.");
		}
		_isRunning = true;
		_cancellationPending = false;
		_asyncOperation = AsyncOperationManager.CreateOperation(null);
		Task.Factory.StartNew(delegate(object arg)
		{
			WorkerThreadStart(arg);
		}, argument, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
	}

	private void WorkerThreadStart(object argument)
	{
		object result = null;
		Exception error = null;
		bool cancelled = false;
		try
		{
			DoWorkEventArgs e = new DoWorkEventArgs(argument);
			OnDoWork(e);
			if (e.Cancel)
			{
				cancelled = true;
			}
			else
			{
				result = e.Result;
			}
		}
		catch (Exception ex)
		{
			error = ex;
		}
		RunWorkerCompletedEventArgs arg = new RunWorkerCompletedEventArgs(result, error, cancelled);
		_asyncOperation.PostOperationCompleted(_operationCompleted, arg);
	}

	protected override void Dispose(bool disposing)
	{
	}
}
