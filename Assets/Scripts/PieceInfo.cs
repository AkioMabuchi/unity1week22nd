using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class PieceInfo
{
    public Texture2D texture;
    public Vector2Int position;
    public List<int> neighbours;
    public bool canPut;
}
