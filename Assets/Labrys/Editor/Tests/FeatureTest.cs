using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Labrys.Generation;

namespace Tests
{
    public class FeatureTest
    {
        [Test]
        public void IsMinimumCorrect()
        {
            Feature feature = new Feature();
            feature.Add(1, 2);
            feature.Add(2, 2);
            feature.Add(3, 2);

            feature.Add(1, 3);
            feature.Add(2, 3);
            feature.Add(3, 3);

            feature.Add(1, 4);
            feature.Add(2, 4);
            feature.Add(3, 4);

            // Basic true statements
            Assert.AreEqual(1, feature.MinX);
            Assert.AreEqual(2, feature.MinY);
            Assert.AreEqual(3, feature.MaxX);
            Assert.AreEqual(4, feature.MaxY);

            /* 
             * Note the four bounding corners are as follows:
             * 
             * (1,4) - (3,4)
             *   |       |
             * (1,2) - (3,2)
             * 
             * When we get the minimum under different rotations, we want the
             * new bottom-left corner. This maps to the Nth corner starting from 
             * the bottom-left corner, going counterclockwise- i.e., bottom-left,
             * then bottom-right, etc. N is the same as the rotation amount.
             */

            Vector2Int bottomLeft   = new Vector2Int(1, 2);
            Vector2Int bottomRight  = new Vector2Int(3, 2);
            Vector2Int topLeft      = new Vector2Int(1, 4);
            Vector2Int topRight     = new Vector2Int(3, 4);

            // Check the 0 rotation case
            Assert.AreEqual(bottomLeft, Feature.Rotate(new Vector2Int(feature.MinX, feature.MinY), 0));
            Assert.AreEqual(bottomLeft, feature.RotatedMin(0));
            Assert.AreEqual(Feature.Rotate(bottomLeft, 0), feature.RotatedMin(0));

            // Check the other rotations
            Assert.AreEqual(Feature.Rotate(bottomRight, 1), feature.RotatedMin(1));
            Assert.AreEqual(Feature.Rotate(topRight, 2), feature.RotatedMin(2));
            Assert.AreEqual(Feature.Rotate(topLeft, 3), feature.RotatedMin(3));

        }


        [Test]
        public void CanPlaceBasic() 
        {
            Labrys.Generation.Grid grid = new Labrys.Generation.Grid();

            grid[0, 0] = Section.Default();

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
            Labrys.Generation.Grid grid = new Labrys.Generation.Grid();
            grid[0, 0] = Section.Default();

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


            /*
             * Every Feature can be initially thought of as normalized. (This might
             * need to be enforced at a later point- TODO test this)
             * 
             * Rotating is followed by a normalization step, which places the 
             * new bottom-left-most Section at the coordinate (0,0). You can 
             * specify which position in the Feature you want to connect using,
             * which essentially shifts the normalized Feature so that the desired
             * coordinate is now at (0,0). These are referred to using the original
             * coordinate (i.e., (1,0) for the middle element to be placed at (0,0)).
             * 
             * A diagram is provided below for the non-shifted case:
             * 
             *  |               |               |X
             *  |               |               |X
             *  |XXX            |X              |X
             * -+------   ->   -+------   ->   -+------
             *  |       rot 90  |X       norm   |
             *                  |X
             */

            // Rotating 90 degrees clockwise should keep everything centered. 
            // Because of this centering property, rotating 270 degrees should be the same.
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(0, 0), 1));

            // Assuming the rotated Feature is normalized (it should be!)-
            // We should be able to place the (0, 0) point anywhere below 0. At y=1 or 2, we overlap.
            // At 3+, there is no overlap anymore.
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(0, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(0, 0), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 1), new Vector2Int(0, 0), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 2), new Vector2Int(0, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 3), new Vector2Int(0, 0), 1));

            // We can place the (1, 0) point at the exact same positions, but offset 1 unit up.
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(1, 0), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(1, 0), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0,  0), new Vector2Int(1, 0), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0,  1), new Vector2Int(1, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0,  2), new Vector2Int(1, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0,  3), new Vector2Int(1, 0), 1));

            // We can place the (2, 0) point at the same positions, but 2 units up.
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(2, 0), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(2, 0), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(2, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 1), new Vector2Int(2, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 2), new Vector2Int(2, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 3), new Vector2Int(2, 0), 1));

            // The pattern should hold for (3, 0), even though that's not a Section in the Feature.
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -3), new Vector2Int(3, 0), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(3, 0), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(3, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(3, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 1), new Vector2Int(3, 0), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 2), new Vector2Int(3, 0), 1));

            // Finally, check for the other rotations. Rotation 2 should == 0 (minus the local offsets),
            // and rotation 3 should == 1 (minus the same).
            Assert.False(feature.CanPlace(grid, new Vector2Int(2, 0), new Vector2Int(0, 0), 2)); 
            Assert.False(feature.CanPlace(grid, new Vector2Int(1, 0), new Vector2Int(0, 0), 2));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(0, 0), 2));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(1, 0), 2));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(2, 0), 2));

            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 2), new Vector2Int(0, 0), 3));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 1), new Vector2Int(0, 0), 3));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(0, 0), 3));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(0, 0), 3));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -3), new Vector2Int(0, 0), 3));
        }

        [Test]
        public void CanPlaceWithRotationAndOffset()
        {
            Labrys.Generation.Grid grid = new Labrys.Generation.Grid();
            grid[0, 0] = Section.Default();

            // 3x1 feature, but offset.
            // With normalization, should be same as CanPlaceWithRotation; the only difference is  
            // that the local offset needs to be different to align with the input coordinates.
            Feature feature = new Feature();
            feature.Add(5, 3);
            feature.Add(6, 3);
            feature.Add(7, 3);

            // Basic tests for placement
            Assert.False(feature.CanPlace(grid, new Vector2Int(-2, 0), new Vector2Int(5, 3), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(-1, 0), new Vector2Int(5, 3), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(5, 3), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(6, 3), 0));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(6, 3), 0));

            // Rotated cases
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(5, 3), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(5, 3), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 1), new Vector2Int(5, 3), 1));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 2), new Vector2Int(5, 3), 1));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 3), new Vector2Int(5, 3), 1));

            Assert.False(feature.CanPlace(grid, new Vector2Int(2, 0), new Vector2Int(5, 3), 2));
            Assert.False(feature.CanPlace(grid, new Vector2Int(1, 0), new Vector2Int(5, 3), 2));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(5, 3), 2));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(6, 3), 2));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(7, 3), 2));

            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 2), new Vector2Int(5, 3), 3));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, 1), new Vector2Int(5, 3), 3));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -1), new Vector2Int(5, 3), 3));
            Assert.False(feature.CanPlace(grid, new Vector2Int(0, -2), new Vector2Int(5, 3), 3));
            Assert.True(feature.CanPlace(grid, new Vector2Int(0, -3), new Vector2Int(5, 3), 3));
        }

        [Test]
        public void CanConnectBasic()
        {
            Labrys.Generation.Grid grid = new Labrys.Generation.Grid();
            grid[0, 0] = Section.Default();

            // 3x1 feature
            Feature feature = new Feature();
            feature.Add(0, 0);
            feature.Add(1, 0);
            feature.Add(2, 0);

            //Assert.True(feature.CanConnect(grid, Vector2Int.zero));
            List<Feature.Configuration> configurations = feature.CanConnect(grid, Vector2Int.zero);
        }
    }
}
