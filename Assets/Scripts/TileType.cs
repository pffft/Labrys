using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An implementation of 2 edge + 2 corner Wang tile set.
/// See: http://www.cr31.co.uk/stagecast/wang/blob.html
/// 
/// In practice, each Section can be seen as having 8 connections (cardinal +
/// diagonal directions). In this file, the convention is that a connection
/// starts facing East, and additional connections are added counterclockwise.
/// (i.e., the corner piece has two connections, so is represented as East and
/// then North).
/// </summary>
public struct TileType 
{
    // 0 connections
    /// <summary>
    /// Room with no connections.
    /// </summary>
    public static TileType EMPTY = new TileType(nameof(EMPTY), 1, 0b0000_0000);

    // 1 connection
    /// <summary>
    /// A one connection dead end hallway.
    /// </summary>
    public static TileType DEAD_END = new TileType(nameof(DEAD_END), 4, 0b0000_0001);

    // 2 connections
    public static TileType CORNER_CORRIDOR = new TileType(nameof(CORNER_CORRIDOR), 4, 0b0000_0101);
    public static TileType CORNER_ROOM = new TileType(nameof(CORNER_ROOM), 4, 0b0000_0111);
    public static TileType HALLWAY = new TileType(nameof(HALLWAY), 2, 0b_0001_0001);

    // 3 connections
    public static TileType T_CORRIDOR = new TileType(nameof(T_CORRIDOR), 4, 0b0001_0101);
    public static TileType T_CORRIDOR_LEFT = new TileType(nameof(T_CORRIDOR_LEFT), 4, 0b0001_1101);
    public static TileType T_CORRIDOR_RIGHT = new TileType(nameof(T_CORRIDOR_RIGHT), 4, 0b0001_0111);
    public static TileType T_ROOM = new TileType(nameof(T_ROOM), 4, 0b0001_1111);

    // 4 connections
    public static TileType CROSS_CORRIDOR = new TileType(nameof(CROSS_CORRIDOR), 1, 0b0101_0101);
    public static TileType CROSS_ROOM_CORNER = new TileType(nameof(CROSS_ROOM_CORNER), 4, 0b0101_0111);
    public static TileType CROSS_ROOM_WALL = new TileType(nameof(CROSS_ROOM_WALL), 4, 0b0101_1111);
    public static TileType CROSS_ROOM_STRAIT = new TileType(nameof(CROSS_ROOM_STRAIT), 2, 0b0111_0111);
    public static TileType CROSS_ROOM_L = new TileType(nameof(CROSS_ROOM_L), 4, 0b0111_1111);
    public static TileType CROSS_ROOM_CENTER = new TileType(nameof(CROSS_ROOM_CENTER), 1, 0b1111_1111);

    public static readonly TileType[] TypeList = {
        EMPTY,
        DEAD_END,
        CORNER_CORRIDOR,
        CORNER_ROOM,
        HALLWAY,
        T_CORRIDOR,
        T_CORRIDOR_LEFT,
        T_CORRIDOR_RIGHT,
        T_ROOM,
        CROSS_CORRIDOR,
        CROSS_ROOM_CORNER,
        CROSS_ROOM_WALL,
        CROSS_ROOM_STRAIT,
        CROSS_ROOM_L,
        CROSS_ROOM_CENTER
    };

    public string Name
    {
        get; private set;
    }

    /// <summary>
    /// How many rotational variants do we have?
    /// This will always be either 1, 2, or 4.
    /// </summary>
    private readonly int numRotations;

    /// <summary>
    /// Defines the basic structure of this TileType. The structures are built
    /// so that their first connection faces East, and other connections are
    /// counterclockwise from there.
    /// 
    /// The bits 0 through 7 represent the directions starting with East = 0, 
    /// moving counterclockwise.
    /// 
    /// The cardinal directions represent connections, while the diagonal
    /// directions represent only physical adjacency. 
    /// 
    /// For a Section to be equal to this TileType, its adjacency byte is masked
    /// by the importance mask. If the result XOR this blueprint is equal to 0,
    /// then it is a match. This is checked against every rotational variant.
    /// </summary>
    private readonly byte tileBlueprint;

    private TileType(string name, int numRotations, byte blueprint)
    {
        this.Name = name;
        this.numRotations = numRotations;
        this.tileBlueprint = blueprint;
    }

    /// <summary>
    /// Rotate the input byte left by the specified amount of places.
    /// 
    /// This corresponds to rotating this TileType counterclockwise by 45 degree
    /// intervals.
    /// </summary>
    /// <returns>The rotated byte.</returns>
    /// <param name="input">Input byte.</param>
    /// <param name="amount">Amount of places to shift.</param>
    private static byte Rotate(byte input, int amount) 
    {
        byte shifted = (byte)(input << amount);
        byte overflow = (byte)(input >> (8 - amount));

        return (byte)(shifted | overflow);
    }

    public static (TileType, int) GetTileType(Connection adjacentRooms, Connection allowedConnections = Connection.All) 
    {
        // Check every tile
        for (int i = 0; i < TypeList.Length; i++) 
        {
            // Check every rotation of every tile
            for (int rotationNumber = 0; rotationNumber < TypeList[i].numRotations; rotationNumber++) 
            {
                byte rotatedBlueprint = Rotate(TypeList[i].tileBlueprint, 2 * rotationNumber);

                // allowedConnections tells us which neighbors we can ignore. We
                // mask it here to determine which Wang tile to use.
                // 
                // If the resulting byte matches the current TileType, we return
                // the TileType and rotation.
                if ((((byte)adjacentRooms & (byte)allowedConnections) ^ rotatedBlueprint) == 0) 
                {
                    return (TypeList[i], rotationNumber);
                }
            }
        }

        // Fail if the byte is invalid. This shouldn't ever run.
        throw new System.Exception("Failed to get tile type for connection type: " + adjacentRooms);
    }

    /// <summary>
    /// Gets the index of the tile.
    /// </summary>
    /// <returns>The tile index.</returns>
    /// <param name="type">The TileType to check.</param>
    public static int GetTileIndex(TileType type) 
    {
        for (int i = 0; i < TypeList.Length; i++)
        {
            if (TypeList[i].Equals(type))
            {
                return i;
            }
        }

        // This shouldn't be hit; you can't define any TileTypes outside the array.
        throw new System.Exception("Failed to get index for tile type with blueprint: " + type.tileBlueprint + ".");
    }
}

