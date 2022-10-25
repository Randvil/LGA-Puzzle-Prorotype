using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGridPositionable
{
    public Vector2Int PositionOnGrid { get; set; }
}
