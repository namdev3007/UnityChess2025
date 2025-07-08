using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    public GameObject whitePawnPrefab;
    public GameObject whiteRookPrefab;
    public GameObject whiteKnightPrefab;
    public GameObject whiteBishopPrefab;
    public GameObject whiteQueenPrefab;
    public GameObject whiteKingPrefab;

    public GameObject blackPawnPrefab;
    public GameObject blackRookPrefab;
    public GameObject blackKnightPrefab;
    public GameObject blackBishopPrefab;
    public GameObject blackQueenPrefab;
    public GameObject blackKingPrefab;

    public GameObject blackPieces;
    public GameObject whitePieces;

    private void Start()
    {
        SpawnAllPieces();
    }

    public void SpawnAllPieces()
    {
        // Spawn pawns
        for (int col = 0; col < 8; col++)
        {
            SpawnPiece(whitePawnPrefab, 1, col, PieceColor.White, PieceType.Pawn);
            SpawnPiece(blackPawnPrefab, 6, col, PieceColor.Black, PieceType.Pawn);
        }

        // Rooks
        SpawnPiece(whiteRookPrefab, 0, 0, PieceColor.White, PieceType.Rook);
        SpawnPiece(whiteRookPrefab, 0, 7, PieceColor.White, PieceType.Rook);
        SpawnPiece(blackRookPrefab, 7, 0, PieceColor.Black, PieceType.Rook);
        SpawnPiece(blackRookPrefab, 7, 7, PieceColor.Black, PieceType.Rook);

        // Knights
        SpawnPiece(whiteKnightPrefab, 0, 1, PieceColor.White, PieceType.Knight);
        SpawnPiece(whiteKnightPrefab, 0, 6, PieceColor.White, PieceType.Knight);
        SpawnPiece(blackKnightPrefab, 7, 1, PieceColor.Black, PieceType.Knight);
        SpawnPiece(blackKnightPrefab, 7, 6, PieceColor.Black, PieceType.Knight);

        // Bishops
        SpawnPiece(whiteBishopPrefab, 0, 2, PieceColor.White, PieceType.Bishop);
        SpawnPiece(whiteBishopPrefab, 0, 5, PieceColor.White, PieceType.Bishop);
        SpawnPiece(blackBishopPrefab, 7, 2, PieceColor.Black, PieceType.Bishop);
        SpawnPiece(blackBishopPrefab, 7, 5, PieceColor.Black, PieceType.Bishop);

        // Queens
        SpawnPiece(whiteQueenPrefab, 0, 3, PieceColor.White, PieceType.Queen);
        SpawnPiece(blackQueenPrefab, 7, 3, PieceColor.Black, PieceType.Queen);

        // Kings
        SpawnPiece(whiteKingPrefab, 0, 4, PieceColor.White, PieceType.King);
        SpawnPiece(blackKingPrefab, 7, 4, PieceColor.Black, PieceType.King);
    }

    private void SpawnPiece(GameObject prefab, int row, int col, PieceColor color, PieceType type)
    {
        Vector3 pos = BoardManager.Instance.GetWorldPosition(row, col);
        Quaternion rotation = Quaternion.identity;

        if (type == PieceType.Knight)
        {
            rotation = color == PieceColor.White
                ? Quaternion.Euler(0, 90f, 0)
                : Quaternion.Euler(0, -90f, 0);
        }

        Transform parent = (color == PieceColor.White) ? whitePieces.transform : blackPieces.transform;

        GameObject pieceObj = Instantiate(prefab, pos, rotation, parent);

        ChessPiece piece = pieceObj.GetComponent<ChessPiece>();
        piece.currentRow = row;
        piece.currentCol = col;
        piece.color = color;
        piece.type = type;

        if (pieceObj.GetComponent<PieceSelector>() == null)
            pieceObj.AddComponent<PieceSelector>();

        GameManager.Instance.pieceBoard[row, col] = piece;
    }
}
