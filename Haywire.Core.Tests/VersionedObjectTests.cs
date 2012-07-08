using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Haywire.Core.Versioning;

namespace Haywire.Core.Tests
{
    [TestClass]
    public class VersionedObjectTests
    {
        private const int node1 = 1;
        private const int node2 = 2;
        private VersionedObject<int> version1;
        private VersionedObject<int> version2;

        [TestInitialize]
        public void GivenSameAncestor()
        {
            version1 = new VersionedObject<int>(0);
            version1.Object = 1;
            version1.Version.IncrementVersion(node1, DateTime.Now.Ticks);

            version2 = version1.Clone();
        }

        [TestMethod]
        public void WhenConcurrentChangesOccur_ThenComparerReturnsConcurrently()
        {
            version1.Object = 2;
            version1.Version.IncrementVersion(node1, DateTime.UtcNow.Ticks);

            version2.Object = 2;
            version2.Version.IncrementVersion(node2, DateTime.UtcNow.Ticks);

            var comparer = new HappenedBeforeComparator<int>();
            var result = (Occured)comparer.Compare(version1, version2);

            Assert.AreEqual<Occured>(Occured.Concurrently, result);
        }
    }
}
