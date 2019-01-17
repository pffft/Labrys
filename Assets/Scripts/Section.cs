using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys
{
    [System.Serializable]
    public class Section
    {
        internal Connection internalConnections;
        internal Connection externalConnections;
        internal string variant;

        public Section(Connection allowedConnections = Connection.All, Connection externalConnections = Connection.All, string variant = "default")
        {
            this.internalConnections = allowedConnections;
            this.externalConnections = externalConnections;
            this.variant = variant;
        }

        public bool CanConnect(Connection other)
        {
            return (internalConnections & other) == other;
        }
    }
}
