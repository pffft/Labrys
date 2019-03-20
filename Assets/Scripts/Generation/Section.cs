using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Generation
{
    /// <summary>
    /// A struct representing a single grid element during generation. This will
    /// map 1:1 to a Tile when generation is completed.
    /// </summary>
    [System.Serializable]
    public struct Section
    {
        public Connection internalConnections;
        public Connection externalConnections;
        //public string variant;
        private short variantID;

        public Section(Connection allowedConnections = Connection.All, Connection externalConnections = Connection.All, string variant = "default")
        {
            this.internalConnections = allowedConnections;
            this.externalConnections = externalConnections;
            //this.variant = variant;

            // Look it up in the lookup table
            if (Variant.VariantToID.TryGetValue(variant, out short id)) 
            {
                this.variantID = id;
            }
            // Failed to find; add it to the table and get the new ID
            else 
            {
                short nextID = (short)Variant.IDToVariant.Count;
                Variant.IDToVariant.Add(nextID, variant);
                Variant.VariantToID.Add(variant, nextID);
                this.variantID = nextID;
            }
        }

        /// <summary>
        /// A replacement for parameterless constructor.
        /// </summary>
        /// <returns>The default.</returns>
        public static Section Default() 
        {
            return new Section(Connection.All, Connection.All, "default");
        }

        public bool CanConnect(Connection other)
        {
            return (internalConnections & other) == other;
        }

        /// <summary>
        /// Get the variant of this Section. 
        /// 
        /// Variant names are interned using the Variant class- this looks up the
        /// internal ID we store in this Section object.
        /// </summary>
        /// <returns>The variant.</returns>
        public string GetVariant() 
        {
            return Variant.IDToVariant[variantID];
        }
    }
}
