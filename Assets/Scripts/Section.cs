﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A logical grid element. Stores information about which connections
/// it can connect to, and contains a physical Tile element when the
/// generation finishes.
/// </summary>
[System.Serializable]
public class Section
{

    /// <summary>
    /// Which connections are allowed. If a connection is not allowed, and a
    /// room is physically adjacent, it will visually not connect.
    /// </summary>
    public Connection allowedConnections = Connection.All;

    /// <summary>
    /// A data structure which contains information on the physical model being
    /// placed at this location, as well as the derived adjacency information.
    /// </summary>
    public Tile tile;

    public Section(Connection allowedConnections=Connection.All) 
    {
        this.allowedConnections = allowedConnections;
    }

    /// <summary>
    /// Can this Section connect in the provided  directions?
    /// 
    /// If multiple directions are provided (i.e., Connection.All), this returns
    /// true if all directions are valid. 
    /// </summary>
    /// <returns><c>true</c>, if we can connect <c>false</c> otherwise.</returns>
    /// <param name="direction">Direction.</param>
    public bool CanConnect(Connection direction) 
    {
        return (allowedConnections & direction) == direction;
    }
}
