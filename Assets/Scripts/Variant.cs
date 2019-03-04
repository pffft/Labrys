using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys
{
    /// <summary>
    /// A static class maintaining lookup tables for Variants to their IDs.
    /// </summary>
    public static class Variant
    {
        /// <summary>
        /// Maps ID numbers to variant strings.
        /// </summary>
        public static Dictionary<short, string> IDToVariant = new Dictionary<short, string>();


        /// <summary>
        /// Maps variant strings to their numerical ID.
        /// </summary>
        public static Dictionary<string, short> VariantToID = new Dictionary<string, short>();
    }
}
