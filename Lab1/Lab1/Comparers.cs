using Lab1.Task3;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Lab1
{
    internal class RepeatingKeyXorAttackerComparer : IEqualityComparer<byte>
    {
        public bool Equals([AllowNull] byte x, [AllowNull] byte y)
        {
            return ((char)x).ToString().Equals(((char)y).ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] byte obj)
        {
            return ((char)obj).ToString().ToLower().GetHashCode();
        }
    }

    internal class IndividualSetsComparer : IEqualityComparer<IndividualSet>
    {
        public bool Equals([AllowNull] IndividualSet x, [AllowNull] IndividualSet y)
        {
            if (x.Count != y.Count)
                return false;

            for (int index = 0; index < x.Count; index++)
            {
                if (x.ElementAt(index).Key.Equals(y.ElementAt(index).Key, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public int GetHashCode([DisallowNull] IndividualSet obj)
        {
            return obj.Sum(e => e.GetHashCode());
        }
    }

    internal class IndividualSetTasksComparer : IndividualSetsComparer, IEqualityComparer<Task<IndividualSet>>
    {
        public bool Equals([AllowNull] Task<IndividualSet> x, [AllowNull] Task<IndividualSet> y) =>
            Equals(x.Result, y.Result);

        public int GetHashCode([DisallowNull] Task<IndividualSet> obj) =>
            GetHashCode(obj.Result);
    }
}
