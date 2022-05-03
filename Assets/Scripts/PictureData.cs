using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PictureInfo
{
    public Texture2D texture;
    public int sizeX;
    public int sizeY;
}

[CreateAssetMenu(menuName = "ScriptableObjects/PictureData")]
public class PictureData : ScriptableObject
{
    [SerializeField] private List<PictureInfo> pictures;
    public IEnumerable<PictureInfo> Pictures => pictures;
}
