using System.Collections;
using System.Collections.Generic;
using Labrys.Generation;
using UnityEngine;

/// <summary>
/// An interface for a Grid that can be read from, but not modified.
/// </summary>
public interface IReadableGrid
{
    /// <summary>
    /// Getter for Sections in this Grid by Vector2Int.
    /// </summary>
    /// <param name="pos">Position.</param>
    Section? this[Vector2Int pos] { get; }

    /// <summary>
    /// Getter for Sections in this Grid by x and y coordinate.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    Section? this[int x, int y] { get; }

    /// <summary>
    /// Gets an array of every single nonempty cell in this Grid.
    /// </summary>
    /// <returns>The full cells.</returns>
    Vector2Int[] GetOccupiedCells();

    /// <summary>
    /// Gets an array of every occupied cell in this Grid that is connected to
    /// the outside. In other words, every nonempty cell that is not completely
    /// enclosed by other nonempty cells.
    /// </summary>
    /// <returns>The boundary.</returns>
    Vector2Int[] GetBoundary();

    /// <summary>
    /// Returns a Connection object, which represents which neighbors are 
    /// nonempty for a given position.
    /// </summary>
    /// <returns>The physical adjacencies.</returns>
    /// <param name="position">Position.</param>
    Connection GetPhysicalAdjacencies(Vector2Int position);
}
