using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MonoMod.Utils;

namespace MonoMod.Cil;

public class ILCursor
{
	private Instruction _next;

	private ILLabel[] _afterLabels;

	private SearchTarget _searchTarget;

	public ILContext Context { get; }

	public Instruction Next
	{
		get
		{
			return _next;
		}
		set
		{
			Goto(value);
		}
	}

	public Instruction Prev
	{
		get
		{
			if (Next != null)
			{
				return Next.Previous;
			}
			return Instrs[Instrs.Count - 1];
		}
		set
		{
			Goto(value, MoveType.After);
		}
	}

	public Instruction Previous
	{
		get
		{
			return Prev;
		}
		set
		{
			Prev = value;
		}
	}

	public int Index
	{
		get
		{
			return Context.IndexOf(Next);
		}
		set
		{
			Goto(value);
		}
	}

	public SearchTarget SearchTarget
	{
		get
		{
			return _searchTarget;
		}
		set
		{
			if ((value == SearchTarget.Next && Next == null) || (value == SearchTarget.Prev && Prev == null))
			{
				value = SearchTarget.None;
			}
			_searchTarget = value;
		}
	}

	public IEnumerable<ILLabel> IncomingLabels => Context.GetIncomingLabels(Next);

	public MethodDefinition Method => Context.Method;

	public ILProcessor IL => Context.IL;

	public Mono.Cecil.Cil.MethodBody Body => Context.Body;

	public ModuleDefinition Module => Context.Module;

	public Collection<Instruction> Instrs => Context.Instrs;

	public ILCursor(ILContext context)
	{
		Context = context;
		Index = 0;
	}

	public ILCursor(ILCursor c)
	{
		Context = c.Context;
		_next = c._next;
		_searchTarget = c._searchTarget;
		_afterLabels = c._afterLabels;
	}

	public ILCursor Clone()
	{
		return new ILCursor(this);
	}

	public bool IsBefore(Instruction instr)
	{
		return Index <= Context.IndexOf(instr);
	}

