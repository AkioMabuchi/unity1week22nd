using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScreen : MonoBehaviour
{
    [SerializeField] private PictureData pictures;

    private void Awake()
    {
        foreach (var picture in pictures.Pictures)
        {
            PieceModel.GeneratePieces(picture);
        }
    }
}
