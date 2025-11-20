using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Net.Security;

internal abstract class SafeDeleteContext : SafeHandle
{
	internal global::Interop.SspiCli.CredHandle _handle;

	private const string dummyStr = " ";

	private static readonly byte[] s_dummyBytes = new byte[1];

	private static readonly IdnMapping s_idnMapping = new IdnMapping();

	protected SafeFreeCredentials _EffectiveCredential;

	public override bool IsInvalid
	{
		get
		{
			if (!base.IsClosed)
			{
				return _handle.IsZero;
			}
			return true;
		}
	}

	protected SafeDeleteContext()
		: base(IntPtr.Zero, ownsHandle: true)
	{
		_handle = default(global::Interop.SspiCli.CredHandle);
	}

	public override string ToString()
	{
		return _handle.ToString();
	}

	internal unsafe static int InitializeSecurityContext(ref SafeFreeCredentials inCredentials, ref SafeDeleteContext refContext, string targetName, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness endianness, SecurityBuffer inSecBuffer, SecurityBuffer[] inSecBuffers, SecurityBuffer outSecBuffer, ref global::Interop.SspiCli.ContextFlags outFlags)
	{
		if (outSecBuffer == null)
		{
			NetEventSource.Fail(null, "outSecBuffer != null", "InitializeSecurityContext");
		}
		if (inSecBuffer != null && inSecBuffers != null)
		{
			NetEventSource.Fail(null, "inSecBuffer == null || inSecBuffers == null", "InitializeSecurityContext");
		}
		if (inCredentials == null)
		{
			throw new ArgumentNullException("inCredentials");
		}
		global::Interop.SspiCli.SecBufferDesc secBufferDesc = default(global::Interop.SspiCli.SecBufferDesc);
		bool flag = false;
		if (inSecBuffer != null)
		{
			secBufferDesc = new global::Interop.SspiCli.SecBufferDesc(1);
			flag = true;
		}
		else if (inSecBuffers != null)
		{
			secBufferDesc = new global::Interop.SspiCli.SecBufferDesc(inSecBuffers.Length);
			flag = true;
		}
		global::Interop.SspiCli.SecBufferDesc outputBuffer = new global::Interop.SspiCli.SecBufferDesc(1);
		bool flag2 = (((inFlags & global::Interop.SspiCli.ContextFlags.AllocateMemory) != global::Interop.SspiCli.ContextFlags.Zero) ? true : false);
		int num = -1;
		global::Interop.SspiCli.CredHandle credHandle = default(global::Interop.SspiCli.CredHandle);
		if (refContext != null)
		{
			credHandle = refContext._handle;
		}
		GCHandle[] array = null;
		GCHandle gCHandle = default(GCHandle);
		SafeFreeContextBuffer safeFreeContextBuffer = null;
		try
		{
			gCHandle = GCHandle.Alloc(outSecBuffer.token, GCHandleType.Pinned);
			global::Interop.SspiCli.SecBuffer[] array2 = new global::Interop.SspiCli.SecBuffer[(!flag) ? 1 : secBufferDesc.cBuffers];
			fixed (global::Interop.SspiCli.SecBuffer* ptr = array2)
			{
				void* pBuffers = ptr;
				if (flag)
				{
					secBufferDesc.pBuffers = pBuffers;
					array = new GCHandle[secBufferDesc.cBuffers];
					for (int i = 0; i < secBufferDesc.cBuffers; i++)
					{
						SecurityBuffer securityBuffer = ((inSecBuffer != null) ? inSecBuffer : inSecBuffers[i]);
						if (securityBuffer != null)
						{
							array2[i].cbBuffer = securityBuffer.size;
							array2[i].BufferType = securityBuffer.type;
							if (securityBuffer.unmanagedToken != null)
							{
								array2[i].pvBuffer = securityBuffer.unmanagedToken.DangerousGetHandle();
								continue;
							}
							if (securityBuffer.token == null || securityBuffer.token.Length == 0)
							{
								array2[i].pvBuffer = IntPtr.Zero;
								continue;
							}
							array[i] = GCHandle.Alloc(securityBuffer.token, GCHandleType.Pinned);
							array2[i].pvBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(securityBuffer.token, securityBuffer.offset);
						}
					}
				}
				global::Interop.SspiCli.SecBuffer secBuffer = default(global::Interop.SspiCli.SecBuffer);
				outputBuffer.pBuffers = &secBuffer;
				secBuffer.cbBuffer = outSecBuffer.size;
				secBuffer.BufferType = outSecBuffer.type;
				if (outSecBuffer.token == null || outSecBuffer.token.Length == 0)
				{
					secBuffer.pvBuffer = IntPtr.Zero;
				}
				else
				{
					secBuffer.pvBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(outSecBuffer.token, outSecBuffer.offset);
				}
				if (flag2)
				{
					safeFreeContextBuffer = SafeFreeContextBuffer.CreateEmptyHandle();
				}
				if (refContext == null || refContext.IsInvalid)
				{
					refContext = new SafeDeleteContext_SECURITY();
				}
				if (targetName == null || targetName.Length == 0)
				{
					targetName = " ";
				}
				string ascii = s_idnMapping.GetAscii(targetName);
				fixed (char* ptr2 = ascii)
				{
					num = MustRunInitializeSecurityContext(ref inCredentials, credHandle.IsZero ? null : (&credHandle), (byte*)(((object)targetName == " ") ? null : ptr2), inFlags, endianness, flag ? (&secBufferDesc) : null, refContext, ref outputBuffer, ref outFlags, safeFreeContextBuffer);
				}
				if (NetEventSource.IsEnabled)
				{
					NetEventSource.Info(null, "Marshalling OUT buffer", "InitializeSecurityContext");
				}
				outSecBuffer.size = secBuffer.cbBuffer;
				outSecBuffer.type = secBuffer.BufferType;
				if (outSecBuffer.size > 0)
				{
					outSecBuffer.token = new byte[outSecBuffer.size];
					Marshal.Copy(secBuffer.pvBuffer, outSecBuffer.token, 0, outSecBuffer.size);
				}
				else
				{
					outSecBuffer.token = null;
				}
			}
		}
		finally
		{
			if (array != null)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].IsAllocated)
					{
						array[j].Free();
					}
				}
			}
			if (gCHandle.IsAllocated)
			{
				gCHandle.Free();
			}
			safeFreeContextBuffer?.Dispose();
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Exit(null, $"errorCode:0x{num:x8}, refContext:{refContext}", "InitializeSecurityContext");
		}
		return num;
	}

	private unsafe static int MustRunInitializeSecurityContext(ref SafeFreeCredentials inCredentials, void* inContextPtr, byte* targetName, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness endianness, global::Interop.SspiCli.SecBufferDesc* inputBuffer, SafeDeleteContext outContext, ref global::Interop.SspiCli.SecBufferDesc outputBuffer, ref global::Interop.SspiCli.ContextFlags attributes, SafeFreeContextBuffer handleTemplate)
	{
		int num = -2146893055;
		try
		{
			bool success = false;
			inCredentials.DangerousAddRef(ref success);
			outContext.DangerousAddRef(ref success);
			global::Interop.SspiCli.CredHandle credentialHandle = inCredentials._handle;
			num = global::Interop.SspiCli.InitializeSecurityContextW(ref credentialHandle, inContextPtr, targetName, inFlags, 0, endianness, inputBuffer, 0, ref outContext._handle, ref outputBuffer, ref attributes, out var _);
		}
		finally
		{
			if (outContext._EffectiveCredential != inCredentials && (num & 0x80000000u) == 0L)
			{
				if (outContext._EffectiveCredential != null)
				{
					outContext._EffectiveCredential.DangerousRelease();
				}
				outContext._EffectiveCredential = inCredentials;
			}
			else
			{
				inCredentials.DangerousRelease();
			}
			outContext.DangerousRelease();
		}
		if (handleTemplate != null)
		{
			handleTemplate.Set(((global::Interop.SspiCli.SecBuffer*)outputBuffer.pBuffers)->pvBuffer);
			if (handleTemplate.IsInvalid)
			{
				handleTemplate.SetHandleAsInvalid();
			}
		}
		if (inContextPtr == null && (num & 0x80000000u) != 0L)
		{
			outContext._handle.SetToInvalid();
		}
		return num;
	}

	internal unsafe static int AcceptSecurityContext(ref SafeFreeCredentials inCredentials, ref SafeDeleteContext refContext, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness endianness, SecurityBuffer inSecBuffer, SecurityBuffer[] inSecBuffers, SecurityBuffer outSecBuffer, ref global::Interop.SspiCli.ContextFlags outFlags)
	{
		if (outSecBuffer == null)
		{
			NetEventSource.Fail(null, "outSecBuffer != null", "AcceptSecurityContext");
		}
		if (inSecBuffer != null && inSecBuffers != null)
		{
			NetEventSource.Fail(null, "inSecBuffer == null || inSecBuffers == null", "AcceptSecurityContext");
		}
		if (inCredentials == null)
		{
			throw new ArgumentNullException("inCredentials");
		}
		global::Interop.SspiCli.SecBufferDesc secBufferDesc = default(global::Interop.SspiCli.SecBufferDesc);
		bool flag = false;
		if (inSecBuffer != null)
		{
			secBufferDesc = new global::Interop.SspiCli.SecBufferDesc(1);
			flag = true;
		}
		else if (inSecBuffers != null)
		{
			secBufferDesc = new global::Interop.SspiCli.SecBufferDesc(inSecBuffers.Length);
			flag = true;
		}
		global::Interop.SspiCli.SecBufferDesc outputBuffer = new global::Interop.SspiCli.SecBufferDesc(1);
		bool flag2 = (((inFlags & global::Interop.SspiCli.ContextFlags.AllocateMemory) != global::Interop.SspiCli.ContextFlags.Zero) ? true : false);
		int num = -1;
		global::Interop.SspiCli.CredHandle credHandle = default(global::Interop.SspiCli.CredHandle);
		if (refContext != null)
		{
			credHandle = refContext._handle;
		}
		GCHandle[] array = null;
		GCHandle gCHandle = default(GCHandle);
		SafeFreeContextBuffer safeFreeContextBuffer = null;
		try
		{
			gCHandle = GCHandle.Alloc(outSecBuffer.token, GCHandleType.Pinned);
			global::Interop.SspiCli.SecBuffer[] array2 = new global::Interop.SspiCli.SecBuffer[(!flag) ? 1 : secBufferDesc.cBuffers];
			fixed (global::Interop.SspiCli.SecBuffer* ptr = array2)
			{
				void* pBuffers = ptr;
				if (flag)
				{
					secBufferDesc.pBuffers = pBuffers;
					array = new GCHandle[secBufferDesc.cBuffers];
					for (int i = 0; i < secBufferDesc.cBuffers; i++)
					{
						SecurityBuffer securityBuffer = ((inSecBuffer != null) ? inSecBuffer : inSecBuffers[i]);
						if (securityBuffer != null)
						{
							array2[i].cbBuffer = securityBuffer.size;
							array2[i].BufferType = securityBuffer.type;
							if (securityBuffer.unmanagedToken != null)
							{
								array2[i].pvBuffer = securityBuffer.unmanagedToken.DangerousGetHandle();
								continue;
							}
							if (securityBuffer.token == null || securityBuffer.token.Length == 0)
							{
								array2[i].pvBuffer = IntPtr.Zero;
								continue;
							}
							array[i] = GCHandle.Alloc(securityBuffer.token, GCHandleType.Pinned);
							array2[i].pvBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(securityBuffer.token, securityBuffer.offset);
						}
					}
				}
				global::Interop.SspiCli.SecBuffer[] array3 = new global::Interop.SspiCli.SecBuffer[1];
				fixed (global::Interop.SspiCli.SecBuffer* ptr2 = &array3[0])
				{
					void* pBuffers2 = ptr2;
					outputBuffer.pBuffers = pBuffers2;
					array3[0].cbBuffer = outSecBuffer.size;
					array3[0].BufferType = outSecBuffer.type;
					if (outSecBuffer.token == null || outSecBuffer.token.Length == 0)
					{
						array3[0].pvBuffer = IntPtr.Zero;
					}
					else
					{
						array3[0].pvBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(outSecBuffer.token, outSecBuffer.offset);
					}
					if (flag2)
					{
						safeFreeContextBuffer = SafeFreeContextBuffer.CreateEmptyHandle();
					}
					if (refContext == null || refContext.IsInvalid)
					{
						refContext = new SafeDeleteContext_SECURITY();
					}
					num = MustRunAcceptSecurityContext_SECURITY(ref inCredentials, credHandle.IsZero ? null : (&credHandle), flag ? (&secBufferDesc) : null, inFlags, endianness, refContext, ref outputBuffer, ref outFlags, safeFreeContextBuffer);
					if (NetEventSource.IsEnabled)
					{
						NetEventSource.Info(null, "Marshaling OUT buffer", "AcceptSecurityContext");
					}
					outSecBuffer.size = array3[0].cbBuffer;
					outSecBuffer.type = array3[0].BufferType;
					if (outSecBuffer.size > 0)
					{
						outSecBuffer.token = new byte[outSecBuffer.size];
						Marshal.Copy(array3[0].pvBuffer, outSecBuffer.token, 0, outSecBuffer.size);
					}
					else
					{
						outSecBuffer.token = null;
					}
				}
			}
		}
		finally
		{
			if (array != null)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].IsAllocated)
					{
						array[j].Free();
					}
				}
			}
			if (gCHandle.IsAllocated)
			{
				gCHandle.Free();
			}
			safeFreeContextBuffer?.Dispose();
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Exit(null, $"errorCode:0x{num:x8}, refContext:{refContext}", "AcceptSecurityContext");
		}
		return num;
	}

	private unsafe static int MustRunAcceptSecurityContext_SECURITY(ref SafeFreeCredentials inCredentials, void* inContextPtr, global::Interop.SspiCli.SecBufferDesc* inputBuffer, global::Interop.SspiCli.ContextFlags inFlags, global::Interop.SspiCli.Endianness endianness, SafeDeleteContext outContext, ref global::Interop.SspiCli.SecBufferDesc outputBuffer, ref global::Interop.SspiCli.ContextFlags outFlags, SafeFreeContextBuffer handleTemplate)
	{
		int num = -2146893055;
		try
		{
			bool success = false;
			inCredentials.DangerousAddRef(ref success);
			outContext.DangerousAddRef(ref success);
			global::Interop.SspiCli.CredHandle credentialHandle = inCredentials._handle;
			num = global::Interop.SspiCli.AcceptSecurityContext(ref credentialHandle, inContextPtr, inputBuffer, inFlags, endianness, ref outContext._handle, ref outputBuffer, ref outFlags, out var _);
		}
		finally
		{
			if (outContext._EffectiveCredential != inCredentials && (num & 0x80000000u) == 0L)
			{
				if (outContext._EffectiveCredential != null)
				{
					outContext._EffectiveCredential.DangerousRelease();
				}
				outContext._EffectiveCredential = inCredentials;
			}
			else
			{
				inCredentials.DangerousRelease();
			}
			outContext.DangerousRelease();
		}
		if (handleTemplate != null)
		{
			handleTemplate.Set(((global::Interop.SspiCli.SecBuffer*)outputBuffer.pBuffers)->pvBuffer);
			if (handleTemplate.IsInvalid)
			{
				handleTemplate.SetHandleAsInvalid();
			}
		}
		if (inContextPtr == null && (num & 0x80000000u) != 0L)
		{
			outContext._handle.SetToInvalid();
		}
		return num;
	}

	internal unsafe static int CompleteAuthToken(ref SafeDeleteContext refContext, SecurityBuffer[] inSecBuffers)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(null, "SafeDeleteContext::CompleteAuthToken", "CompleteAuthToken");
			NetEventSource.Info(null, $"    refContext       = {refContext}", "CompleteAuthToken");
			NetEventSource.Info(null, $"    inSecBuffers[]   = {inSecBuffers}", "CompleteAuthToken");
		}
		if (inSecBuffers == null)
		{
			NetEventSource.Fail(null, "inSecBuffers == null", "CompleteAuthToken");
		}
		global::Interop.SspiCli.SecBufferDesc inputBuffers = new global::Interop.SspiCli.SecBufferDesc(inSecBuffers.Length);
		int num = -2146893055;
		GCHandle[] array = null;
		global::Interop.SspiCli.SecBuffer[] array2 = new global::Interop.SspiCli.SecBuffer[inputBuffers.cBuffers];
		fixed (global::Interop.SspiCli.SecBuffer* ptr = array2)
		{
			void* pBuffers = ptr;
			inputBuffers.pBuffers = pBuffers;
			array = new GCHandle[inputBuffers.cBuffers];
			for (int i = 0; i < inputBuffers.cBuffers; i++)
			{
				SecurityBuffer securityBuffer = inSecBuffers[i];
				if (securityBuffer != null)
				{
					array2[i].cbBuffer = securityBuffer.size;
					array2[i].BufferType = securityBuffer.type;
					if (securityBuffer.unmanagedToken != null)
					{
						array2[i].pvBuffer = securityBuffer.unmanagedToken.DangerousGetHandle();
						continue;
					}
					if (securityBuffer.token == null || securityBuffer.token.Length == 0)
					{
						array2[i].pvBuffer = IntPtr.Zero;
						continue;
					}
					array[i] = GCHandle.Alloc(securityBuffer.token, GCHandleType.Pinned);
					array2[i].pvBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(securityBuffer.token, securityBuffer.offset);
				}
			}
			global::Interop.SspiCli.CredHandle credHandle = default(global::Interop.SspiCli.CredHandle);
			if (refContext != null)
			{
				credHandle = refContext._handle;
			}
			try
			{
				if (refContext == null || refContext.IsInvalid)
				{
					refContext = new SafeDeleteContext_SECURITY();
				}
				try
				{
					bool success = false;
					refContext.DangerousAddRef(ref success);
					num = global::Interop.SspiCli.CompleteAuthToken(credHandle.IsZero ? null : (&credHandle), ref inputBuffers);
				}
				finally
				{
					refContext.DangerousRelease();
				}
			}
			finally
			{
				if (array != null)
				{
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j].IsAllocated)
						{
							array[j].Free();
						}
					}
				}
			}
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Exit(null, $"unmanaged CompleteAuthToken() errorCode:0x{num:x8} refContext:{refContext}", "CompleteAuthToken");
		}
		return num;
	}

	internal unsafe static int ApplyControlToken(ref SafeDeleteContext refContext, SecurityBuffer[] inSecBuffers)
	{
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Enter(null, null, "ApplyControlToken");
			NetEventSource.Info(null, $"    refContext       = {refContext}", "ApplyControlToken");
			NetEventSource.Info(null, $"    inSecBuffers[]   = length:{inSecBuffers.Length}", "ApplyControlToken");
		}
		if (inSecBuffers == null)
		{
			NetEventSource.Fail(null, "inSecBuffers == null", "ApplyControlToken");
		}
		global::Interop.SspiCli.SecBufferDesc inputBuffers = new global::Interop.SspiCli.SecBufferDesc(inSecBuffers.Length);
		int num = -2146893055;
		GCHandle[] array = null;
		global::Interop.SspiCli.SecBuffer[] array2 = new global::Interop.SspiCli.SecBuffer[inputBuffers.cBuffers];
		fixed (global::Interop.SspiCli.SecBuffer* ptr = array2)
		{
			void* pBuffers = ptr;
			inputBuffers.pBuffers = pBuffers;
			array = new GCHandle[inputBuffers.cBuffers];
			for (int i = 0; i < inputBuffers.cBuffers; i++)
			{
				SecurityBuffer securityBuffer = inSecBuffers[i];
				if (securityBuffer != null)
				{
					array2[i].cbBuffer = securityBuffer.size;
					array2[i].BufferType = securityBuffer.type;
					if (securityBuffer.unmanagedToken != null)
					{
						array2[i].pvBuffer = securityBuffer.unmanagedToken.DangerousGetHandle();
						continue;
					}
					if (securityBuffer.token == null || securityBuffer.token.Length == 0)
					{
						array2[i].pvBuffer = IntPtr.Zero;
						continue;
					}
					array[i] = GCHandle.Alloc(securityBuffer.token, GCHandleType.Pinned);
					array2[i].pvBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(securityBuffer.token, securityBuffer.offset);
				}
			}
			global::Interop.SspiCli.CredHandle credHandle = default(global::Interop.SspiCli.CredHandle);
			if (refContext != null)
			{
				credHandle = refContext._handle;
			}
			try
			{
				if (refContext == null || refContext.IsInvalid)
				{
					refContext = new SafeDeleteContext_SECURITY();
				}
				try
				{
					bool success = false;
					refContext.DangerousAddRef(ref success);
					num = global::Interop.SspiCli.ApplyControlToken(credHandle.IsZero ? null : (&credHandle), ref inputBuffers);
				}
				finally
				{
					refContext.DangerousRelease();
				}
			}
			finally
			{
				if (array != null)
				{
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j].IsAllocated)
						{
							array[j].Free();
						}
					}
				}
			}
		}
		if (NetEventSource.IsEnabled)
		{
			NetEventSource.Exit(null, $"unmanaged ApplyControlToken() errorCode:0x{num:x8} refContext: {refContext}", "ApplyControlToken");
		}
		return num;
	}
}
