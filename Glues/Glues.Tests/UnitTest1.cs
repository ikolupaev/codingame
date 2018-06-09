using System;
using Xunit;

namespace Glues.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void MathAngle()
        {
            var p = new Point(-515, 8208);
            var p1 = new Point(3667,5984);
            var p2 = new Point(-9657,1905);

            var a1 = p1.AngleTo(p);
            var a2 = p2.AngleTo(p);
            Assert.True(a1 > a2);
        }
    }
}
