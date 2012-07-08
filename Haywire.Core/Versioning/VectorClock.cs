using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Haywire.Core.Versioning
{
    public class VectorClock : IVersion
    {
        private const int MAX_NUMBER_OF_VERSIONS = short.MaxValue;

        private List<ClockEntry> versions;
        public long Timestamp { get; private set; }

        /// <summary>
        /// Construct an empty VectorClock.
        /// </summary>
        public VectorClock()
            : this(new List<ClockEntry>(0), System.Environment.TickCount)
        {
        }

        /// <summary>
        /// Create a VectorClock with the given timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp to prepopulate</param>
        public VectorClock(long timestamp)
            : this(new List<ClockEntry>(0), timestamp)
        {
        }

        /// <summary>
        /// Create a VectorClock with the given version and timestamp.
        /// </summary>
        /// <param name="versions">The version to prepopulate</param>
        /// <param name="timestamp">The timestamp to prepopulate</param>
        public VectorClock(List<ClockEntry> versions, long timestamp)
        {
            this.versions = versions;
            this.Timestamp = timestamp;
        }

        public ReadOnlyCollection<ClockEntry> Versions
        {
            get { return versions.AsReadOnly(); }
        }

        /// <summary>
        /// Increment the version info associated with the given node
        /// </summary>
        /// <param name="nodeId">Node to increment</param>
        /// <param name="time">Latest timestamp to use</param>
        public void IncrementVersion(int nodeId, long time)
        {
            if (nodeId < 0 || nodeId > short.MaxValue)
                throw new ArgumentOutOfRangeException("nodeId");

            this.Timestamp = time;

            // stop on the index greater or equal to the node
            bool found = false;
            int index = 0;

            for (; index < this.versions.Count(); index++)
            {
                if (versions[index].NodeId == nodeId)
                {
                    found = true;
                    break;
                }
                else if (versions[index].NodeId > nodeId)
                {
                    found = false;
                    break;
                }
            }

            if (found)
            {
                versions[index] = versions[index].Increment();
            }
            else if (index < versions.Count - 1)
            {
                versions.Insert(index, new ClockEntry((short)nodeId, (short)1));
            }
            else
            {
                // we don't already have a version for this, so add it
                if (versions.Count > MAX_NUMBER_OF_VERSIONS)
                    throw new ArgumentOutOfRangeException("Vector clock is full");

                versions.Add(new ClockEntry((short)nodeId, (short)1));
            }
        }

        /// <summary>
        /// Get new vector clock based on this clock but incremented on index nodeId
        /// </summary>
        /// <param name="nodeId">The id of the node to increment</param>
        /// <param name="time">A vector clock equal on each element execept that indexed by nodeId</param>
        /// <returns></returns>
        public VectorClock Increment(int nodeId, long time)
        {
            VectorClock copyClock = this.Clone();
            copyClock.IncrementVersion(nodeId, time);
            return copyClock;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public VectorClock Clone()
        {
            return new VectorClock(new List<ClockEntry>(versions), this.Timestamp);
        }

        public override bool Equals(object obj)
        {
            if(this == obj)
                return true;
            if(obj == null)
                return false;

            if (obj is VectorClock)
            {
                var clock = obj as VectorClock;
                
                if (clock.versions.Count != versions.Count)
                    return false;

                for (int i = 0; i < versions.Count; i++)
                {
                    if (!versions[i].Equals(clock.versions[i]))
                        return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return versions.GetHashCode();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("version(");
            if (this.versions.Count > 0)
            {
                for (int i = 0; i < this.versions.Count - 1; i++)
                {
                    builder.Append(this.versions[i]);
                    builder.Append(", ");
                }
                builder.Append(this.versions[this.versions.Count - 1]);
            }
            builder.Append(")");
            return builder.ToString();
        }

        public VectorClock Merge(VectorClock clock)
        {
            VectorClock newClock = new VectorClock();
            int i = 0;
            int j = 0;
            while (i < this.versions.Count && j < clock.versions.Count)
            {
                ClockEntry v1 = this.versions[i];
                ClockEntry v2 = clock.versions[j];
                if (v1.NodeId == v2.NodeId)
                {
                    newClock.versions.Add(new ClockEntry(v1.NodeId,
                                                         (short)Math.Max(v1.Version,
                                                                          v2.Version)));
                    i++;
                    j++;
                }
                else if (v1.NodeId < v2.NodeId)
                {
                    newClock.versions.Add(v1.Clone());
                    i++;
                }
                else
                {
                    newClock.versions.Add(v2.Clone());
                    j++;
                }
            }

            // Okay now there may be leftovers on one or the other list remaining
            for (int k = i; k < this.versions.Count; k++)
                newClock.versions.Add(this.versions[k].Clone());
            for (int k = j; k < clock.versions.Count; k++)
                newClock.versions.Add(clock.versions[k].Clone());

            return newClock;
        }

        public Occured Compare(IVersion v) 
        {
            if (v is VectorClock)
            {
                return Compare(this, (VectorClock)v);
            }
            else
            {
                throw new ArgumentException("Cannot compare Versions of different types.");
            }
        }

        /// <summary>
        /// Is this Reflexive, AntiSymetic, and Transitive? Compare two VectorClocks,
        /// the outcomes will be one of the following: -- Clock 1 is BEFORE clock 2
        /// if there exists an i such that c1(i) <= c(2) and there does not exist a j
        /// such that c1(j) > c2(j). -- Clock 1 is CONCURRANT to clock 2 if there
        /// exists an i, j such that c1(i) < c2(i) and c1(j) > c2(j) -- Clock 1 is
        /// AFTER clock 2 otherwise
        /// </summary>
        /// <param name="v1">The first VectorClock</param>
        /// <param name="v2">The second VectorClock</param>
        /// <returns>Whether the change occured before, after or concurrently</returns>
        public static Occured Compare(VectorClock v1, VectorClock v2)
        {
            if (v1 == null || v2 == null)
                throw new ArgumentException("Can't compare null vector clocks!");

            // We do two checks: v1 <= v2 and v2 <= v1 if both are true then
            bool v1Bigger = false;
            bool v2Bigger = false;
            int p1 = 0;
            int p2 = 0;

            while (p1 < v1.versions.Count && p2 < v2.versions.Count)
            {
                ClockEntry ver1 = v1.versions[p1];
                ClockEntry ver2 = v2.versions[p2];

                if (ver1.NodeId == ver2.NodeId)
                {
                    if (ver1.Version > ver2.Version)
                        v1Bigger = true;
                    else if (ver2.Version > ver1.Version)
                        v2Bigger = true;
                    p1++;
                    p2++;
                }
                else if (ver1.NodeId > ver2.NodeId)
                {
                    // since ver1 is bigger that means it is missing a version that ver2 has
                    v2Bigger = true;
                    p2++;
                }
                else
                {
                    // this means ver2 is bigger which means it is missing a version ver1 has
                    v1Bigger = true;
                    p1++;
                }
            }

            // Check for left overs
            if (p1 < v1.versions.Count)
                v1Bigger = true;
            else if (p2 < v2.versions.Count)
                v2Bigger = true;

            // This is the case where they are equal, return BEFORE arbitrarily
            if (!v1Bigger && !v2Bigger)
                return Occured.Before;
            // This is the case where v1 is a successor clock to v2
            else if (v1Bigger && !v2Bigger)
                return Occured.After;
            // This is the case where v2 is a successor clock to v1
            else if (!v1Bigger && v2Bigger)
                return Occured.Before;
            // This is the case where both clocks are parallel to one another
            else
                return Occured.Concurrently;
        }
    }
}
