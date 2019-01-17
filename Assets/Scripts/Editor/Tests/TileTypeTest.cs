using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Labrys;

namespace Tests
{
    public class TileTypeTest
    {
        [Test]
        public void AllBytesReturn()
        {
            for (int b = byte.MinValue; b <= byte.MaxValue; b++) 
            {
                TileType t = TileType.GetTileType((Connection)b).Item1;
            }
        }

        [Test]
        public void DoesEqualsWork()
        {
            TileType empty = TileType.EMPTY;
            (TileType empty2, _) = TileType.GetTileType(0);

            Assert.AreEqual(empty, empty2);
            Assert.True(empty.Equals(empty2)); // Explicit test
        }
    }
}
