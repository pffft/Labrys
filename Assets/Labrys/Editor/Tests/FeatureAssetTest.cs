using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Labrys.Generation;
using Labrys.FeatureEditor;

namespace Tests
{
    public class FeatureAssetTest
    {
        [Test]
        public void CanConvertToFromFeature()
        {
            Feature feature1 = new Feature();
            feature1.Add(0, 0);
            feature1.Add(1, 0);
            feature1.Add(2, 0);

            FeatureAsset featureAsset1 = FeatureAsset.FromFeature(feature1);
            Feature testFeature1 = featureAsset1.ToFeature();

            // Basic test. All input positions should be present in output
            Assert.Contains(new Vector2Int(0, 0), testFeature1.Elements.Keys);
            Assert.Contains(new Vector2Int(1, 0), testFeature1.Elements.Keys);
            Assert.Contains(new Vector2Int(2, 0), testFeature1.Elements.Keys);

            // Basic test. Ensure bounds are unchanged
            Assert.AreEqual(feature1.MaxX, testFeature1.MaxX);
            Assert.AreEqual(feature1.MaxY, testFeature1.MaxY);
            Assert.AreEqual(feature1.MinX, testFeature1.MinX);
            Assert.AreEqual(feature1.MinY, testFeature1.MinY);


            // Harder test. Conversion of Feature -> FeatureAsset -> Feature does not obey identity property.
            // All connections at the end will be present in the original, but additionally,
            // they will be restricted based on FeatureAsset retention rules. 
            // 
            // Thus, we test FeatureAsset retention rules separately, and ensure that
            // FeatureAfter.connections & FeatureBefore.connections == FeatureAfter.connections
            // to be sure that no connections are ever dropped.
            foreach (KeyValuePair<Vector2Int, Labrys.Generation.Section> kvp in testFeature1.Elements)
            {
				Labrys.Generation.Section orig = feature1.Elements[kvp.Key];
				Labrys.Generation.Section conv = kvp.Value;

                // Some connections may be lost. Ensure no new ones are added.
                Assert.AreEqual(orig.externalConnections & conv.externalConnections, conv.externalConnections);
                Assert.AreEqual(orig.internalConnections & conv.internalConnections, conv.internalConnections);

                // Make sure variants are unchanged.
                Assert.AreEqual(orig.GetVariant(), conv.GetVariant());
            }
        }

        // Confirms a given section exists at a position.
        private void ValidateSection(FeatureAsset feature, Vector2Int featurePos)
        {
            feature.TryGetSection(featurePos * 2, out Labrys.FeatureEditor.Section section);
            Assert.IsNotNull(section, "Section expected to exist at position: " + (featurePos * 2));
        }

        // Check for valid external links
        void ValidateExternalLink(FeatureAsset feature, Vector2Int featureAssetPos)
        {
            feature.TryGetLink(featureAssetPos, out Link link);

            Assert.IsNotNull(link, "External link expected to exist at position: " + featureAssetPos);
            Assert.True(link.open, "Link was expected to be open (found closed) at position: " + featureAssetPos);
            Assert.True(link.external, "Link was expected to be external (found internal) at position: " + featureAssetPos);
        }

        // Check for valid internal links
        void ValidateInternalLink(FeatureAsset feature, Vector2Int featureAssetPos)
        {
            feature.TryGetLink(featureAssetPos, out Link link);

            Assert.IsNotNull(link, "Internal link expected to exist at position: " + featureAssetPos);
            Assert.True(link.open, "Link was expected to be open (found closed) at position: " + featureAssetPos);
            Assert.False(link.external, "Link was expected to be internal (found external) at position: " + featureAssetPos);
        }

        // Check for invalid links
        void ValidateMissingLink(FeatureAsset feature, Vector2Int featureAssetPos)
        {
            feature.TryGetLink(featureAssetPos, out Link link);

            Assert.IsNull(link, "Link expected to be missing at position: " + featureAssetPos);
        }


        [Test]
        public void CanConvertFromFeature_Line()
        {
            // Feature 1 is a horizontal line.
            Feature feature1 = new Feature();
            feature1.Add(0, 0);
            feature1.Add(1, 0);
            feature1.Add(2, 0);

            FeatureAsset featureAsset1 = FeatureAsset.FromFeature(feature1);

            // Basic test. Make sure all sections are present in FeatureAsset.
            // FA uses a 2x grid, so we multiply all inputs by 2 to verify.

            ValidateSection(featureAsset1, new Vector2Int(0, 0));
            ValidateSection(featureAsset1, new Vector2Int(1, 0));
            ValidateSection(featureAsset1, new Vector2Int(2, 0));


            /* -1012345
             *  o|o|o|o
             *  -X-X-X-
             *  o|o|o|o 
             */

            // Middle
            ValidateExternalLink(featureAsset1, new Vector2Int(-1, 0));
            ValidateInternalLink(featureAsset1, new Vector2Int(1, 0)); // Should be internal
            ValidateInternalLink(featureAsset1, new Vector2Int(3, 0)); // Should be internal
            ValidateExternalLink(featureAsset1, new Vector2Int(5, 0));

            // Top row
            ValidateExternalLink(featureAsset1, new Vector2Int(0, 1));
            ValidateExternalLink(featureAsset1, new Vector2Int(2, 1));
            ValidateExternalLink(featureAsset1, new Vector2Int(4, 1));

            // Bottom row
            ValidateExternalLink(featureAsset1, new Vector2Int(0, -1));
            ValidateExternalLink(featureAsset1, new Vector2Int(2, -1));
            ValidateExternalLink(featureAsset1, new Vector2Int(4, -1));

            // Invalid links
            ValidateMissingLink(featureAsset1, new Vector2Int(-1, 1));
            ValidateMissingLink(featureAsset1, new Vector2Int(1, 1));
            ValidateMissingLink(featureAsset1, new Vector2Int(3, 1));
            ValidateMissingLink(featureAsset1, new Vector2Int(5, 1));
            ValidateMissingLink(featureAsset1, new Vector2Int(-1, -1));
            ValidateMissingLink(featureAsset1, new Vector2Int(1, -1));
            ValidateMissingLink(featureAsset1, new Vector2Int(3, -1));
            ValidateMissingLink(featureAsset1, new Vector2Int(5, -1));
        }

