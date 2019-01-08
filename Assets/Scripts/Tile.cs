using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An individual 1x1 grid element to be placed in the world.
/// </summary>
[CreateAssetMenu(fileName="NewTile", menuName="Labrys/RoomTile")]
public class Tile : ScriptableObject
{
    /// <summary>
    /// The type of the connection.
    /// </summary>
    // TODO: Make a custom inspector so that this shows up in the inspector. See SectionInspector.
    [Tooltip("The connection type.")]
    public TileType type;

    /// <summary>
    /// The variant of this Tile. This is used with the TileType to uniquely
    /// determine this Tile in TileSet.
    /// </summary>
    public string variant = "none";

    /// <summary>
    /// The actual physical model that represents this Tile.
    /// TODO: The GameObject can be set in the inspector, but not in the
    /// generator. Vice-versa for the string. Enforce this somehow.
    /// </summary>
    public GameObject prefab;

    /// <summary>
    /// The location of the prefab, used for autogenerating Tiles.
    /// TODO: as above.
    /// </summary>
    public string prefabString;
}
