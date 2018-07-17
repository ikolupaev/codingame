using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Kutulu.Tests
{
    public class PathFinderTests
    {
        [Fact]
        public void FindPath()
        {
            using (var reader = File.OpenText("../../../maze1.txt"))
            {
                var maze = PlayfieldFactory.Create(reader);
                var result = new PathFinder(maze)
                    .From(new Vector2D(1, 1))
                    .To(new Vector2D(3, 1))
                    .FindPath();

                Assert.Equal(5, result.Path.Length);
            }
        }
    }
}
