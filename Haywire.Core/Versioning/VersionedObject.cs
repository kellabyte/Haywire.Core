using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Haywire.Core.Versioning
{
    public class VersionedObject<TObject>
    {
        public VersionedObject(TObject obj)
            : this(obj, new VectorClock())
        {
        }

        public VersionedObject(TObject obj, IVersion version)
        {
            this.Version = version == null ? new VectorClock() : (VectorClock)version;
            this.Object = obj;
        }

        public VectorClock Version { get; private set; }
        public TObject Object { get; set; }

        public override bool Equals(Object obj)
        {
            if(obj == this)
                return true;
            
            var version = obj as VersionedObject<TObject>;
            if (version != null)
            {
                return object.Equals(this.Version, version.Version)
                    //&& Utils.deepEquals(getValue(), versioned.getValue());
                       && object.Equals(this.Object, version.Object);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return 31 + this.Version.GetHashCode() + 31 * this.Object.GetHashCode();
        }

        public override string ToString()
        {
            return "[" + this.Object + ", " + this.Version + "]";
        }

        /// <summary>
        /// Create a clone of this VersionedObject such that the object pointed to
        /// is the same, but the VectorClock and Versioned wrapper is a shallow copy.
        /// </summary>
        /// <returns>Returns clone of this VersionedObject</returns>
        public VersionedObject<TObject> Clone()
        {
            return new VersionedObject<TObject>(this.Object, this.Version.Clone());
        }
    }
}
