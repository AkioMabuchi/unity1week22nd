using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public static class PieceModel
{
    private static readonly ReactiveProperty<int[]> _pieceMap = new(Array.Empty<int>());
    private static readonly ReactiveProperty<int> _maxPieceSizeX = new();
    public static IReadOnlyReactiveProperty<int> MaxPieceSizeX => _maxPieceSizeX;
    private static readonly ReactiveProperty<int> _maxPieceSizeY = new();
    private static readonly ReactiveProperty<int> _pieceSizeX = new();
    private static readonly ReactiveProperty<int> _pieceSizeY = new();
    private static readonly ReactiveProperty<int> _pieceAmount = new(0);
    public static IReadOnlyReactiveProperty<int> PieceAmount => _pieceAmount;
    private static readonly ReactiveProperty<Texture2D[]> _pieceTextures = new(Array.Empty<Texture2D>());
    public static IReadOnlyReactiveProperty<Texture2D[]> PieceTextures => _pieceTextures; 

    public static void GeneratePieces(PictureInfo picture)
    {
        var pieceSizeX = picture.texture.width / picture.sizeX;
        var pieceSizeY = picture.texture.height / picture.sizeY;
        var maxPieceSizeX = pieceSizeX >= 16 ? pieceSizeX + 6 : pieceSizeX + 4;
        var maxPieceSizeY = pieceSizeY >= 16 ? pieceSizeY + 6 : pieceSizeY + 4;
        
        var pieceMap = new int[picture.texture.width * picture.texture.height];
        var colorMap = new Color[picture.texture.width * picture.texture.height];
        for (var i = 0; i < pieceMap.Length; i++)
        {
            var mapX = i % picture.texture.width;
            var mapY = i / picture.texture.width;
            colorMap[i] = picture.texture.GetPixel(mapX, mapY);
            var x = mapX / pieceSizeX;
            var y = mapY / pieceSizeY;
            var index = y * picture.sizeX + x;
            pieceMap[i] = index;
        }

        var pieceAmount = picture.sizeX * picture.sizeY;

        var pieceTextures = new Texture2D[pieceAmount];
        for (var i = 0; i < pieceAmount; i++)
        {
            pieceTextures[i] = new Texture2D(maxPieceSizeX, maxPieceSizeY, TextureFormat.RGBA32, false);
            pieceTextures[i].filterMode = FilterMode.Point;
            for (var x = 0; x < maxPieceSizeX; x++)
            {
                for (var y = 0; y < maxPieceSizeY; y++)
                {
                    pieceTextures[i].SetPixel(x, y, Color.clear);
                }
            }
        }

        for (var x = 0; x < picture.sizeX - 1; x++)
        {
            for (var y = 0; y < picture.sizeY; y++)
            {
                var currentIndex = x + y * picture.sizeX;
                var nextIndexX = currentIndex + 1;
                var dig = (x + y) % 2 == 0;
                for (var nx = 0; nx < 3; nx++)
                {
                    for (var ny = 0; ny < 4; ny++)
                    {
                        var ni = dig ? nextIndexX : currentIndex;
                        var mapX = dig ? (x + 1) * pieceSizeX - nx - 1 : (x + 1) * pieceSizeX + nx;
                        var mapY = y * pieceSizeY + pieceSizeY / 2 - 2 + ny;
                        var mapI = mapY * picture.texture.width + mapX;
                        pieceMap[mapI] = ni;
                    }
                }
            }
        }
        
        for (var x = 0; x < picture.sizeX; x++)
        {
            for (var y = 0; y < picture.sizeY-1; y++)
            {
                var currentIndex = x + y * picture.sizeX;
                var nextIndexY = currentIndex + picture.sizeX;
                var dig = (x + y) % 2 == 1;
                for (var nx = 0; nx < 4; nx++)
                {
                    for (var ny = 0; ny < 3; ny++)
                    {
                        var ni = dig ? nextIndexY : currentIndex;
                        var mapX = x * pieceSizeX + pieceSizeX / 2 - 2 + nx;
                        var mapY = dig ? (y + 1) * pieceSizeY - ny - 1 : (y + 1) * pieceSizeY + ny;
                        var mapI = mapY * picture.texture.width + mapX;
                        pieceMap[mapI] = ni;
                    }
                }
            }
        }

        for (var i = 0; i < pieceMap.Length; i++)
        {
            var mapX = i % picture.texture.width;
            var mapY = i / picture.texture.width;
            var index = pieceMap[i];
            var indexX = index % picture.sizeX;
            var indexY = index / picture.sizeX;
            var baseX = (pieceSizeX - maxPieceSizeX) / 2 + indexX * pieceSizeX;
            var baseY = (pieceSizeY - maxPieceSizeY) / 2 + indexY * pieceSizeY;
            var texX = mapX - baseX;
            var texY = mapY - baseY;
            pieceTextures[index].SetPixel(texX, texY, colorMap[i]);
        }

        _maxPieceSizeX.Value = maxPieceSizeX;
        _pieceAmount.Value = pieceAmount;
        _pieceTextures.Value = pieceTextures;
    }
}
