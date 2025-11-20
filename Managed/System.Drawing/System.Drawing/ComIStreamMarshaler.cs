using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace System.Drawing;

internal sealed class ComIStreamMarshaler : ICustomMarshaler
{
	private delegate int QueryInterfaceDelegate(IntPtr @this, [In] ref Guid riid, IntPtr ppvObject);

	private delegate int AddRefDelegate(IntPtr @this);

	private delegate int ReleaseDelegate(IntPtr @this);

	private delegate int ReadDelegate(IntPtr @this, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pv, int cb, IntPtr pcbRead);

	private delegate int WriteDelegate(IntPtr @this, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pv, int cb, IntPtr pcbWritten);

	private delegate int SeekDelegate(IntPtr @this, long dlibMove, int dwOrigin, IntPtr plibNewPosition);

	private delegate int SetSizeDelegate(IntPtr @this, long libNewSize);

	private delegate int CopyToDelegate(IntPtr @this, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Drawing.ComIStreamMarshaler")] IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten);

	private delegate int CommitDelegate(IntPtr @this, int grfCommitFlags);

	private delegate int RevertDelegate(IntPtr @this);

	private delegate int LockRegionDelegate(IntPtr @this, long libOffset, long cb, int dwLockType);

	private delegate int UnlockRegionDelegate(IntPtr @this, long libOffset, long cb, int dwLockType);

	private delegate int StatDelegate(IntPtr @this, out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag);

	private delegate int CloneDelegate(IntPtr @this, out IntPtr ppstm);

	[StructLayout(LayoutKind.Sequential)]
	private sealed class IStreamInterface
	{
		internal IntPtr lpVtbl;

		internal IntPtr gcHandle;
	}

	[StructLayout(LayoutKind.Sequential)]
	private sealed class IStreamVtbl
	{
		internal QueryInterfaceDelegate QueryInterface;

		internal AddRefDelegate AddRef;

		internal ReleaseDelegate Release;

		internal ReadDelegate Read;

		internal WriteDelegate Write;

		internal SeekDelegate Seek;

		internal SetSizeDelegate SetSize;

		internal CopyToDelegate CopyTo;

		internal CommitDelegate Commit;

		internal RevertDelegate Revert;

		internal LockRegionDelegate LockRegion;

		internal UnlockRegionDelegate UnlockRegion;

		internal StatDelegate Stat;

		internal CloneDelegate Clone;
	}

	private sealed class ManagedToNativeWrapper
	{
		[StructLayout(LayoutKind.Sequential)]
		private sealed class ReleaseSlot
		{
			internal ReleaseDelegate Release;
		}

		private static readonly Guid IID_IUnknown;

		private static readonly Guid IID_IStream;

		private static readonly MethodInfo exceptionGetHResult;

		private static readonly IStreamVtbl managedVtable;

		private static IntPtr comVtable;

		private static int vtableRefCount;

		private IStream managedInterface;

		private IntPtr comInterface;

		private GCHandle gcHandle;

		private int refCount = 1;

		static ManagedToNativeWrapper()
		{
			IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
			IID_IStream = new Guid("0000000C-0000-0000-C000-000000000046");
			exceptionGetHResult = typeof(Exception).GetTypeInfo().GetProperty("HResult", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.ExactBinding, null, typeof(int), new Type[0], null).GetGetMethod(nonPublic: true);
			EventHandler value = OnShutdown;
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.DomainUnload += value;
			currentDomain.ProcessExit += value;
			managedVtable = new IStreamVtbl
			{
				QueryInterface = QueryInterface,
				AddRef = AddRef,
				Release = Release,
				Read = Read,
				Write = Write,
				Seek = Seek,
				SetSize = SetSize,
				CopyTo = CopyTo,
				Commit = Commit,
				Revert = Revert,
				LockRegion = LockRegion,
				UnlockRegion = UnlockRegion,
				Stat = Stat,
				Clone = Clone
			};
			CreateVtable();
		}

		private ManagedToNativeWrapper(IStream managedInterface)
		{
			lock (managedVtable)
			{
				if (vtableRefCount == 0 && comVtable == IntPtr.Zero)
				{
					CreateVtable();
				}
				vtableRefCount++;
			}
			try
			{
				this.managedInterface = managedInterface;
				gcHandle = GCHandle.Alloc(this);
				IStreamInterface structure = new IStreamInterface
				{
					lpVtbl = comVtable,
					gcHandle = (IntPtr)gcHandle
				};
				comInterface = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IStreamInterface)));
				Marshal.StructureToPtr(structure, comInterface, fDeleteOld: false);
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		private void Dispose()
		{
			if (gcHandle.IsAllocated)
			{
				gcHandle.Free();
			}
			if (comInterface != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(comInterface);
				comInterface = IntPtr.Zero;
			}
			managedInterface = null;
			lock (managedVtable)
			{
				if (--vtableRefCount == 0 && Environment.HasShutdownStarted)
				{
					DisposeVtable();
				}
			}
		}

		private static void OnShutdown(object sender, EventArgs e)
		{
			lock (managedVtable)
			{
				if (vtableRefCount == 0 && comVtable != IntPtr.Zero)
				{
					DisposeVtable();
				}
			}
		}

		private static void CreateVtable()
		{
			comVtable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IStreamVtbl)));
			Marshal.StructureToPtr(managedVtable, comVtable, fDeleteOld: false);
		}

		private static void DisposeVtable()
		{
			Marshal.DestroyStructure(comVtable, typeof(IStreamVtbl));
			Marshal.FreeHGlobal(comVtable);
			comVtable = IntPtr.Zero;
		}

		internal static IStream GetUnderlyingInterface(IntPtr comInterface, bool outParam)
		{
			if (Marshal.ReadIntPtr(comInterface) == comVtable)
			{
				IStream result = GetObject(comInterface).managedInterface;
				if (outParam)
				{
					Release(comInterface);
				}
				return result;
			}
			return null;
		}

		internal static IntPtr GetInterface(IStream managedInterface)
		{
			if (managedInterface == null)
			{
				return IntPtr.Zero;
			}
			IntPtr underlyingInterface;
			if ((underlyingInterface = NativeToManagedWrapper.GetUnderlyingInterface(managedInterface)) == IntPtr.Zero)
			{
				underlyingInterface = new ManagedToNativeWrapper(managedInterface).comInterface;
			}
			return underlyingInterface;
		}

		internal static void ReleaseInterface(IntPtr comInterface)
		{
			if (comInterface != IntPtr.Zero)
			{
				IntPtr intPtr = Marshal.ReadIntPtr(comInterface);
				if (intPtr == comVtable)
				{
					Release(comInterface);
				}
				else
				{
					((ReleaseSlot)Marshal.PtrToStructure((IntPtr)((long)intPtr + IntPtr.Size * 2), typeof(ReleaseSlot))).Release(comInterface);
				}
			}
		}

		private static int GetHRForException(Exception e)
		{
			return (int)exceptionGetHResult.Invoke(e, null);
		}

		private static ManagedToNativeWrapper GetObject(IntPtr @this)
		{
			return (ManagedToNativeWrapper)((GCHandle)Marshal.ReadIntPtr(@this, IntPtr.Size)).Target;
		}

		private static int QueryInterface(IntPtr @this, ref Guid riid, IntPtr ppvObject)
		{
			try
			{
				if (IID_IUnknown.Equals(riid) || IID_IStream.Equals(riid))
				{
					Marshal.WriteIntPtr(ppvObject, @this);
					AddRef(@this);
					return 0;
				}
				Marshal.WriteIntPtr(ppvObject, IntPtr.Zero);
				return -2147467262;
			}
			catch (Exception e)
			{
				return GetHRForException(e);
			}
		}

		private static int AddRef(IntPtr @this)
		{
			int num;
			try
			{
				ManagedToNativeWrapper managedToNativeWrapper = GetObject(@this);
				lock (managedToNativeWrapper)
				{
					num = ++managedToNativeWrapper.refCount;
					num = num;
				}
			}
			catch
			{
				num = 0;
			}
			return num;
		}

		private static int Release(IntPtr @this)
		{
			int num;
			try
			{
				ManagedToNativeWrapper managedToNativeWrapper = GetObject(@this);
				lock (managedToNativeWrapper)
				{
					if (managedToNativeWrapper.refCount != 0)
					{
						num = --managedToNativeWrapper.refCount;
						if (num == 0)
						{
							managedToNativeWrapper.Dispose();
						}
					}
					num = managedToNativeWrapper.refCount;
				}
			}
			catch
			{
				num = 0;
			}
			return num;
		}

		private static int Read(IntPtr @this, byte[] pv, int cb, IntPtr pcbRead)
		{
			try
			{
				GetObject(@this).managedInterface.Read(pv, cb, pcbRead);
				return 0;
			}
			catch (Exception e)
			{
				return GetHRForException(e);
			}
		}

		private static int Write(IntPtr @this, byte[] pv, int cb, IntPtr pcbWritten)
		{
			try
			{
				GetObject(@this).managedInterface.Write(pv, cb, pcbWritten);
				return 0;
			}
			catch (Exception e)
			{
				return GetHRForException(e);
			}
		}

		private static int Seek(IntPtr @this, long dlibMove, int dwOrigin, IntPtr plibNewPosition)
		{
			try
			{
				GetObject(@this).managedInterface.Seek(dlibMove, dwOrigin, plibNewPosition);
				return 0;
			}
			catch (Exception e)
			{
				return GetHRForException(e);
			}
		}

		private static int SetSize(IntPtr @this, long libNewSize)
		{
			try
			{
				GetObject(@this).managedInterface.SetSize(libNewSize);
				return 0;
			}
			catch (Exception e)
			{
				return GetHRForException(e);
			}
		}

		private static int CopyTo(IntPtr @this, IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
		{
			try
			{
				GetObject(@this).managedInterface.CopyTo(pstm, cb, pcbRead, pcbWritten);
				return 0;
			}
			catch (Exception e)
			{
				return GetHRForException(e);
			}
		}

		private static int Commit(IntPtr @this, int grfCommitFlags)
		{
			try
			{
				GetObject(@this).managedInterface.Commit(grfCommitFlags);
				return 0;
			}
			catch (Exception e)
			{
				return GetHRForException(e);
			}
		}

		private static int Revert(IntPtr @this)
		{
			try
			{
				GetObject(@this).managedInterface.Revert();
				return 0;
			}
			catch (Exception e)
			{
				return GetHRForException(e);
			}
		}

		private static int LockRegion(IntPtr @this, long libOffset, long cb, int dwLockType)
		{
			try
			{
				GetObject(@this).managedInterface.LockRegion(libOffset, cb, dwLockType);
				return 0;
			}
			catch (Exception e)
			{
				return GetHRForException(e);
			}
		}

		private static int UnlockRegion(IntPtr @this, long libOffset, long cb, int dwLockType)
		{
			try
			{
				GetObject(@this).managedInterface.UnlockRegion(libOffset, cb, dwLockType);
				return 0;
			}
			catch (Exception e)
			{
				return GetHRForException(e);
			}
		}

		private static int Stat(IntPtr @this, out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
		{
			try
			{
				GetObject(@this).managedInterface.Stat(out pstatstg, grfStatFlag);
				return 0;
			}
			catch (Exception e)
			{
				pstatstg = default(System.Runtime.InteropServices.ComTypes.STATSTG);
				return GetHRForException(e);
			}
		}

		private static int Clone(IntPtr @this, out IntPtr ppstm)
		{
			ppstm = IntPtr.Zero;
			try
			{
				GetObject(@this).managedInterface.Clone(out var ppstm2);
				ppstm = GetInterface(ppstm2);
				return 0;
			}
			catch (Exception e)
			{
				return GetHRForException(e);
			}
		}
	}

	private sealed class NativeToManagedWrapper : IStream
	{
		private IntPtr comInterface;

		private IStreamVtbl managedVtable;

		private NativeToManagedWrapper(IntPtr comInterface, bool outParam)
		{
			this.comInterface = comInterface;
			managedVtable = (IStreamVtbl)Marshal.PtrToStructure(Marshal.ReadIntPtr(comInterface), typeof(IStreamVtbl));
			if (!outParam)
			{
				managedVtable.AddRef(comInterface);
			}
		}

		~NativeToManagedWrapper()
		{
			Dispose(disposing: false);
		}

		private void Dispose(bool disposing)
		{
			managedVtable.Release(comInterface);
			if (disposing)
			{
				comInterface = IntPtr.Zero;
				managedVtable = null;
				GC.SuppressFinalize(this);
			}
		}

		internal static IntPtr GetUnderlyingInterface(IStream managedInterface)
		{
			if (managedInterface is NativeToManagedWrapper)
			{
				NativeToManagedWrapper nativeToManagedWrapper = (NativeToManagedWrapper)managedInterface;
				nativeToManagedWrapper.managedVtable.AddRef(nativeToManagedWrapper.comInterface);
				return nativeToManagedWrapper.comInterface;
			}
			return IntPtr.Zero;
		}

		internal static IStream GetInterface(IntPtr comInterface, bool outParam)
		{
			if (comInterface == IntPtr.Zero)
			{
				return null;
			}
			IStream result;
			if ((result = ManagedToNativeWrapper.GetUnderlyingInterface(comInterface, outParam)) == null)
			{
				result = new NativeToManagedWrapper(comInterface, outParam);
			}
			return result;
		}

		internal static void ReleaseInterface(IStream managedInterface)
		{
			if (managedInterface is NativeToManagedWrapper)
			{
				((NativeToManagedWrapper)managedInterface).Dispose(disposing: true);
			}
		}

		private static void ThrowExceptionForHR(int result)
		{
			if (result < 0)
			{
				throw new COMException(null, result);
			}
		}

		public void Read(byte[] pv, int cb, IntPtr pcbRead)
		{
			ThrowExceptionForHR(managedVtable.Read(comInterface, pv, cb, pcbRead));
		}

		public void Write(byte[] pv, int cb, IntPtr pcbWritten)
		{
			ThrowExceptionForHR(managedVtable.Write(comInterface, pv, cb, pcbWritten));
		}

		public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
		{
			ThrowExceptionForHR(managedVtable.Seek(comInterface, dlibMove, dwOrigin, plibNewPosition));
		}

		public void SetSize(long libNewSize)
		{
			ThrowExceptionForHR(managedVtable.SetSize(comInterface, libNewSize));
		}

		public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
		{
			ThrowExceptionForHR(managedVtable.CopyTo(comInterface, pstm, cb, pcbRead, pcbWritten));
		}

		public void Commit(int grfCommitFlags)
		{
			ThrowExceptionForHR(managedVtable.Commit(comInterface, grfCommitFlags));
		}

		public void Revert()
		{
			ThrowExceptionForHR(managedVtable.Revert(comInterface));
		}

		public void LockRegion(long libOffset, long cb, int dwLockType)
		{
			ThrowExceptionForHR(managedVtable.LockRegion(comInterface, libOffset, cb, dwLockType));
		}

		public void UnlockRegion(long libOffset, long cb, int dwLockType)
		{
			ThrowExceptionForHR(managedVtable.UnlockRegion(comInterface, libOffset, cb, dwLockType));
		}

		public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
		{
			ThrowExceptionForHR(managedVtable.Stat(comInterface, out pstatstg, grfStatFlag));
		}

		public void Clone(out IStream ppstm)
		{
			ThrowExceptionForHR(managedVtable.Clone(comInterface, out var ppstm2));
			ppstm = GetInterface(ppstm2, outParam: true);
		}
	}

	private const int S_OK = 0;

	private const int E_NOINTERFACE = -2147467262;

	private static readonly ComIStreamMarshaler defaultInstance = new ComIStreamMarshaler();

	private ComIStreamMarshaler()
	{
	}

	private static ICustomMarshaler GetInstance(string cookie)
	{
		return defaultInstance;
	}

	public IntPtr MarshalManagedToNative(object managedObj)
	{
		return ManagedToNativeWrapper.GetInterface((IStream)managedObj);
	}

	public void CleanUpNativeData(IntPtr pNativeData)
	{
		ManagedToNativeWrapper.ReleaseInterface(pNativeData);
	}

	public object MarshalNativeToManaged(IntPtr pNativeData)
	{
		return NativeToManagedWrapper.GetInterface(pNativeData, outParam: false);
	}

	public void CleanUpManagedData(object managedObj)
	{
		NativeToManagedWrapper.ReleaseInterface((IStream)managedObj);
	}

	public int GetNativeDataSize()
	{
		return -1;
	}
}
