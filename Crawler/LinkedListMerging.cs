using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    public static class LinkedListMerging
    {
        public static void MergeInto<T>(this LinkedList<T> list, LinkedList<T> newlist, Func<T, bool> skipped) where T : IComparable<T>
        {
            if (newlist.First == null)
                return;

            if (list.First == null)
                foreach (var n in newlist)
                    if (!skipped(n))
                        list.AddLast(n);

            MergeInto(list.First, newlist.First, skipped);
        }
        private static void MergeInto<T>(LinkedListNode<T> list, LinkedListNode<T> node, Func<T, bool> skipped) where T : IComparable<T>
        {
            if (node == null)
                return;

            if (skipped(node.Value))
                MergeInto(list, node.Next, skipped);
            else if (list.Value.CompareTo(node.Value) > 0)
            {
                list.List.AddBefore(list, node.Value);
                MergeInto(list, node.Next, skipped);
            }
            else
            {
                if (list.Next != null)
                    MergeInto(list.Next, node, skipped);
                else
                {
                    list.List.AddLast(node.Value);
                    MergeInto(list, node.Next, skipped);
                }
            }
        }

        public static void MergeInto<T>(this LinkedList<T> list, LinkedList<T> newlist, Func<T, T, int> comparer, Func<T, bool> skipped)
        {
            if (newlist.First == null)
                return;

            if (list.First == null)
                foreach (var n in newlist)
                    if (!skipped(n))
                        list.AddLast(n);

            MergeInto(list.First, newlist.First, comparer, skipped);
        }
        private static void MergeInto<T>(LinkedListNode<T> list, LinkedListNode<T> node, Func<T, T, int> comparer, Func<T, bool> skipped)
        {
            if (node == null)
                return;

            if (skipped(node.Value))
                MergeInto(list, node.Next, comparer, skipped);
            else if (comparer(list.Value, node.Value) > 0)
            {
                list.List.AddBefore(list, node.Value);
                MergeInto(list, node.Next, comparer, skipped);
            }
            else
            {
                if (list.Next != null)
                    MergeInto(list.Next, node, comparer, skipped);
                else
                {
                    list.List.AddLast(node.Value);
                    MergeInto(list, node.Next, comparer, skipped);
                }
            }
        }
    }
}
