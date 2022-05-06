using System;

[Serializable]
public class PicturePieceSaveInfo
{
    public int time;
    public int pieceSizeX;
    public int pieceSizeY;
    public int maxPieceSizeX;
    public int maxPieceSizeY;
    public int putRange;
    public int[] pieceMap;
    public PieceSaveInfo[] pieces;
}

[Serializable]
public class PieceSaveInfo
{
    public int belongCode;
    public int positionX;
    public int positionY;
    public int basePositionX;
    public int basePositionY;
    public int[] neighbours;
    public bool canPut;
}
