using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace System.Diagnostics;

/// <summary>Represents a stack trace, which is an ordered collection of one or more stack frames.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
[MonoTODO("Serialized objects are not compatible with .NET")]
public class StackTrace
{
	internal enum TraceFormat
	{
		Normal,
		TrailingNewLine,
		NoResourceLookup
	}

	/// <summary>Defines the default for the number of methods to omit from the stack trace. This field is constant.</summary>
	/// <filterpriority>1</filterpriority>
	public const int METHODS_TO_SKIP = 0;

	private const string prefix = "  at ";

	private StackFrame[] frames;

	private readonly StackTrace[] captured_traces;

	private bool debug_info;

	private static bool isAotidSet;

	private static string aotid;

	/// <summary>Gets the number of frames in the stack trace.</summary>
	/// <returns>The number of frames in the stack trace. </returns>
	/// <filterpriority>2</filterpriority>
	public virtual int FrameCount
	{
		get
		{
			if (frames != null)
			{
				return frames.Length;
			}
			return 0;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.StackTrace" /> class from the caller's frame.</summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public StackTrace()
	{
		init_frames(0, fNeedFileInfo: false);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.StackTrace" /> class from the caller's frame, optionally capturing source information.</summary>
	/// <param name="fNeedFileInfo">true to capture the file name, line number, and column number; otherwise, false. </param>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public StackTrace(bool fNeedFileInfo)
	{
		init_frames(0, fNeedFileInfo);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.StackTrace" /> class from the caller's frame, skipping the specified number of frames.</summary>
	/// <param name="skipFrames">The number of frames up the stack from which to start the trace. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="skipFrames" /> parameter is negative. </exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public StackTrace(int skipFrames)
	{
		init_frames(skipFrames, fNeedFileInfo: false);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.StackTrace" /> class from the caller's frame, skipping the specified number of frames and optionally capturing source information.</summary>
	/// <param name="skipFrames">The number of frames up the stack from which to start the trace. </param>
	/// <param name="fNeedFileInfo">true to capture the file name, line number, and column number; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="skipFrames" /> parameter is negative. </exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public StackTrace(int skipFrames, bool fNeedFileInfo)
	{
		init_frames(skipFrames, fNeedFileInfo);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void init_frames(int skipFrames, bool fNeedFileInfo)
	{
		if (skipFrames < 0)
		{
			throw new ArgumentOutOfRangeException("< 0", "skipFrames");
		}
		List<StackFrame> list = new List<StackFrame>();
		skipFrames += 2;
		StackFrame stackFrame;
		while ((stackFrame = new StackFrame(skipFrames, fNeedFileInfo)) != null && stackFrame.GetMethod() != null)
		{
			list.Add(stackFrame);
			skipFrames++;
		}
		debug_info = fNeedFileInfo;
		frames = list.ToArray();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern StackFrame[] get_trace(Exception e, int skipFrames, bool fNeedFileInfo);

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.StackTrace" /> class using the provided exception object.</summary>
	/// <param name="e">The exception object from which to construct the stack trace. </param>
	/// <exception cref="T:System.ArgumentNullException">The parameter <paramref name="e" /> is null. </exception>
	public StackTrace(Exception e)
		: this(e, 0, fNeedFileInfo: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.StackTrace" /> class, using the provided exception object and optionally capturing source information.</summary>
	/// <param name="e">The exception object from which to construct the stack trace. </param>
	/// <param name="fNeedFileInfo">true to capture the file name, line number, and column number; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">The parameter <paramref name="e" /> is null. </exception>
	public StackTrace(Exception e, bool fNeedFileInfo)
		: this(e, 0, fNeedFileInfo)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.StackTrace" /> class using the provided exception object and skipping the specified number of frames.</summary>
	/// <param name="e">The exception object from which to construct the stack trace. </param>
	/// <param name="skipFrames">The number of frames up the stack from which to start the trace. </param>
	/// <exception cref="T:System.ArgumentNullException">The parameter <paramref name="e" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="skipFrames" /> parameter is negative. </exception>
	public StackTrace(Exception e, int skipFrames)
		: this(e, skipFrames, fNeedFileInfo: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.StackTrace" /> class using the provided exception object, skipping the specified number of frames and optionally capturing source information.</summary>
	/// <param name="e">The exception object from which to construct the stack trace. </param>
	/// <param name="skipFrames">The number of frames up the stack from which to start the trace. </param>
	/// <param name="fNeedFileInfo">true to capture the file name, line number, and column number; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">The parameter <paramref name="e" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="skipFrames" /> parameter is negative. </exception>
	public StackTrace(Exception e, int skipFrames, bool fNeedFileInfo)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (skipFrames < 0)
		{
			throw new ArgumentOutOfRangeException("< 0", "skipFrames");
		}
		frames = get_trace(e, skipFrames, fNeedFileInfo);
		captured_traces = e.captured_traces;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.StackTrace" /> class that contains a single frame.</summary>
	/// <param name="frame">The frame that the <see cref="T:System.Diagnostics.StackTrace" /> object should contain. </param>
	public StackTrace(StackFrame frame)
	{
		frames = new StackFrame[1];
		frames[0] = frame;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.StackTrace" /> class for a specific thread, optionally capturing source information. Do not use this constructor overload.</summary>
	/// <param name="targetThread">The thread whose stack trace is requested. </param>
	/// <param name="needFileInfo">true to capture the file name, line number, and column number; otherwise, false. </param>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread <paramref name="targetThread" /> is not suspended. </exception>
	[MonoLimitation("Not possible to create StackTraces from other threads")]
	[Obsolete]
	public StackTrace(Thread targetThread, bool needFileInfo)
	{
		if (targetThread == Thread.CurrentThread)
		{
			init_frames(0, needFileInfo);
			return;
		}
		throw new NotImplementedException();
	}

	internal StackTrace(StackFrame[] frames)
	{
		this.frames = frames;
	}

	/// <summary>Gets the specified stack frame.</summary>
	/// <returns>The specified stack frame.</returns>
	/// <param name="index">The index of the stack frame requested. </param>
	/// <filterpriority>2</filterpriority>
	public virtual StackFrame GetFrame(int index)
	{
		if (index < 0 || index >= FrameCount)
		{
			return null;
		}
		return frames[index];
	}

	/// <summary>Returns a copy of all stack frames in the current stack trace.</summary>
	/// <returns>An array of type <see cref="T:System.Diagnostics.StackFrame" /> representing the function calls in the stack trace.</returns>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public virtual StackFrame[] GetFrames()
	{
		if (captured_traces == null)
		{
			return frames;
		}
		List<StackFrame> list = new List<StackFrame>();
		StackTrace[] array = captured_traces;
		foreach (StackTrace stackTrace in array)
		{
			for (int j = 0; j < stackTrace.FrameCount; j++)
			{
				list.Add(stackTrace.GetFrame(j));
			}
		}
		list.AddRange(frames);
		return list.ToArray();
	}

	private static string GetAotId()
	{
		if (!isAotidSet)
		{
			byte[] aotId = RuntimeAssembly.GetAotId();
			if (aotId != null)
			{
				aotid = new Guid(aotId).ToString("N");
			}
			isAotidSet = true;
		}
		return aotid;
	}

	private bool AddFrames(StringBuilder sb, bool separator, out bool isAsync)
	{
		isAsync = false;
		bool flag = false;
		for (int i = 0; i < FrameCount; i++)
		{
			StackFrame frame = GetFrame(i);
			if (frame.GetMethod() == null)
			{
				if (flag || separator)
				{
					sb.Append(Environment.NewLine);
				}
				sb.Append("  at ");
				string internalMethodName = frame.GetInternalMethodName();
				if (internalMethodName != null)
				{
					sb.Append(internalMethodName);
				}
				else
				{
					sb.AppendFormat("<0x{0:x5} + 0x{1:x5}> <unknown method>", frame.GetMethodAddress(), frame.GetNativeOffset());
				}
			}
			else
			{
				GetFullNameForStackTrace(sb, frame.GetMethod(), flag || separator, out var skipped, out isAsync);
				if (skipped)
				{
					continue;
				}
				if (frame.GetILOffset() == -1)
				{
					sb.AppendFormat(" <0x{0:x5} + 0x{1:x5}>", frame.GetMethodAddress(), frame.GetNativeOffset());
					if (frame.GetMethodIndex() != 16777215)
					{
						sb.AppendFormat(" {0}", frame.GetMethodIndex());
					}
				}
				else
				{
					sb.AppendFormat(" [0x{0:x5}]", frame.GetILOffset());
				}
				string text = frame.GetSecureFileName();
				if (text[0] == '<')
				{
					string arg = frame.GetMethod().Module.ModuleVersionId.ToString("N");
					string aotId = GetAotId();
					text = ((frame.GetILOffset() == -1 && aotId != null) ? $"<{arg}#{aotId}>" : $"<{arg}>");
				}
				sb.AppendFormat(" in {0}:{1} ", text, frame.GetFileLineNumber());
			}
			flag = true;
		}
		return flag;
	}

	private void GetFullNameForStackTrace(StringBuilder sb, MethodBase mi, bool needsNewLine, out bool skipped, out bool isAsync)
	{
		Type declaringType = mi.DeclaringType;
		if (declaringType.IsGenericType && !declaringType.IsGenericTypeDefinition)
		{
			declaringType = declaringType.GetGenericTypeDefinition();
			MethodInfo[] methods = declaringType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.MetadataToken == mi.MetadataToken)
				{
					mi = methodInfo;
					break;
				}
			}
		}
		isAsync = typeof(IAsyncStateMachine).IsAssignableFrom(declaringType);
		skipped = mi.IsDefined(typeof(StackTraceHiddenAttribute)) || declaringType.IsDefined(typeof(StackTraceHiddenAttribute));
		if (skipped)
		{
			return;
		}
		if (isAsync)
		{
			ConvertAsyncStateMachineMethod(ref mi, ref declaringType);
		}
		if (needsNewLine)
		{
			sb.Append(Environment.NewLine);
		}
		sb.Append("  at ");
		sb.Append(declaringType.ToString());
		sb.Append(".");
		sb.Append(mi.Name);
		if (mi.IsGenericMethod)
		{
			mi = ((MethodInfo)mi).GetGenericMethodDefinition();
			Type[] genericArguments = mi.GetGenericArguments();
			sb.Append("[");
			for (int j = 0; j < genericArguments.Length; j++)
			{
				if (j > 0)
				{
					sb.Append(",");
				}
				sb.Append(genericArguments[j].Name);
			}
			sb.Append("]");
		}
		ParameterInfo[] parameters = mi.GetParameters();
		sb.Append(" (");
		for (int k = 0; k < parameters.Length; k++)
		{
			if (k > 0)
			{
				sb.Append(", ");
			}
			Type type = parameters[k].ParameterType;
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				type = type.GetGenericTypeDefinition();
			}
			sb.Append(type.ToString());
			if (parameters[k].Name != null)
			{
				sb.Append(" ");
				sb.Append(parameters[k].Name);
			}
		}
		sb.Append(")");
	}

	private static void ConvertAsyncStateMachineMethod(ref MethodBase method, ref Type declaringType)
	{
		Type declaringType2 = declaringType.DeclaringType;
		if (declaringType2 == null)
		{
			return;
		}
		MethodInfo[] methods = declaringType2.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (methods == null)
		{
			return;
		}
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			IEnumerable<AsyncStateMachineAttribute> customAttributes = methodInfo.GetCustomAttributes<AsyncStateMachineAttribute>();
			if (customAttributes == null)
			{
				continue;
			}
			foreach (AsyncStateMachineAttribute item in customAttributes)
			{
				if (item.StateMachineType == declaringType)
				{
					method = methodInfo;
					declaringType = methodInfo.DeclaringType;
					return;
				}
			}
		}
	}

	/// <summary>Builds a readable representation of the stack trace.</summary>
	/// <returns>A readable representation of the stack trace.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		if (captured_traces != null)
		{
			StackTrace[] array = captured_traces;
			for (int i = 0; i < array.Length; i++)
			{
				flag = array[i].AddFrames(stringBuilder, flag, out var isAsync);
				if (flag && !isAsync)
				{
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append("--- End of stack trace from previous location where exception was thrown ---");
					stringBuilder.Append(Environment.NewLine);
				}
			}
		}
		AddFrames(stringBuilder, flag, out var _);
		return stringBuilder.ToString();
	}

	internal string ToString(TraceFormat traceFormat)
	{
		return ToString();
	}
}
