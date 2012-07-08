using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Haywire.Core.Versioning
{
    public class HappenedBeforeComparator<TObject> : IComparer<VersionedObject<TObject>>
    {
        public int Compare(VersionedObject<TObject> x, VersionedObject<TObject> y)
        {
            Occured occured = x.Version.Compare(y.Version);
            if (occured == Occured.Before)
                return -1;
            else if (occured == Occured.After)
                return 1;
            else
                return 0;
        }
    }
}
