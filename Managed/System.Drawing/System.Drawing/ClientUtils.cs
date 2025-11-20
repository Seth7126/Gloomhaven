using System.Collections;
using System.Security;
using System.Threading;

namespace System.Drawing;

internal static class ClientUtils
{
	internal class WeakRefCollection : IList, ICollection, IEnumerable
	{
		internal class WeakRefObject
		{
			private int _hash;

			private WeakReference _weakHolder;

			internal bool IsAlive => _weakHolder.IsAlive;

			internal object Target => _weakHolder.Target;

			internal WeakRefObject(object obj)
			{
				_weakHolder = new WeakReference(obj);
				_hash = obj.GetHashCode();
			}

			public override int GetHashCode()
			{
				return _hash;
			}

			public override bool Equals(object obj)
			{
				WeakRefObject weakRefObject = obj as WeakRefObject;
				if (weakRefObject == this)
				{
					return true;
				}
				if (weakRefObject == null)
				{
					return false;
				}
				if (weakRefObject.Target != Target && (Target == null || !Target.Equals(weakRefObject.Target)))
				{
					return false;
				}
				return true;
			}
		}

		internal ArrayList InnerList { get; }

		public int RefCheckThreshold { get; set; } = int.MaxValue;

		public object this[int index]
		{
			get
			{
				if (InnerList[index] is WeakRefObject { IsAlive: not false } weakRefObject)
				{
					return weakRefObject.Target;
				}
				return null;
			}
			set
			{
				InnerList[index] = CreateWeakRefObject(value);
			}
		}

		public bool IsFixedSize => InnerList.IsFixedSize;

		public int Count => InnerList.Count;

		object ICollection.SyncRoot => InnerList.SyncRoot;

		public bool IsReadOnly => InnerList.IsReadOnly;

		bool ICollection.IsSynchronized => InnerList.IsSynchronized;

		internal WeakRefCollection()
			: this(4)
		{
		}

		internal WeakRefCollection(int size)
		{
			InnerList = new ArrayList(size);
		}

		public void ScavengeReferences()
		{
			int num = 0;
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				if (this[num] == null)
				{
					InnerList.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}

		public override bool Equals(object obj)
		{
			if (!(obj is WeakRefCollection weakRefCollection))
			{
				return true;
			}
			if (weakRefCollection == null || Count != weakRefCollection.Count)
			{
				return false;
			}
			for (int i = 0; i < Count; i++)
			{
				if (InnerList[i] != weakRefCollection.InnerList[i] && (InnerList[i] == null || !InnerList[i].Equals(weakRefCollection.InnerList[i])))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		private WeakRefObject CreateWeakRefObject(object value)
		{
			if (value == null)
			{
				return null;
			}
			return new WeakRefObject(value);
		}

		private static void Copy(WeakRefCollection sourceList, int sourceIndex, WeakRefCollection destinationList, int destinationIndex, int length)
		{
			if (sourceIndex < destinationIndex)
			{
				sourceIndex += length;
				destinationIndex += length;
				while (length > 0)
				{
					destinationList.InnerList[--destinationIndex] = sourceList.InnerList[--sourceIndex];
					length--;
				}
			}
			else
			{
				while (length > 0)
				{
					destinationList.InnerList[destinationIndex++] = sourceList.InnerList[sourceIndex++];
					length--;
				}
			}
		}

		public void RemoveByHashCode(object value)
		{
			if (value == null)
			{
				return;
			}
			int hashCode = value.GetHashCode();
			for (int i = 0; i < InnerList.Count; i++)
			{
				if (InnerList[i] != null && InnerList[i].GetHashCode() == hashCode)
				{
					RemoveAt(i);
					break;
				}
			}
		}

		public void Clear()
		{
			InnerList.Clear();
		}

		public bool Contains(object value)
		{
			return InnerList.Contains(CreateWeakRefObject(value));
		}

		public void RemoveAt(int index)
		{
			InnerList.RemoveAt(index);
		}

		public void Remove(object value)
		{
			InnerList.Remove(CreateWeakRefObject(value));
		}

		public int IndexOf(object value)
		{
			return InnerList.IndexOf(CreateWeakRefObject(value));
		}

		public void Insert(int index, object value)
		{
			InnerList.Insert(index, CreateWeakRefObject(value));
		}

		public int Add(object value)
		{
			if (Count > RefCheckThreshold)
			{
				ScavengeReferences();
			}
			return InnerList.Add(CreateWeakRefObject(value));
		}

		public void CopyTo(Array array, int index)
		{
			InnerList.CopyTo(array, index);
		}

		public IEnumerator GetEnumerator()
		{
			return InnerList.GetEnumerator();
		}
	}

	public static bool IsCriticalException(Exception ex)
	{
		if (!(ex is NullReferenceException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException) && !(ex is ExecutionEngineException) && !(ex is IndexOutOfRangeException))
		{
			return ex is AccessViolationException;
		}
		return true;
	}

	public static bool IsSecurityOrCriticalException(Exception ex)
	{
		if (!(ex is SecurityException))
		{
			return IsCriticalException(ex);
		}
		return true;
	}
}
