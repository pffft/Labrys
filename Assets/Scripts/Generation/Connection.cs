using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum Connection : byte
{
    East = 1,
    Northeast = 2,
    North = 4,
    Northwest = 8,
    West = 16,
    Southwest = 32,
    South = 64,
    Southeast = 128,

    None = 0,
    Vertical = North | South,
    Horizontal = East | West,
    Cardinal = East | North | West | South,
    All = Cardinal | Northeast | Northwest | Southeast | Southwest
}