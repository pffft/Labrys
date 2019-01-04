using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileEnum
{
    EMPTY = 0b00__0000_0000__0101_0101,
    DEAD_END = 0b11__0000_0001__0101_0101
}

public static class TileEnumHelper
{
    public static (TileEnum, int) GetTileType(this TileEnum tile, byte adjacencyByte)
    {
        return (TileEnum.EMPTY, 0);
    }
}

public struct TileType 
{
    // 0 connections
    /// <summary>
    /// Room with no connections.
    /// </summary>
    public static TileType EMPTY = new TileType(1, 0b0000_0000, 0b0101_0101);

    // 1 connection
    public static TileType DEAD_END = new TileType(4, 0b0000_0001, 0b0101_0101);

    // 2 connections
    public static TileType CORNER_CORRIDOR = new TileType(4, 0b0000_0101, 0b0101_0111);
    public static TileType CORNER_ROOM = new TileType(4, 0b0000_0111, 0b0101_0111);
    public static TileType HALLWAY = new TileType(2, 0b_0001_0001, 0b0101_0101);

    // 3 connections
    public static TileType T_CORRIDOR = new TileType(4, 0b0001_0101, 0b0101_1111);
    public static TileType T_CORRIDOR_LEFT = new TileType(4, 0b0001_1101, 0b0101_1111);
    public static TileType T_CORRIDOR_RIGHT = new TileType(4, 0b0001_0111, 0b0101_1111);
    public static TileType T_ROOM = new TileType(4, 0b0001_1111, 0b0101_1111);

    // 4 connections
    public static TileType CROSS_CORRIDOR = new TileType(1, 0b0101_0101, 0b1111_1111);
    public static TileType CROSS_ROOM_CORNER = new TileType(4, 0b0101_0111, 0b1111_1111);
    public static TileType CROSS_ROOM_WALL = new TileType(4, 0b0101_1111, 0b1111_1111);
    public static TileType CROSS_ROOM_STRAIT = new TileType(2, 0b0111_0111, 0b1111_1111);
    public static TileType CROSS_ROOM_L = new TileType(4, 0b0111_1111, 0b1111_1111);
    public static TileType CROSS_ROOM_CENTER = new TileType(1, 0b1111_1111, 0b1111_1111);

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

    /// <summary>
    /// Which adjacencies are important to determine which TileType a Section is.
    /// Some adjacencies are "don't care"s, meaning they don't affect which type
    /// the Section is.
    /// 
    /// TODO: note that the importance mask can be computed from the number of adjacent
    /// cardinal connections. We always care about the bits 0, 2, 4, 6; and the first
    /// 0, 1, 2, or 4 unused bits, based on how many "corners" we have. I.e.,
    /// a corner piece has one corner, so we care about the first corner.
    /// 
    /// </summary>
    private readonly byte importanceMask;

    private TileType(int numRotations, byte blueprint, byte importance)
    {
        this.numRotations = numRotations;
        this.tileBlueprint = blueprint;
        this.importanceMask = importance;
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

    public static (TileType, int) GetTileType(byte adjacencyByte) 
    {
        // Check every tile
        for (int i = 0; i < TypeList.Length; i++) 
        {
            // Check every rotation of every tile
            for (int rotationNumber = 0; rotationNumber < TypeList[i].numRotations; rotationNumber++) 
            {
                byte rotatedBlueprint = Rotate(TypeList[i].tileBlueprint, 2 * rotationNumber);
                byte rotatedMask = Rotate(TypeList[i].importanceMask, 2 * rotationNumber);

                // Mask to ignore adjacencies we don't care about. If what remains matches the current
                // TileType's blueprint exactly, then we return the TileType and rotation.
                if (((adjacencyByte & rotatedMask) ^ rotatedBlueprint) == 0) 
                {
                    return (TypeList[i], rotationNumber);
                }
            }
        }

        // Fail if the byte is invalid. This shouldn't ever run.
        throw new System.Exception("Failed to get tile type for byte: " + adjacencyByte);
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
        throw new System.Exception("Failed to get index for tile type with blueprint: " + type.tileBlueprint + " and mask: " + type.importanceMask + ".");
    }
}

