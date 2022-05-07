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

    public int PutPieceNum
    {
        get
        {
            var r = 0;
            foreach (var piece in pieces)
            {
                if (piece.belongCode == 2)
                {
                    r++;
                }
            }

            return r;
        }
    }
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
