using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Labrys;

namespace Tests
{
    public class FeatureTest
    {
        [Test]
        public void CanPlaceBasic() 
        {
            Labrys.Grid grid = new Labrys.Grid();

            grid[0, 0] = new Section();

            // Basic 2x2 feature
            Feature feature = new Feature();
            feature.Add(0, 0);
            feature.Add(1, 0);
            feature.Add(0, 1);
            feature.Add(1, 1);

            // Placing it in any configuration over the blocking tile should fail

            // This tests for placing the Feature over different grid positions
            Assert.False(feature.CanPlace(grid, new Vector2Int( 0,  0), new Vector2Int(0, 0), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(-1,  0), new Vector2Int(0, 0), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int( 0, -1), new Vector2Int(0, 0), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(-1, -1), new Vector2Int(0, 0), 0));

            // This tests placing the Feature with a different local position.
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(0, 0), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(1, 0), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(0, 1), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(1, 1), 0));

            // This tests both at once.
            // This can be read as "I want to place this Feature so that its upper-right Section is
            // placed at the position (1, 1) in the grid". This should fail, because the bottom-left
            // corner of the Feature is over (0, 0), which already exists.
            Assert.False(feature.CanPlace(grid, new Vector2Int(1, 1), new Vector2Int(1, 1), 0));

            // Weird case, you can use offsets that don't exist but they can still work.
            Assert.False(feature.CanPlace(grid, new Vector2Int(100, 100), new Vector2Int(100, 100), 0));

            // The above types of cases are the only ones that should fail- most other placements 
            // that don't follow those patterns should succeed. Here's a few that should work.

            // No local offset
            Assert.True(feature.CanPlace(grid, new Vector2Int(1, 0), new Vector2Int(0, 0), 0));
            Assert.True(feature.CanPlace(grid, new Vector2Int(-2, 0), new Vector2Int(0, 0), 0));
            Assert.True(feature.CanPlace(grid, new Vector2Int(-2, -2), new Vector2Int(0, 0), 0));
            Assert.True(feature.CanPlace(grid, new Vector2Int(10, 0), new Vector2Int(0, 0), 0));

            // Top-right local offset
            Assert.True(feature.CanPlace(grid, new Vector2Int(-1, 0), new Vector2Int(1, 1), 0));
            Assert.True(feature.CanPlace(grid, new Vector2Int(-1, -1), new Vector2Int(1, 1), 0));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(1, 1), 0));

            // Just y local offset
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(0, 1), 0));
        }

        [Test]
        public void CanPlaceWithRotation()
        {
            Labrys.Grid grid = new Labrys.Grid();
            grid[0, 0] = new Section();

            // 3x1 feature
            Feature feature = new Feature();
            feature.Add(0, 0);
            feature.Add(1, 0);
            feature.Add(2, 0);

            // Basic tests for placement
            Assert.False(feature.CanPlace(grid, new Vector2Int(-2, 0), new Vector2Int(0, 0), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(-1, 0), new Vector2Int(0, 0), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int( 0, 0), new Vector2Int(0, 0), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(1, 0), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(2, 0), 0));

            // Rotating 90 degrees clockwise should keep everything centered. 
            // Because of this centering property, rotating 270 degrees should be the same.
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(0, 0), 1));

            // Assuming we're not normalized..
            //Assert.False(feature.CanPlace(grid, new Vector2Int(0, 1), new Vector2Int(0, 0), 1));
            //Assert.False(feature.CanPlace(grid, new Vector2Int(0, 2), new Vector2Int(0, 0), 1));

            // Assuming we are..
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(0, 0), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(0, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 1), new Vector2Int(0, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 2), new Vector2Int(0, 0), 1));

            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(1, 0), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(1, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(1, 0), 1));

            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(2, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(2, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(2, 0), 1));


            //Assert.False(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(0, 0), 1));
            //Assert.False(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(0, 0), 1));
            //Assert.False(feature.CanPlace(grid, new Vector2Int(0,  0), new Vector2Int(0, 0), 1));
            //Assert.False(feature.CanPlace(grid, new Vector2Int(0,  0), new Vector2Int(0, 1), 1));
            //Assert.False(feature.CanPlace(grid, new Vector2Int(0,  0), new Vector2Int(0, 2), 1));

            //Assert.False(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(0, 0), 3));
            //Assert.False(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(0, 0), 3));
            //Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(0, 0), 3));
            //Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(0, 1), 3));
            //Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(0, 2), 3));
        }
    }
}
