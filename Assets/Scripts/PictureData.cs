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
    public string name;
    public string title;
    public string author;
}

[CreateAssetMenu(menuName = "ScriptableObjects/PictureData")]
public class PictureData : ScriptableObject
{
    [SerializeField] private List<PictureInfo> pictures;
    public IReadOnlyList<PictureInfo> Pictures => pictures;
}