	public bool IsAfter(Instruction instr)
	{
		return Index > Context.IndexOf(instr);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"// ILCursor: {Method}, {Index}, {SearchTarget}");
		ILContext.ToString(stringBuilder, Prev);
		ILContext.ToString(stringBuilder, Next);
		return stringBuilder.ToString();
	}

	public ILCursor Goto(Instruction insn, MoveType moveType = MoveType.Before, bool setTarget = false)
	{
		if (moveType == MoveType.After)
		{
			_next = insn?.Next;
		}
		else
		{
			_next = insn;
		}
		if (setTarget)
		{
			_searchTarget = ((moveType != MoveType.After) ? SearchTarget.Next : SearchTarget.Prev);
		}
		else
		{
			_searchTarget = SearchTarget.None;
		}
		if (moveType == MoveType.AfterLabel)
		{
			MoveAfterLabels();
		}
		else
		{
			MoveBeforeLabels();
		}
		return this;
	}

	public ILCursor MoveAfterLabels()
	{
		_afterLabels = IncomingLabels.ToArray();
		return this;
	}

	public ILCursor MoveBeforeLabels()
	{
		_afterLabels = null;
		return this;
	}

	public ILCursor Goto(int index, MoveType moveType = MoveType.Before, bool setTarget = false)
	{
		if (index < 0)
		{
			index += Instrs.Count;
		}
		return Goto((index == Instrs.Count) ? null : Instrs[index], moveType, setTarget);
	}

	public ILCursor GotoLabel(ILLabel label, MoveType moveType = MoveType.AfterLabel, bool setTarget = false)
	{
		return Goto(label.Target, moveType, setTarget);
	}

	public ILCursor GotoNext(MoveType moveType = MoveType.Before, params Func<Instruction, bool>[] predicates)
	{
		if (!TryGotoNext(moveType, predicates))
		{
			throw new KeyNotFoundException();
		}
		return this;
	}

	public bool TryGotoNext(MoveType moveType = MoveType.Before, params Func<Instruction, bool>[] predicates)
	{
		Collection<Instruction> instrs = Instrs;
		int i = Index;
		if (SearchTarget == SearchTarget.Next)
		{
			i++;
		}
		for (; i + predicates.Length <= instrs.Count; i++)
		{
			int num = 0;
			while (true)
			{
				if (num < predicates.Length)
				{
					Func<Instruction, bool> obj = predicates[num];
					if (obj != null && !obj(instrs[i + num]))
					{
						break;
					}
					num++;
					continue;
				}
				Goto((moveType == MoveType.After) ? (i + predicates.Length - 1) : i, moveType, setTarget: true);
				return true;
			}
		}
		return false;
	}

	public ILCursor GotoPrev(MoveType moveType = MoveType.Before, params Func<Instruction, bool>[] predicates)
	{
		if (!TryGotoPrev(moveType, predicates))
		{
			throw new KeyNotFoundException();
		}
		return this;
	}

	public bool TryGotoPrev(MoveType moveType = MoveType.Before, params Func<Instruction, bool>[] predicates)
	{
		Collection<Instruction> instrs = Instrs;
		int num = Index - 1;
		if (SearchTarget == SearchTarget.Prev)
		{
			num--;
		}
		for (num = Math.Min(num, instrs.Count - predicates.Length); num >= 0; num--)
		{
			int num2 = 0;
			while (true)
			{
				if (num2 < predicates.Length)
				{
					Func<Instruction, bool> obj = predicates[num2];
					if (obj != null && !obj(instrs[num + num2]))
					{
						break;
					}
					num2++;
					continue;
				}
				Goto((moveType == MoveType.After) ? (num + predicates.Length - 1) : num, moveType, setTarget: true);
				return true;
			}
		}
		return false;
	}

	public ILCursor GotoNext(params Func<Instruction, bool>[] predicates)
	{
		return GotoNext(MoveType.Before, predicates);
	}

	public bool TryGotoNext(params Func<Instruction, bool>[] predicates)
	{
		return TryGotoNext(MoveType.Before, predicates);
	}

	public ILCursor GotoPrev(params Func<Instruction, bool>[] predicates)
	{
		return GotoPrev(MoveType.Before, predicates);
	}

	public bool TryGotoPrev(params Func<Instruction, bool>[] predicates)
	{
		return TryGotoPrev(MoveType.Before, predicates);
	}

	public void FindNext(out ILCursor[] cursors, params Func<Instruction, bool>[] predicates)
	{
		if (!TryFindNext(out cursors, predicates))
		{
			throw new KeyNotFoundException();
		}
	}

	public bool TryFindNext(out ILCursor[] cursors, params Func<Instruction, bool>[] predicates)
	{
		cursors = new ILCursor[predicates.Length];
		ILCursor iLCursor = this;
		for (int i = 0; i < predicates.Length; i++)
		{
			iLCursor = iLCursor.Clone();
			if (!iLCursor.TryGotoNext(predicates[i]))
			{
				return false;
			}
			cursors[i] = iLCursor;
		}
		return true;
	}

	public void FindPrev(out ILCursor[] cursors, params Func<Instruction, bool>[] predicates)
	{
		if (!TryFindPrev(out cursors, predicates))
		{
			throw new KeyNotFoundException();
		}
	}

	public bool TryFindPrev(out ILCursor[] cursors, params Func<Instruction, bool>[] predicates)
	{
		cursors = new ILCursor[predicates.Length];
		ILCursor iLCursor = this;
		for (int num = predicates.Length - 1; num >= 0; num--)
		{
			iLCursor = iLCursor.Clone();
			if (!iLCursor.TryGotoPrev(predicates[num]))
			{
				return false;
			}
			cursors[num] = iLCursor;
		}
		return true;
	}

	public void MarkLabel(ILLabel label)
	{
		if (label == null)
		{
			label = new ILLabel(Context);
		}
		label.Target = Next;
		if (_afterLabels != null)
		{
			Array.Resize(ref _afterLabels, _afterLabels.Length + 1);
			_afterLabels[_afterLabels.Length - 1] = label;
		}
		else
		{
			_afterLabels = new ILLabel[1] { label };
		}
	}

	public ILLabel MarkLabel()
	{
		ILLabel iLLabel = DefineLabel();
		MarkLabel(iLLabel);
		return iLLabel;
	}

	public ILLabel DefineLabel()
	{
		return Context.DefineLabel();
	}

	private ILCursor _Insert(Instruction instr)
	{
		Instrs.Insert(Index, instr);
		_Retarget(instr, MoveType.After);
		return this;
	}

	public ILCursor Remove()
	{
		int index = Index;
		_Retarget(Next.Next, MoveType.Before);
		Instrs.RemoveAt(index);
		return this;
	}

	public ILCursor RemoveRange(int num)
	{
		int index = Index;
		_Retarget(Instrs[index + num], MoveType.Before);
		while (num-- > 0)
		{
			Instrs.RemoveAt(index);
		}
		return this;
	}

	private void _Retarget(Instruction next, MoveType moveType)
	{
		if (_afterLabels != null)
		{
			ILLabel[] afterLabels = _afterLabels;
			for (int i = 0; i < afterLabels.Length; i++)
			{
				afterLabels[i].Target = next;
			}
		}
		Goto(next, moveType);
	}

	public ILCursor Emit(OpCode opcode, ParameterDefinition parameter)
	{
		return _Insert(IL.Create(opcode, parameter));
	}

	public ILCursor Emit(OpCode opcode, VariableDefinition variable)
	{
		return _Insert(IL.Create(opcode, variable));
	}

	public ILCursor Emit(OpCode opcode, Instruction[] targets)
	{
		return _Insert(IL.Create(opcode, targets));
	}

	public ILCursor Emit(OpCode opcode, Instruction target)
	{
		return _Insert(IL.Create(opcode, target));
	}

	public ILCursor Emit(OpCode opcode, double value)
	{
		return _Insert(IL.Create(opcode, value));
	}

	public ILCursor Emit(OpCode opcode, float value)
	{
		return _Insert(IL.Create(opcode, value));
	}

	public ILCursor Emit(OpCode opcode, long value)
	{
		return _Insert(IL.Create(opcode, value));
	}

	public ILCursor Emit(OpCode opcode, sbyte value)
	{
		return _Insert(IL.Create(opcode, value));
	}

	public ILCursor Emit(OpCode opcode, byte value)
	{
		return _Insert(IL.Create(opcode, value));
	}

	public ILCursor Emit(OpCode opcode, string value)
	{
		return _Insert(IL.Create(opcode, value));
	}

	public ILCursor Emit(OpCode opcode, FieldReference field)
	{
		return _Insert(IL.Create(opcode, field));
	}

	public ILCursor Emit(OpCode opcode, CallSite site)
	{
		return _Insert(IL.Create(opcode, site));
	}

	public ILCursor Emit(OpCode opcode, TypeReference type)
	{
		return _Insert(IL.Create(opcode, type));
	}

	public ILCursor Emit(OpCode opcode)
	{
		return _Insert(IL.Create(opcode));
	}

	public ILCursor Emit(OpCode opcode, int value)
	{
		return _Insert(IL.Create(opcode, value));
	}

	public ILCursor Emit(OpCode opcode, MethodReference method)
	{
		return _Insert(IL.Create(opcode, method));
	}

	public ILCursor Emit(OpCode opcode, FieldInfo field)
	{
		return _Insert(IL.Create(opcode, field));
	}

	public ILCursor Emit(OpCode opcode, MethodBase method)
	{
		return _Insert(IL.Create(opcode, method));
	}

	public ILCursor Emit(OpCode opcode, Type type)
	{
		return _Insert(IL.Create(opcode, type));
	}

	public ILCursor Emit(OpCode opcode, object operand)
	{
		return _Insert(IL.Create(opcode, operand));
	}

	public ILCursor Emit<T>(OpCode opcode, string memberName)
	{
		return _Insert(IL.Create(opcode, typeof(T).GetMember(memberName, (BindingFlags)(-1)).FirstOrDefault()));
	}

	public int AddReference<T>(T t)
	{
		return Context.AddReference(t);
	}

	public void EmitGetReference<T>(int id)
	{
		Emit(OpCodes.Ldc_I4, id);
		Emit(OpCodes.Call, Context.ReferenceBag.GetGetter<T>());
	}

	public int EmitReference<T>(T t)
	{
		int num = AddReference(t);
		EmitGetReference<T>(num);
		return num;
	}

	public int EmitDelegate<T>(T cb) where T : Delegate
	{
		if (cb.GetInvocationList().Length == 1 && cb.Target == null)
		{
			Emit(OpCodes.Call, cb.Method);
			return -1;
		}
		int result = EmitReference(cb);
		MethodInfo method = typeof(T).GetMethod("Invoke");
		MethodInfo delegateInvoker = Context.ReferenceBag.GetDelegateInvoker<T>();
		if (delegateInvoker != null)
		{
			AddReference(delegateInvoker);
			Emit(OpCodes.Call, delegateInvoker);
			return result;
		}
		Emit(OpCodes.Callvirt, method);
		return result;
	}
}
