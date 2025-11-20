using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace MonoMod.Utils.Cil;

public sealed class CecilILGenerator : ILGeneratorShim
{
	private class ExceptionHandlerChain
	{
		private readonly CecilILGenerator IL;

		private readonly Instruction _Start;

		public readonly Label SkipAll;

		private readonly Instruction _SkipAllI;

		private Label _SkipHandler;

		private ExceptionHandler _Prev;

		private ExceptionHandler _Handler;

		public ExceptionHandlerChain(CecilILGenerator il)
		{
			IL = il;
			_Start = il.MarkLabel();
			SkipAll = il.DefineLabel();
			_SkipAllI = il._(SkipAll);
		}

		public ExceptionHandler BeginHandler(ExceptionHandlerType type)
		{
			ExceptionHandler exceptionHandler = (_Prev = _Handler);
			if (exceptionHandler != null)
			{
				EndHandler(exceptionHandler);
			}
			IL.Emit(System.Reflection.Emit.OpCodes.Leave, _SkipHandler = IL.DefineLabel());
			ExceptionHandler exceptionHandler2 = (_Handler = new ExceptionHandler(ExceptionHandlerType.Catch));
			Instruction instruction = IL.MarkLabel();
			exceptionHandler2.TryStart = _Start;
			exceptionHandler2.TryEnd = instruction;
			exceptionHandler2.HandlerType = type;
			if (type == ExceptionHandlerType.Filter)
			{
				exceptionHandler2.FilterStart = instruction;
			}
			else
			{
				exceptionHandler2.HandlerStart = instruction;
			}
			IL.IL.Body.ExceptionHandlers.Add(exceptionHandler2);
			return exceptionHandler2;
		}

		public void EndHandler(ExceptionHandler handler)
		{
			Label skipHandler = _SkipHandler;
			switch (handler.HandlerType)
			{
			case ExceptionHandlerType.Filter:
				IL.Emit(System.Reflection.Emit.OpCodes.Endfilter);
				break;
			case ExceptionHandlerType.Finally:
				IL.Emit(System.Reflection.Emit.OpCodes.Endfinally);
				break;
			default:
				IL.Emit(System.Reflection.Emit.OpCodes.Leave, skipHandler);
				break;
			}
			IL.MarkLabel(skipHandler);
			handler.HandlerEnd = IL._(skipHandler);
		}

		public void End()
		{
			EndHandler(_Handler);
			IL.MarkLabel(SkipAll);
		}
	}

	private static readonly ConstructorInfo c_LocalBuilder;

	private static ParameterInfo[] c_LocalBuilder_params;

	private static readonly Dictionary<short, Mono.Cecil.Cil.OpCode> _MCCOpCodes;

	public readonly ILProcessor IL;

	private readonly List<Instruction> _Labels = new List<Instruction>();

	private readonly Dictionary<LocalBuilder, VariableDefinition> _Variables = new Dictionary<LocalBuilder, VariableDefinition>();

	private readonly Stack<ExceptionHandlerChain> _ExceptionHandlers = new Stack<ExceptionHandlerChain>();

	public override int ILOffset
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	static CecilILGenerator()
	{
		c_LocalBuilder = typeof(LocalBuilder).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
		c_LocalBuilder_params = c_LocalBuilder.GetParameters();
		_MCCOpCodes = new Dictionary<short, Mono.Cecil.Cil.OpCode>();
		FieldInfo[] fields = typeof(Mono.Cecil.Cil.OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			Mono.Cecil.Cil.OpCode value = (Mono.Cecil.Cil.OpCode)fields[i].GetValue(null);
			_MCCOpCodes[value.Value] = value;
		}
	}

	public CecilILGenerator(ILProcessor il)
	{
		IL = il;
	}

	private Mono.Cecil.Cil.OpCode _(System.Reflection.Emit.OpCode opcode)
	{
		return _MCCOpCodes[opcode.Value];
	}

	private unsafe Instruction _(Label handle)
	{
		return _Labels[*(int*)(&handle)];
	}

	private VariableDefinition _(LocalBuilder handle)
	{
		return _Variables[handle];
	}

	private TypeReference _(Type info)
	{
		return IL.Body.Method.Module.ImportReference(info);
	}

	private FieldReference _(FieldInfo info)
	{
		return IL.Body.Method.Module.ImportReference(info);
	}

	private MethodReference _(MethodBase info)
	{
		return IL.Body.Method.Module.ImportReference(info);
	}

	public unsafe override Label DefineLabel()
	{
		Label result = default(Label);
		*(int*)(&result) = _Labels.Count;
		Instruction item = IL.Create(Mono.Cecil.Cil.OpCodes.Nop);
		_Labels.Add(item);
		return result;
	}

	public override void MarkLabel(Label loc)
	{
		MarkLabel(_(loc));
	}

	private Instruction MarkLabel()
	{
		return MarkLabel(_(DefineLabel()));
	}

	private Instruction MarkLabel(Instruction instr)
	{
		Collection<Instruction> instructions = IL.Body.Instructions;
		int num = instructions.IndexOf(instr);
		if (num != -1)
		{
			instructions.RemoveAt(num);
		}
		IL.Append(instr);
		return instr;
	}

	public override LocalBuilder DeclareLocal(Type type)
	{
		return DeclareLocal(type, pinned: false);
	}

