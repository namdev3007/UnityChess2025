using UnityEngine;

public class KingMoveRule : IMoveRule
{
    public bool IsValidMove(ChessPiece piece, int targetRow, int targetCol, ChessPiece[,] board)
    {
        int dr = Mathf.Abs(targetRow - piece.currentRow);
        int dc = Mathf.Abs(targetCol - piece.currentCol);

        if (dr > 1 || dc > 1) return false;

        ChessPiece target = board[targetRow, targetCol];
        return target == null || target.color != piece.color;
    }
}

