using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Haywire.Core.Versioning
{
    public class ClockEntry
    {
        public ClockEntry(short nodeId, long version)
        {
            if (nodeId < 0)
                throw new ArgumentOutOfRangeException("nodeId");
            if (version < 0)
                throw new ArgumentOutOfRangeException("version");

            this.NodeId = nodeId;
            this.Version = version;
        }

        public short NodeId { get; private set; }
        public long Version { get; private set; }

        public ClockEntry Clone()
        {
            return new ClockEntry(this.NodeId, this.Version);
        }

        public ClockEntry Increment()
        {
            return new ClockEntry(this.NodeId, this.Version + 1);
        }

        public override int GetHashCode()
        {
            return this.NodeId + (((int)this.Version) << 16);
        }

        public override bool Equals(object obj)
        {
            if(this == obj)
                return true;

            if(obj == null)
                return false;

            var clockEntry = obj as ClockEntry;
            if (clockEntry != null)
            {
                return clockEntry.NodeId == this.NodeId && clockEntry.Version == this.Version;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return this.NodeId + ":" + this.Version;
        }
    }
}
