using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Kutulu.Tests
{
    public class PlayfieldTests
    {
        [Fact]
        public void PlayfieldFactoryCreate()
        {
            using (var reader = File.OpenText("../../../maze.txt"))
            {
                var maze = PlayfieldFactory.Create(reader);

                for (var y = 0; y < maze.Dimentions.Y; y++)
                {
                    for (var x = 0; x < maze.Dimentions.X; x++)
                    {
                        Debug.Write(GetCellChar(maze[x, y].CellType));
                    }
                    Debug.WriteLine("");
                }
            }
        }

        [Fact]
        public void CopyFromTest()
        {
            using (var reader = File.OpenText("../../../maze.txt"))
            {
                var maze = PlayfieldFactory.Create(reader);
                var maze1 = new Playfield(maze.Dimentions.X, maze.Dimentions.Y);
                maze1.CopyCellTypesFrom(maze);

                Assert.Equal(maze[0, 0].CellType, maze1[0, 0].CellType);
                Assert.Equal(maze[10, 10].CellType, maze1[10, 10].CellType);
            }
        }

        private char GetCellChar(CellType c)
        {
            switch (c)
            {
                case CellType.WALL:
                    return Playfield.WALL;
                case CellType.EMPTY:
                    return Playfield.EMPTY;
                case CellType.SHELTER:
                    return Playfield.EMPTY;
                default:
                    throw new ArgumentException(c.ToString());
            }
        }
    }
}
