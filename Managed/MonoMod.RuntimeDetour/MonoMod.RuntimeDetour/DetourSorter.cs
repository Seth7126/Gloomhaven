using System.Collections.Generic;
using System.Linq;

namespace MonoMod.RuntimeDetour;

internal static class DetourSorter<T> where T : ISortableDetour
{
	private sealed class Group
	{
		public readonly string StepName;

		public List<T> Items = new List<T>();

		public List<Group> Children = new List<Group>();

		public List<Group> NonMatching = new List<Group>();

		public Group(string stepName)
		{
			StepName = stepName;
		}

		public Group(string stepName, List<T> items)
			: this(stepName)
		{
			Items.AddRange(items);
		}

		public void Step(Step step)
		{
			if (Children.Count != 0)
			{
				foreach (Group child in Children)
				{
					child.Step(step);
				}
				return;
			}
			if (Items.Count <= 1)
			{
				return;
			}
			if ((Items.Count == 2 && !((!step.IsFlat) ?? false)) || step.IsFlat == true)
			{
				Items.Sort(step);
				return;
			}
			string name = step.GetType().Name;
			Group obj = new Group(name, new List<T> { Items[0] });
			Children.Add(obj);
			for (int i = 1; i < Items.Count; i++)
			{
				T val = Items[i];
				if (step.Any(obj.Items, val))
				{
					Group obj2 = obj;
					obj = null;
					if (Children.Count > 1)
					{
						foreach (Group child2 in Children)
						{
							if (child2 != obj2 && !step.Any(child2.Items, val) && !step.Any(child2.NonMatching, val))
							{
								obj = child2;
								break;
							}
						}
					}
					if (obj == null)
					{
						obj = new Group(name);
						Children.Add(obj);
						obj.NonMatching.Add(obj2);
						obj2.NonMatching.Add(obj);
					}
				}
				obj.Items.Add(val);
			}
			if (Children.Count == 1)
			{
				Children.Clear();
			}
			else
			{
				Children.Sort(step.ForGroup);
			}
		}

		public void Flatten()
		{
			if (Children.Count != 0)
			{
				Items.Clear();
				Flatten(Items);
			}
		}

		public void Flatten(List<T> total)
		{
			if (Children.Count == 0)
			{
				total.AddRange(Items);
				return;
			}
			foreach (Group child in Children)
			{
				child.Flatten(total);
			}
		}
	}

	private abstract class Step : IComparer<T>
	{
		public abstract GroupComparer ForGroup { get; }

		public virtual bool? IsFlat => null;

		public abstract int Compare(T x, T y);

		public bool Any(List<T> xlist, T y)
		{
			foreach (T item in xlist)
			{
				if (Compare(item, y) != 0)
				{
					return true;
				}
			}
			return false;
		}

		public bool Any(List<Group> groups, T y)
		{
			foreach (Group group in groups)
			{
				if (Any(group.Items, y))
				{
					return true;
				}
			}
			return false;
		}
	}

	private sealed class GroupComparer : IComparer<Group>
	{
		public Step Step;

		public GroupComparer(Step step)
		{
			Step = step;
		}

		public int Compare(Group xg, Group yg)
		{
			foreach (T item in xg.Items)
			{
				foreach (T item2 in yg.Items)
				{
					int result;
					if ((result = Step.Compare(item, item2)) != 0)
					{
						return result;
					}
				}
			}
			return 0;
		}
	}

	private sealed class BeforeAfterAll : Step
	{
		public static readonly BeforeAfterAll _ = new BeforeAfterAll();

		public static readonly GroupComparer Group = new GroupComparer(_);

		public override GroupComparer ForGroup => Group;

		public override bool? IsFlat => false;

		public override int Compare(T a, T b)
		{
			if (a.Before.Contains("*") && !b.Before.Contains("*"))
			{
				return -1;
			}
			if (!a.Before.Contains("*") && b.Before.Contains("*"))
			{
				return 1;
			}
			if (a.After.Contains("*") && !b.After.Contains("*"))
			{
				return 1;
			}
			if (!a.After.Contains("*") && b.After.Contains("*"))
			{
				return -1;
			}
			return 0;
		}
	}

	private sealed class BeforeAfter : Step
	{
		public static readonly BeforeAfter _ = new BeforeAfter();

		public static readonly GroupComparer Group = new GroupComparer(_);

		public override GroupComparer ForGroup => Group;

		public override int Compare(T a, T b)
		{
			if (a.Before.Contains(b.ID))
			{
				return -1;
			}
			if (a.After.Contains(b.ID))
			{
				return 1;
			}
			if (b.Before.Contains(a.ID))
			{
				return 1;
			}
			if (b.After.Contains(a.ID))
			{
				return -1;
			}
			return 0;
		}
	}

	private sealed class Priority : Step
	{
		public static readonly Priority _ = new Priority();

		public static readonly GroupComparer Group = new GroupComparer(_);

		public override GroupComparer ForGroup => Group;

		public override int Compare(T a, T b)
		{
			int num = a.Priority - b.Priority;
			if (num != 0)
			{
				return num;
			}
			return 0;
		}
	}

	private sealed class GlobalIndex : Step
	{
		public static readonly GlobalIndex _ = new GlobalIndex();

		public static readonly GroupComparer Group = new GroupComparer(_);

		public override GroupComparer ForGroup => Group;

		public override int Compare(T a, T b)
		{
			return a.GlobalIndex.CompareTo(b.GlobalIndex);
		}
	}

	public static void Sort(List<T> detours)
	{
		lock (detours)
		{
			if (detours.Count > 1)
			{
				detours.Sort(GlobalIndex._);
				Group obj = new Group("Init", detours);
				obj.Step(BeforeAfterAll._);
				obj.Step(BeforeAfter._);
				obj.Step(Priority._);
				obj.Step(GlobalIndex._);
				detours.Clear();
				obj.Flatten(detours);
			}
		}
	}
}
