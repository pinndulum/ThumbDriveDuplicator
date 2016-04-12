using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThumbDriveDuplicator
{
    public static class SystemExtensions
    {
        public static bool IsNullOrEmpty(this ICollection obj)
        {
            return (obj == null || obj.Count == 0);
        }

        public static IEnumerable<IEnumerable<T>> SplitGroup<T>(this ICollection<T> groupToSplit, int itemsPerGroup)
        {
            var groupcount = (groupToSplit.Count() / itemsPerGroup) + ((groupToSplit.Count() % itemsPerGroup) > 0 ? 1 : 0);
            for (int groupid = 0; groupid < groupcount; groupid++)
            {
                yield return groupToSplit.Skip(groupid * itemsPerGroup).Take(itemsPerGroup);
            }
        }
    }
}