	public override LocalBuilder DeclareLocal(Type type, bool pinned)
	{
		object obj;
		if (c_LocalBuilder_params.Length != 4)
		{
			if (c_LocalBuilder_params.Length != 3)
			{
				if (c_LocalBuilder_params.Length != 2)
				{
					if (c_LocalBuilder_params.Length != 0)
					{
						throw new NotSupportedException();
					}
					obj = c_LocalBuilder.Invoke(new object[0]);
				}
				else
				{
					obj = c_LocalBuilder.Invoke(new object[2] { type, null });
				}
			}
			else
			{
				obj = c_LocalBuilder.Invoke(new object[3] { 0, type, null });
			}
		}
		else
		{
			obj = c_LocalBuilder.Invoke(new object[4] { 0, type, null, false });
		}
		LocalBuilder localBuilder = (LocalBuilder)obj;
		TypeReference typeReference = _(type);
		if (pinned)
		{
			typeReference = new PinnedType(typeReference);
		}
		VariableDefinition variableDefinition = new VariableDefinition(typeReference);
		IL.Body.Variables.Add(variableDefinition);
		_Variables[localBuilder] = variableDefinition;
		return localBuilder;
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode)
	{
		IL.Emit(_(opcode));
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, byte arg)
	{
		IL.Emit(_(opcode), arg);
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, sbyte arg)
	{
		IL.Emit(_(opcode), arg);
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, short arg)
	{
		IL.Emit(_(opcode), arg);
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, int arg)
	{
		if (opcode.Name.EndsWith(".s"))
		{
			IL.Emit(_(opcode), (sbyte)arg);
		}
		else
		{
			IL.Emit(_(opcode), arg);
		}
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, long arg)
	{
		IL.Emit(_(opcode), arg);
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, float arg)
	{
		IL.Emit(_(opcode), arg);
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, double arg)
	{
		IL.Emit(_(opcode), arg);
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, string arg)
	{
		IL.Emit(_(opcode), arg);
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, Type arg)
	{
		IL.Emit(_(opcode), _(arg));
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, FieldInfo arg)
	{
		IL.Emit(_(opcode), _(arg));
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, ConstructorInfo arg)
	{
		IL.Emit(_(opcode), _(arg));
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, MethodInfo arg)
	{
		IL.Emit(_(opcode), _(arg));
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, Label label)
	{
		IL.Emit(_(opcode), _(label));
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, Label[] labels)
	{
		IL.Emit(_(opcode), labels.Select((Label label) => _(label)).ToArray());
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, LocalBuilder local)
	{
		IL.Emit(_(opcode), _(local));
	}

	public override void Emit(System.Reflection.Emit.OpCode opcode, SignatureHelper signature)
	{
		throw new NotSupportedException();
	}

	public override void EmitCall(System.Reflection.Emit.OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes)
	{
		IL.Emit(_(opcode), _(methodInfo));
	}

	public override void EmitCalli(System.Reflection.Emit.OpCode opcode, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
	{
		throw new NotSupportedException();
	}

	public override void EmitCalli(System.Reflection.Emit.OpCode opcode, CallingConvention unmanagedCallConv, Type returnType, Type[] parameterTypes)
	{
		throw new NotSupportedException();
	}

	public override void EmitWriteLine(FieldInfo field)
	{
		if (field.IsStatic)
		{
			IL.Emit(Mono.Cecil.Cil.OpCodes.Ldsfld, _(field));
		}
		else
		{
			IL.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
			IL.Emit(Mono.Cecil.Cil.OpCodes.Ldfld, _(field));
		}
		IL.Emit(Mono.Cecil.Cil.OpCodes.Call, _(typeof(Console).GetMethod("WriteLine", new Type[1] { field.FieldType })));
	}

	public override void EmitWriteLine(LocalBuilder localBuilder)
	{
		IL.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, _(localBuilder));
		IL.Emit(Mono.Cecil.Cil.OpCodes.Call, _(typeof(Console).GetMethod("WriteLine", new Type[1] { localBuilder.LocalType })));
	}

	public override void EmitWriteLine(string value)
	{
		IL.Emit(Mono.Cecil.Cil.OpCodes.Ldstr, value);
		IL.Emit(Mono.Cecil.Cil.OpCodes.Call, _(typeof(Console).GetMethod("WriteLine", new Type[1] { typeof(string) })));
	}

	public override void ThrowException(Type type)
	{
		IL.Emit(Mono.Cecil.Cil.OpCodes.Newobj, _(type.GetConstructor(Type.EmptyTypes)));
		IL.Emit(Mono.Cecil.Cil.OpCodes.Throw);
	}

	public override Label BeginExceptionBlock()
	{
		ExceptionHandlerChain exceptionHandlerChain = new ExceptionHandlerChain(this);
		_ExceptionHandlers.Push(exceptionHandlerChain);
		return exceptionHandlerChain.SkipAll;
	}

	public override void BeginCatchBlock(Type exceptionType)
	{
		_ExceptionHandlers.Peek().BeginHandler(ExceptionHandlerType.Catch).CatchType = ((exceptionType == null) ? null : _(exceptionType));
	}

	public override void BeginExceptFilterBlock()
	{
		_ExceptionHandlers.Peek().BeginHandler(ExceptionHandlerType.Filter);
	}

	public override void BeginFaultBlock()
	{
		_ExceptionHandlers.Peek().BeginHandler(ExceptionHandlerType.Fault);
	}

	public override void BeginFinallyBlock()
	{
		_ExceptionHandlers.Peek().BeginHandler(ExceptionHandlerType.Finally);
	}

	public override void EndExceptionBlock()
	{
		_ExceptionHandlers.Pop().End();
	}

	public override void BeginScope()
	{
	}

	public override void EndScope()
	{
	}

	public override void UsingNamespace(string usingNamespace)
	{
	}
}
