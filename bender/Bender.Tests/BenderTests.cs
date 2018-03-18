using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bender.Tests
{
    [TestFixture]
    public class BenderTests
    {
        [Test]
        public void TestMethod()
        {
            var reader = new StringReader(MultipleLoops);
            Solution.Run(reader);
        }

        string MultipleLoops =
        @"30 15
###############
#  #@#I  T$#  #
#  #    IB #  #
#  #     W #  #
#  #      ##  #
#  #B XBN# #  #
#  ##      #  #
#  #       #  #
#  #     W #  #
#  #      ##  #
#  #B XBN# #  #
#  ##      #  #
#  #       #  #
#  #     W #  #
#  #      ##  #
#  #B XBN# #  #
#  ##      #  #
#  #       #  #
#  #       #  #
#  #      ##  #
#  #  XBIT #  #
#  #########  #
#             #
# ##### ##### #
# #     #     #
# #     #  ## #
# #     #   # #
# ##### ##### #
#             #
###############
";
    }
}
