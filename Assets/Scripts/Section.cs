using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Section
{

    /// <summary>
    /// Which connections are allowed. If a connection is not allowed, and a
    /// room is physically adjacent, it will visually not connect.
    /// </summary>
    public Connection allowedConnections = Connection.All;

    /// <summary>
    /// Which rooms we're physically adjacent to. At runtime, these should be
    /// set for us. For example, if there is a Section North of us, then 
    /// adjacentRooms should have its North flag set to true.
    /// </summary>
    public Connection adjacentRooms = Connection.None;

    /// <summary>
    /// A data structure which contains information on the physical model being
    /// placed at this location, as well as the derived adjacency information.
    /// </summary>
    public Tile tile;

    public void RequestTile() 
    {
        (TileType type, int rotation) = TileType.GetTileType(adjacentRooms, allowedConnections);

        // Here, we would lookup a valid Tile in Tileset for the given TileType.
        // TODO
    }
}
