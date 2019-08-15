using System.Collections.Generic;

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
        private readonly short variantID;
        private Dictionary<string, string> metadata;

        public Section(Connection allowedConnections = Connection.All, Connection externalConnections = Connection.All, string variant = "default")
        {
            this.internalConnections = allowedConnections;
            this.externalConnections = externalConnections;
            metadata = null;

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
        /// Add some key-value-pair metadata to this section. If metadata already exists, then it will be overwritten.
        /// </summary>
        /// <param name="metadata">A dictionary of metadata to tie to this section</param>
        public void SetMetadata(IDictionary<string, string> metadata)
        {
            this.metadata = new Dictionary<string, string>(metadata);
        }

        public IEnumerable<string> GetMetadataNames()
        {
            return metadata.Keys;
        }

        /// <summary>
        /// Get a string value from this section's list of metadata. Returns null if an entry with the given name could
        /// not be found.
        /// </summary>
        /// <param name="name">The name of the metadata field</param>
        /// <returns>A string value or null</returns>
        public string GetString(string name)
        {
            if (metadata != null && metadata.TryGetValue(name, out string val))
            {
                return val;
            }
            return null;
        }

        /// <summary>
        /// Get a boolean value from this section's list of metadata. Returns null if an entry with the given name could
        /// be found, or if the field corresponding to the given name is not a boolean.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>True or false or null</returns>
        public bool? GetBool(string name)
        {
            string strVal = GetString(name);
            return (strVal == null) ? null : bool.TryParse(strVal, out bool boolVal) ? (bool?)boolVal : null;
        }

        /// <summary>
        /// Get an integer value from this section's list of metadata. Returns null if an entry with the given name could
        /// be found, or if the field corresponding to the given name is not an integer.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>An integer or null</returns>
        public int? GetInt(string name)
        {
            string strVal = GetString(name);
            return (strVal == null) ? null : int.TryParse(strVal, out int intVal) ? (int?)intVal : null;
        }

        /// <summary>
        /// Get a floating point value from this section's list of metadata. Returns null if an entry with the given name could
        /// be found, or if the field corresponding to the given name is not a float.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A float or null</returns>
        public float? GetFloat(string name)
        {
            string strVal = GetString(name);
            return (strVal == null) ? null : float.TryParse(strVal, out float floatVal) ? (float?)floatVal : null;
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

        public override string ToString() 
        {
            return $"(internal: {internalConnections}; external: {externalConnections}; variant: {GetVariant()})";
        }
    }
}
