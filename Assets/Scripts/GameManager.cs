using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public ChessPiece[,] pieceBoard = new ChessPiece[8, 8];
    public ChessPiece selectedPiece;
    public PieceColor currentTurn = PieceColor.White;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SelectPiece(ChessPiece piece)
    {
        if (piece.color != currentTurn) return;

        // Nếu có quân cũ đang được chọn → bỏ highlight
        if (selectedPiece != null)
        {
            selectedPiece.Highlight(false);
        }

        // Gán và highlight quân mới được chọn
        selectedPiece = piece;
        selectedPiece.Highlight(true);

        Debug.Log($"Selected {piece.color} {piece.type} at {piece.currentRow},{piece.currentCol}");
    }

    public void TryMoveTo(int row, int col)
    {
        if (selectedPiece == null) return;

        // TODO: kiểm tra nước đi hợp lệ ở bước sau

        // Di chuyển quân cờ
        Vector3 targetPos = BoardManager.Instance.GetWorldPosition(row, col);
        pieceBoard[selectedPiece.currentRow, selectedPiece.currentCol] = null;

        selectedPiece.transform.position = targetPos;
        selectedPiece.currentRow = row;
        selectedPiece.currentCol = col;
        pieceBoard[row, col] = selectedPiece;

        // Bỏ highlight và reset chọn
        selectedPiece.Highlight(false);
        selectedPiece = null;

        SwitchTurn();
    }

    private void SwitchTurn()
    {
        currentTurn = currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
        Debug.Log($"Turn: {currentTurn}");
    }
}
