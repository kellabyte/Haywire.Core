using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Haywire.Core.Versioning
{
    /// <summary>
    /// The result of comparing two times--either t1 is BEFORE t2, t1 is AFTER t2, or t1 happens CONCURRENTLY to t2.
    /// </summary>
    public enum Occured
    {
        Before = -1,
        After = 1,
        Concurrently = 0,
    }
}
