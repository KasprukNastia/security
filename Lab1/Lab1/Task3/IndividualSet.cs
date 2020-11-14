using System;
using System.Collections.Generic;

namespace Lab1.Task3
{
    public class IndividualSet : List<Individual>
    {
        public int MembersCount { get; }

        public double Fitness { get; set; }

        public IndividualSet(int membersCount) : base(membersCount)
        {
            MembersCount = membersCount;
        }

        public void Add(Individual item)
        {
            if (Count == MembersCount)
                throw new ArgumentOutOfRangeException("Individual set is full");

            base.Add(item);
        }
    }
}