        [Test]
        public void CanConvertFromFeature_Square()
        {

            // Feature is a 2x2 square.
            Feature feature = new Feature();
            feature.Add(0, 0);
            feature.Add(1, 0);
            feature.Add(0, 1);
            feature.Add(1, 1);

            FeatureAsset featureAsset = FeatureAsset.FromFeature(feature);

            // Basic test- are sections present?
            ValidateSection(featureAsset, new Vector2Int(0, 0));
            ValidateSection(featureAsset, new Vector2Int(1, 0));
            ValidateSection(featureAsset, new Vector2Int(0, 1));
            ValidateSection(featureAsset, new Vector2Int(1, 1));

            /*
             * Layout of the FeatureAsset grid, for reference.
             * 
             *  3 o | o | o
             *  2 - r - r -
             *  1 o | x | o
             *  0 - r - r -
             * -1 o | o | o
             *   -1 0 1 2 3
             */

            ValidateExternalLink(featureAsset, new Vector2Int(0, 3)); // Top
            ValidateExternalLink(featureAsset, new Vector2Int(2, 3));
            ValidateExternalLink(featureAsset, new Vector2Int(-1, 0)); // Left
            ValidateExternalLink(featureAsset, new Vector2Int(-1, 2));
            ValidateExternalLink(featureAsset, new Vector2Int(0, -1)); // Bottom
            ValidateExternalLink(featureAsset, new Vector2Int(2, -1));
            ValidateExternalLink(featureAsset, new Vector2Int(3, 0)); // Right
            ValidateExternalLink(featureAsset, new Vector2Int(3, 2));

            ValidateInternalLink(featureAsset, new Vector2Int(1, 2)); // Top
            ValidateInternalLink(featureAsset, new Vector2Int(0, 1)); // Left
            ValidateInternalLink(featureAsset, new Vector2Int(1, 0)); // Bottom
            ValidateInternalLink(featureAsset, new Vector2Int(2, 1)); // Right
            ValidateInternalLink(featureAsset, new Vector2Int(1, 1)); // Middle

            ValidateMissingLink(featureAsset, new Vector2Int(-1, -1));
            ValidateMissingLink(featureAsset, new Vector2Int(1, -1));
            ValidateMissingLink(featureAsset, new Vector2Int(3, -1));
            ValidateMissingLink(featureAsset, new Vector2Int(-1, 1));
            //ValidateMissingLink(featureAsset, new Vector2Int(1, 1)); // Middle
            ValidateMissingLink(featureAsset, new Vector2Int(3, 1));
            ValidateMissingLink(featureAsset, new Vector2Int(-1, 3));
            ValidateMissingLink(featureAsset, new Vector2Int(1, 3));
            ValidateMissingLink(featureAsset, new Vector2Int(3, 3));
        }

        [Test]
        public void CanConvertFromFeature_Donut()
        {

            // Feature is a 3x3 square with a missing center.
            Feature feature = new Feature();
            feature.Add(0, 0);
            feature.Add(1, 0);
            feature.Add(2, 0);

            feature.Add(0, 1);
            //feature.Add(1, 1);
            feature.Add(2, 1);

            feature.Add(0, 2);
            feature.Add(1, 2);
            feature.Add(2, 2);

            FeatureAsset featureAsset = FeatureAsset.FromFeature(feature);

            // Basic test- are sections present?
            //ValidateSection(featureAsset, new Vector2Int(0, 0));
            //ValidateSection(featureAsset, new Vector2Int(1, 0));
            //ValidateSection(featureAsset, new Vector2Int(0, 1));
            //ValidateSection(featureAsset, new Vector2Int(1, 1));

            /*
             *  5 o | o | o | o
             *  4 - r - r - r -
             *  3 o | o | o | o
             *  2 - r - o - r -
             *  1 o | o | o | o
             *  0 - r - r - r -
             * -1 o | o | o | o
             *   -1 0 1 2 3 4 5
             */

            // Inside the donut, we should have external links
            ValidateExternalLink(featureAsset, new Vector2Int(2, 1));
            ValidateExternalLink(featureAsset, new Vector2Int(2, 3));
            ValidateExternalLink(featureAsset, new Vector2Int(1, 2));
            ValidateExternalLink(featureAsset, new Vector2Int(3, 2));

            // More tests should go here
        }

        [Test]
        public void CanConvertFromFeature_Diagonal()
        {
            // Feature is a diagonal line. Shouldn't have a link on the diagonal.
            Feature feature = new Feature();
            feature.Add(0, 0);
            feature.Add(1, 1);

            FeatureAsset featureAsset = FeatureAsset.FromFeature(feature);

            ValidateMissingLink(featureAsset, new Vector2Int(1, 1));

            // More tests should go here
        }

        // TODO test UpdateLinks
        // TODO optionally test a longer chain of conversions (vs. just Feature -> FA -> Feature)
    }
}