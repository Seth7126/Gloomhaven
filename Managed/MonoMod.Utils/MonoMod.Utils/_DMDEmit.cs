using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MonoMod.Utils;

internal static class _DMDEmit
{
	private static readonly MethodInfo m_MethodBase_InvokeSimple;

	private static readonly Dictionary<short, System.Reflection.Emit.OpCode> _ReflOpCodes;

	private static readonly Dictionary<short, Mono.Cecil.Cil.OpCode> _CecilOpCodes;

	private static readonly Dictionary<Type, MethodInfo> _Emitters;

	private static readonly MethodInfo _ILGen_make_room;

	private static readonly MethodInfo _ILGen_emit_int;

	private static readonly MethodInfo _ILGen_ll_emit;

	private static readonly MethodInfo _ILGen_EnsureCapacity;

	private static readonly MethodInfo _ILGen_PutInteger4;

	private static readonly MethodInfo _ILGen_InternalEmit;

	private static readonly MethodInfo _ILGen_UpdateStackSize;

	private static readonly FieldInfo f_DynILGen_m_scope;

	private static readonly FieldInfo f_DynScope_m_tokens;

	private static readonly Type[] CorElementTypes;

	private static MethodBuilder _CreateMethodProxy(MethodBuilder context, DynamicMethod target)
	{
		TypeBuilder obj = (TypeBuilder)context.DeclaringType;
		string name = $".dmdproxy<{target.Name.Replace('.', '_')}>?{target.GetHashCode()}";
		Type[] array = (from param in target.GetParameters()
			select param.ParameterType).ToArray();
		MethodBuilder methodBuilder = obj.DefineMethod(name, System.Reflection.MethodAttributes.Private | System.Reflection.MethodAttributes.Static | System.Reflection.MethodAttributes.HideBySig, CallingConventions.Standard, target.ReturnType, array);
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		iLGenerator.EmitReference(target);
		iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldnull);
		iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, array.Length);
		iLGenerator.Emit(System.Reflection.Emit.OpCodes.Newarr, typeof(object));
		for (int num = 0; num < array.Length; num++)
		{
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Dup);
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, num);
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg, num);
			Type type = array[num];
			if (type.IsByRef)
			{
				type = type.GetElementType();
			}
			if (type.IsValueType)
			{
				iLGenerator.Emit(System.Reflection.Emit.OpCodes.Box, type);
			}
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Stelem_Ref);
		}
		iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, m_MethodBase_InvokeSimple);
		if (target.ReturnType == typeof(void))
		{
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Pop);
		}
		else if (target.ReturnType.IsValueType)
		{
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, target.ReturnType);
		}
		iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);
		return methodBuilder;
	}

	static _DMDEmit()
	{
		m_MethodBase_InvokeSimple = typeof(MethodBase).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public, null, new Type[2]
		{
			typeof(object),
			typeof(object[])
		}, null);
		_ReflOpCodes = new Dictionary<short, System.Reflection.Emit.OpCode>();
		_CecilOpCodes = new Dictionary<short, Mono.Cecil.Cil.OpCode>();
		_Emitters = new Dictionary<Type, MethodInfo>();
		_ILGen_make_room = typeof(ILGenerator).GetMethod("make_room", BindingFlags.Instance | BindingFlags.NonPublic);
		_ILGen_emit_int = typeof(ILGenerator).GetMethod("emit_int", BindingFlags.Instance | BindingFlags.NonPublic);
		_ILGen_ll_emit = typeof(ILGenerator).GetMethod("ll_emit", BindingFlags.Instance | BindingFlags.NonPublic);
		_ILGen_EnsureCapacity = typeof(ILGenerator).GetMethod("EnsureCapacity", BindingFlags.Instance | BindingFlags.NonPublic);
		_ILGen_PutInteger4 = typeof(ILGenerator).GetMethod("PutInteger4", BindingFlags.Instance | BindingFlags.NonPublic);
		_ILGen_InternalEmit = typeof(ILGenerator).GetMethod("InternalEmit", BindingFlags.Instance | BindingFlags.NonPublic);
		_ILGen_UpdateStackSize = typeof(ILGenerator).GetMethod("UpdateStackSize", BindingFlags.Instance | BindingFlags.NonPublic);
		f_DynILGen_m_scope = typeof(ILGenerator).Assembly.GetType("System.Reflection.Emit.DynamicILGenerator")?.GetField("m_scope", BindingFlags.Instance | BindingFlags.NonPublic);
		f_DynScope_m_tokens = typeof(ILGenerator).Assembly.GetType("System.Reflection.Emit.DynamicScope")?.GetField("m_tokens", BindingFlags.Instance | BindingFlags.NonPublic);
		CorElementTypes = new Type[14]
		{
			null,
			typeof(void),
			typeof(bool),
			typeof(char),
			typeof(sbyte),
			typeof(byte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(string),
			typeof(IntPtr)
		};
		FieldInfo[] fields = typeof(System.Reflection.Emit.OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			System.Reflection.Emit.OpCode value = (System.Reflection.Emit.OpCode)fields[i].GetValue(null);
			_ReflOpCodes[value.Value] = value;
		}
		fields = typeof(Mono.Cecil.Cil.OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			Mono.Cecil.Cil.OpCode value2 = (Mono.Cecil.Cil.OpCode)fields[i].GetValue(null);
			_CecilOpCodes[value2.Value] = value2;
		}
		MethodInfo[] methods = typeof(ILGenerator).GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			if (!(methodInfo.Name != "Emit"))
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length == 2 && parameters[0].ParameterType == typeof(System.Reflection.Emit.OpCode))
				{
					_Emitters[parameters[1].ParameterType] = methodInfo;
				}
			}
		}
	}

	public static void Generate(DynamicMethodDefinition dmd, MethodBase _mb, ILGenerator il)
	{
		MethodDefinition definition = dmd.Definition;
		DynamicMethod dynamicMethod = _mb as DynamicMethod;
		MethodBuilder mb = _mb as MethodBuilder;
		ModuleBuilder moduleBuilder = mb?.Module as ModuleBuilder;
		AssemblyBuilder assemblyBuilder = (mb?.DeclaringType as TypeBuilder)?.Assembly as AssemblyBuilder;
		HashSet<Assembly> hashSet = null;
		if (mb != null)
		{
			hashSet = new HashSet<Assembly>();
		}
		MethodDebugInformation defInfo = (dmd.Debug ? definition.DebugInformation : null);
		LocalBuilder[] array = definition.Body.Variables.Select(delegate(VariableDefinition var)
		{
			LocalBuilder localBuilder = il.DeclareLocal(var.VariableType.ResolveReflection(), var.IsPinned);
			if (mb != null && defInfo != null && defInfo.TryGetName(var, out var name))
			{
				localBuilder.SetLocalSymInfo(name);
			}
			return localBuilder;
		}).ToArray();
		Dictionary<Instruction, Label> labelMap = new Dictionary<Instruction, Label>();
		foreach (Instruction instruction in definition.Body.Instructions)
		{
			if (instruction.Operand is Instruction[] array2)
			{
				Instruction[] array3 = array2;
				foreach (Instruction key in array3)
				{
					if (!labelMap.ContainsKey(key))
					{
						labelMap[key] = il.DefineLabel();
					}
				}
			}
			else if (instruction.Operand is Instruction key2 && !labelMap.ContainsKey(key2))
			{
				labelMap[key2] = il.DefineLabel();
			}
		}
		Dictionary<Document, ISymbolDocumentWriter> dictionary = ((mb == null) ? null : new Dictionary<Document, ISymbolDocumentWriter>());
		int num2 = (definition.HasThis ? 1 : 0);
		object[] array4 = new object[2];
		bool flag = false;
		foreach (Instruction instruction2 in definition.Body.Instructions)
		{
			if (labelMap.TryGetValue(instruction2, out var value))
			{
				il.MarkLabel(value);
			}
			SequencePoint sequencePoint = defInfo?.GetSequencePoint(instruction2);
			if (mb != null && sequencePoint != null)
			{
				if (!dictionary.TryGetValue(sequencePoint.Document, out var value2))
				{
					value2 = (dictionary[sequencePoint.Document] = moduleBuilder.DefineDocument(sequencePoint.Document.Url, sequencePoint.Document.LanguageGuid, sequencePoint.Document.LanguageVendorGuid, sequencePoint.Document.TypeGuid));
				}
				il.MarkSequencePoint(value2, sequencePoint.StartLine, sequencePoint.StartColumn, sequencePoint.EndLine, sequencePoint.EndColumn);
			}
			foreach (ExceptionHandler exceptionHandler in definition.Body.ExceptionHandlers)
			{
				if (flag && exceptionHandler.HandlerEnd == instruction2)
				{
					il.EndExceptionBlock();
				}
				if (exceptionHandler.TryStart == instruction2)
				{
					il.BeginExceptionBlock();
				}
				else if (exceptionHandler.FilterStart == instruction2)
				{
					il.BeginExceptFilterBlock();
				}
				else if (exceptionHandler.HandlerStart == instruction2)
				{
					switch (exceptionHandler.HandlerType)
					{
					case ExceptionHandlerType.Filter:
						il.BeginCatchBlock(null);
						break;
					case ExceptionHandlerType.Catch:
						il.BeginCatchBlock(exceptionHandler.CatchType.ResolveReflection());
						break;
					case ExceptionHandlerType.Finally:
						il.BeginFinallyBlock();
						break;
					case ExceptionHandlerType.Fault:
						il.BeginFaultBlock();
						break;
					}
				}
				if (exceptionHandler.HandlerStart != instruction2.Next)
				{
					continue;
				}
				switch (exceptionHandler.HandlerType)
				{
				case ExceptionHandlerType.Filter:
					if (!(instruction2.OpCode == Mono.Cecil.Cil.OpCodes.Endfilter))
					{
						break;
					}
					goto IL_07fd;
				case ExceptionHandlerType.Finally:
					if (!(instruction2.OpCode == Mono.Cecil.Cil.OpCodes.Endfinally))
					{
						break;
					}
					goto IL_07fd;
				}
			}
			if (instruction2.OpCode.OperandType == Mono.Cecil.Cil.OperandType.InlineNone)
			{
				il.Emit(_ReflOpCodes[instruction2.OpCode.Value]);
			}
			else
			{
				object obj = instruction2.Operand;
				if (obj is Instruction[] source)
				{
					obj = source.Select((Instruction key4) => labelMap[key4]).ToArray();
					instruction2.OpCode = instruction2.OpCode.ToLongOp();
				}
				else if (obj is Instruction key3)
				{
					obj = labelMap[key3];
					instruction2.OpCode = instruction2.OpCode.ToLongOp();
				}
				else if (obj is VariableDefinition variableDefinition)
				{
					obj = array[variableDefinition.Index];
				}
				else if (obj is ParameterDefinition parameterDefinition)
				{
					obj = parameterDefinition.Index + num2;
				}
				else if (obj is MemberReference memberReference)
				{
					if (memberReference is DynamicMethodReference dynamicMethodReference)
					{
						obj = dynamicMethodReference.DynamicMethod;
					}
					else
					{
						MemberInfo memberInfo = memberReference.ResolveReflection();
						obj = memberInfo;
						if (mb != null && memberInfo != null)
						{
							Assembly assembly = memberInfo.Module.Assembly;
							if (!hashSet.Contains(assembly))
							{
								assemblyBuilder.SetCustomAttribute(new CustomAttributeBuilder(DynamicMethodDefinition.c_IgnoresAccessChecksToAttribute, new object[1] { assembly.GetName().Name }));
								hashSet.Add(assembly);
							}
						}
					}
				}
				else if (obj is CallSite csite)
				{
					if (dynamicMethod != null)
					{
						_EmitCallSite(dynamicMethod, il, _ReflOpCodes[instruction2.OpCode.Value], csite);
						continue;
					}
					obj = csite.ResolveReflection(mb.Module);
				}
				if (mb != null && obj is MethodBase { DeclaringType: null } methodBase)
				{
					if (!(instruction2.OpCode == Mono.Cecil.Cil.OpCodes.Call))
					{
						throw new NotSupportedException("Unsupported global method operand on opcode " + instruction2.OpCode.Name);
					}
					if (obj is DynamicMethod target)
					{
						obj = _CreateMethodProxy(mb, target);
					}
					else
					{
						IntPtr ldftnPointer = methodBase.GetLdftnPointer();
						if (IntPtr.Size == 4)
						{
							il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, (int)ldftnPointer);
						}
						else
						{
							il.Emit(System.Reflection.Emit.OpCodes.Ldc_I8, (long)ldftnPointer);
						}
						il.Emit(System.Reflection.Emit.OpCodes.Conv_I);
						instruction2.OpCode = Mono.Cecil.Cil.OpCodes.Calli;
						obj = ((MethodReference)instruction2.Operand).ResolveReflectionSignature(mb.Module);
					}
				}
				if (obj == null)
				{
					throw new NullReferenceException($"Unexpected null in {definition} @ {instruction2}");
				}
				Type operandType = obj.GetType();
				if (!_Emitters.TryGetValue(operandType, out var value3))
				{
					value3 = _Emitters.FirstOrDefault((KeyValuePair<Type, MethodInfo> kvp) => kvp.Key.IsAssignableFrom(operandType)).Value;
				}
				if (value3 == null)
				{
					throw new InvalidOperationException($"Unexpected unemittable {obj.GetType().FullName} in {definition} @ {instruction2}");
				}
				array4[0] = _ReflOpCodes[instruction2.OpCode.Value];
				array4[1] = obj;
				value3.Invoke(il, array4);
			}
			if (!flag)
			{
				foreach (ExceptionHandler exceptionHandler2 in definition.Body.ExceptionHandlers)
				{
					if (exceptionHandler2.HandlerEnd == instruction2.Next)
					{
						il.EndExceptionBlock();
					}
				}
			}
			flag = false;
			continue;
			IL_07fd:
			flag = true;
		}
	}

	public static void ResolveWithModifiers(TypeReference typeRef, out Type type, out Type[] typeModReq, out Type[] typeModOpt, List<Type> modReq = null, List<Type> modOpt = null)
	{
		if (modReq == null)
		{
			modReq = new List<Type>();
		}
		else
		{
			modReq.Clear();
		}
		if (modOpt == null)
		{
			modOpt = new List<Type>();
		}
		else
		{
			modOpt.Clear();
		}
		for (TypeReference typeReference = typeRef; typeReference is TypeSpecification typeSpecification; typeReference = typeSpecification.ElementType)
		{
			if (!(typeReference is RequiredModifierType requiredModifierType))
			{
				if (typeReference is OptionalModifierType optionalModifierType)
				{
					modOpt.Add(optionalModifierType.ModifierType.ResolveReflection());
				}
			}
			else
			{
				modReq.Add(requiredModifierType.ModifierType.ResolveReflection());
			}
		}
		type = typeRef.ResolveReflection();
		typeModReq = modReq.ToArray();
		typeModOpt = modOpt.ToArray();
	}

	internal static void _EmitCallSite(DynamicMethod dm, ILGenerator il, System.Reflection.Emit.OpCode opcode, CallSite csite)
	{
		List<object> _tokens = null;
		DynamicILInfo _info = null;
		if (DynamicMethodDefinition._IsMono)
		{
			_info = dm.GetDynamicILInfo();
		}
		else
		{
			_tokens = f_DynScope_m_tokens.GetValue(f_DynILGen_m_scope.GetValue(il)) as List<object>;
		}
		byte[] signature = new byte[32];
		int currSig = 0;
		int num = -1;
		AddData((int)csite.CallingConvention);
		num = currSig++;
		List<Type> modReq = new List<Type>();
		List<Type> modOpt = new List<Type>();
		ResolveWithModifiers(csite.ReturnType, out var type, out var typeModReq, out var typeModOpt, modReq, modOpt);
		AddArgument(type, typeModReq, typeModOpt);
		foreach (ParameterDefinition parameter in csite.Parameters)
		{
			if (parameter.ParameterType.IsSentinel)
			{
				AddElementType(65);
			}
			if (parameter.ParameterType.IsPinned)
			{
				AddElementType(69);
			}
			ResolveWithModifiers(parameter.ParameterType, out var type2, out var typeModReq2, out var typeModOpt2, modReq, modOpt);
			AddArgument(type2, typeModReq2, typeModOpt2);
		}
		AddElementType(0);
		int num2 = currSig;
		int num3 = ((csite.Parameters.Count < 128) ? 1 : ((csite.Parameters.Count >= 16384) ? 4 : 2));
		byte[] array = new byte[currSig + num3 - 1];
		array[0] = signature[0];
		Buffer.BlockCopy(signature, num + 1, array, num + num3, num2 - (num + 1));
		signature = array;
		currSig = num;
		AddData(csite.Parameters.Count);
		currSig = num2 + (num3 - 1);
		if (signature.Length > currSig)
		{
			array = new byte[currSig];
			Array.Copy(signature, array, currSig);
			signature = array;
		}
		if (_ILGen_emit_int != null)
		{
			_ILGen_make_room.Invoke(il, new object[1] { 6 });
			_ILGen_ll_emit.Invoke(il, new object[1] { opcode });
			_ILGen_emit_int.Invoke(il, new object[1] { GetTokenForSig(signature) });
			return;
		}
		_ILGen_EnsureCapacity.Invoke(il, new object[1] { 7 });
		_ILGen_InternalEmit.Invoke(il, new object[1] { opcode });
		if (opcode.StackBehaviourPop == System.Reflection.Emit.StackBehaviour.Varpop)
		{
			_ILGen_UpdateStackSize.Invoke(il, new object[2]
			{
				opcode,
				-csite.Parameters.Count - 1
			});
		}
		_ILGen_PutInteger4.Invoke(il, new object[1] { GetTokenForSig(signature) });
		void AddArgument(Type clsArgument, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			if (optionalCustomModifiers != null)
			{
				Type[] array2 = optionalCustomModifiers;
				for (int i = 0; i < array2.Length; i++)
				{
					InternalAddTypeToken(GetTokenForType(array2[i]), 32);
				}
			}
			if (requiredCustomModifiers != null)
			{
				Type[] array2 = requiredCustomModifiers;
				for (int i = 0; i < array2.Length; i++)
				{
					InternalAddTypeToken(GetTokenForType(array2[i]), 31);
				}
			}
			AddOneArgTypeHelper(clsArgument);
		}
		void AddData(int data)
		{
			if (currSig + 4 > signature.Length)
			{
				signature = ExpandArray(signature, -1);
			}
			if (data <= 127)
			{
				signature[currSig++] = (byte)(data & 0xFF);
			}
			else if (data <= 16383)
			{
				signature[currSig++] = (byte)((data >> 8) | 0x80);
				signature[currSig++] = (byte)(data & 0xFF);
			}
			else
			{
				if (data > 536870911)
				{
					throw new ArgumentException("Integer or token was too large to be encoded.");
				}
				signature[currSig++] = (byte)((data >> 24) | 0xC0);
				signature[currSig++] = (byte)((data >> 16) & 0xFF);
				signature[currSig++] = (byte)((data >> 8) & 0xFF);
				signature[currSig++] = (byte)(data & 0xFF);
			}
		}
		void AddElementType(byte cvt)
		{
			if (currSig + 1 > signature.Length)
			{
				signature = ExpandArray(signature, -1);
			}
			signature[currSig++] = cvt;
		}
		void AddOneArgTypeHelper(Type clsArgument)
		{
			AddOneArgTypeHelperWorker(clsArgument, lastWasGenericInst: false);
		}
		void AddOneArgTypeHelperWorker(Type clsArgument, bool lastWasGenericInst)
		{
			if (clsArgument.IsGenericType && (!clsArgument.IsGenericTypeDefinition || !lastWasGenericInst))
			{
				AddElementType(21);
				AddOneArgTypeHelperWorker(clsArgument.GetGenericTypeDefinition(), lastWasGenericInst: true);
				Type[] genericArguments = clsArgument.GetGenericArguments();
				AddData(genericArguments.Length);
				Type[] array2 = genericArguments;
				for (int i = 0; i < array2.Length; i++)
				{
					AddOneArgTypeHelper(array2[i]);
				}
			}
			else if (clsArgument.IsByRef)
			{
				AddElementType(16);
				clsArgument = clsArgument.GetElementType();
				AddOneArgTypeHelper(clsArgument);
			}
			else if (clsArgument.IsPointer)
			{
				AddElementType(15);
				AddOneArgTypeHelper(clsArgument.GetElementType());
			}
			else if (clsArgument.IsArray)
			{
				AddElementType(20);
				AddOneArgTypeHelper(clsArgument.GetElementType());
				int arrayRank = clsArgument.GetArrayRank();
				AddData(arrayRank);
				AddData(0);
				AddData(arrayRank);
				for (int j = 0; j < arrayRank; j++)
				{
					AddData(0);
				}
			}
			else
			{
				byte b = 0;
				for (int k = 0; k < CorElementTypes.Length; k++)
				{
					if (clsArgument == CorElementTypes[k])
					{
						b = (byte)k;
						break;
					}
				}
				if (b == 0)
				{
					b = (byte)((clsArgument == typeof(object)) ? 28 : ((!clsArgument.IsValueType) ? 18 : 17));
				}
				if (b <= 14 || b == 22 || b == 24 || b == 25 || b == 28)
				{
					AddElementType(b);
				}
				else if (clsArgument.IsValueType)
				{
					InternalAddTypeToken(GetTokenForType(clsArgument), 17);
				}
				else
				{
					InternalAddTypeToken(GetTokenForType(clsArgument), 18);
				}
			}
		}
		void AddToken(int token)
		{
			int num4 = token & 0xFFFFFF;
			int num5 = token & -16777216;
			if (num4 > 67108863)
			{
				throw new ArgumentException("Integer or token was too large to be encoded.");
			}
			num4 <<= 2;
			switch (num5)
			{
			case 16777216:
				num4 |= 1;
				break;
			case 452984832:
				num4 |= 2;
				break;
			}
			AddData(num4);
		}
		static byte[] ExpandArray(byte[] inArray, int requiredLength)
		{
			if (requiredLength < inArray.Length)
			{
				requiredLength = inArray.Length * 2;
			}
			byte[] array2 = new byte[requiredLength];
			Buffer.BlockCopy(inArray, 0, array2, 0, inArray.Length);
			return array2;
		}
		int GetTokenForSig(byte[] v)
		{
			if (_info == null)
			{
				return _GetTokenForSig(v);
			}
			return _info.GetTokenFor(v);
		}
		int GetTokenForType(Type v)
		{
			if (_info == null)
			{
				return _GetTokenForType(v);
			}
			return _info.GetTokenFor(v.TypeHandle);
		}
		void InternalAddTypeToken(int clsToken, byte CorType)
		{
			AddElementType(CorType);
			AddToken(clsToken);
		}
		int _GetTokenForSig(byte[] v)
		{
			_tokens.Add(v);
			return (_tokens.Count - 1) | 0x11000000;
		}
		int _GetTokenForType(Type v)
		{
			_tokens.Add(v.TypeHandle);
			return (_tokens.Count - 1) | 0x2000000;
		}
	}
}
