using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Haywire.Core.Versioning
{
    /// <summary>
    /// An interface that allows us to determine if a given version happened before or after another version.
    /// </summary>
    public interface IVersion
    {
        /// <summary>
        /// Compares whether the given version proceeded or succeeded this version.
        /// </summary>
        /// <param name="version">The version to compare with</param>
        /// <returns>Return whether or not the given version preceeded this one, succeeded it, or is concurrant with it</returns>
        Occured Compare(IVersion version);
    }
}