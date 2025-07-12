using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public ChessPiece[,] pieceBoard = new ChessPiece[8, 8];
    public ChessPiece selectedPiece;
    public PieceColor currentTurn = PieceColor.White;

    private readonly List<Square> highlightedSquares = new();
    private readonly List<IKingCheckObserver> checkObservers = new();

    private bool isMoving = false;
    private ChessPiece lastCheckedKing = null;
    public bool enableCameraSwitch = true; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterObserver(IKingCheckObserver observer)
    {
        if (!checkObservers.Contains(observer))
            checkObservers.Add(observer);
    }

    public void UpdatePieceColliders()
    {
        foreach (ChessPiece piece in pieceBoard)
        {
            if (piece == null) continue;
            piece.SetColliderEnabled(piece.color == currentTurn);
        }
    }

    public void SelectPiece(ChessPiece piece)
    {
        if (piece.color != currentTurn) return;

        if (selectedPiece == piece)
        {
            DeselectPiece();
            return;
        }

        List<Vector2Int> validMoves = GetValidMoves(piece);
        if (validMoves.Count == 0)
        {
            IPieceAnimation shake = new ShakeAnimation(0.25f, 0.15f);
            StartCoroutine(shake.Play(piece));
            Debug.Log($"⛔ {piece.type} không có nước đi hợp lệ.");
            CursorManager.Instance.SetDefault();

            //AudioManager.Instance.PlayInvalidMoveSound(); // 🔊 âm khi đi sai

            return;
        }

        DeselectPiece();
        selectedPiece = piece;
        selectedPiece.Highlight(true);
        CursorManager.Instance.SetHand();
        HighlightValidMoves(validMoves);
    }

    public void TryMoveTo(int row, int col)
    {
        if (selectedPiece == null) return;

        isMoving = true;

        Vector2Int target = new(row, col);
        if (!IsMoveLegal(selectedPiece, target))
        {
            CursorManager.Instance.SetDefault();
            //AudioManager.Instance.PlayInvalidMoveSound(); // 🔊 sai luật
            return;
        }

        ChessPiece targetPiece = pieceBoard[row, col];
        HandleCapture(targetPiece);
        MovePiece(selectedPiece, row, col);

        AudioManager.Instance.PlayMoveSound(); // 🔊 di chuyển

        if (HandlePromotionIfNeeded(selectedPiece))
        {
            ClearHighlights();
            return;
        }

        DeselectPiece();
        NotifyIfCheck();

        PieceColor opponent = currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
        if (IsCheckmate(opponent))
        {
            EndGame(currentTurn);
            return;
        }

        StartCoroutine(SwitchTurnWithCamera());
    }

    public void EndGame(PieceColor winner)
    {
        Debug.Log($"🏁 {winner} thắng!");

        AudioManager.Instance.PlayWinSound(); // 🔊 kết thúc game

        isMoving = false;
        Time.timeScale = 0f;

        foreach (ChessPiece piece in pieceBoard)
        {
            if (piece != null)
                piece.ResetHighlight();
        }

        // UIManager.Instance.ShowWinScreen(winner); // nếu có UI
    }

    public void DeselectPiece()
    {
        if (selectedPiece != null)
        {
            selectedPiece.Highlight(false);
            ClearHighlights();
            selectedPiece = null;
            CursorManager.Instance.SetDefault();
        }
    }

    public List<Vector2Int> GetValidMoves(ChessPiece piece)
    {
        List<Vector2Int> moves = new();
        var rule = MoveRuleFactory.GetRule(piece.type);
        if (rule == null) return moves;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (row == piece.currentRow && col == piece.currentCol) continue;
                if (!rule.IsValidMove(piece, row, col, pieceBoard)) continue;

                if (SimulateMoveSafe(piece, row, col))
                    moves.Add(new Vector2Int(row, col));
            }
        }

        return moves;
    }

    private bool SimulateMoveSafe(ChessPiece piece, int targetRow, int targetCol)
    {
        ChessPiece[,] tempBoard = new ChessPiece[8, 8];
        System.Array.Copy(pieceBoard, tempBoard, pieceBoard.Length);

        ChessPiece tempPiece = piece;
        ChessPiece oldTarget = tempBoard[targetRow, targetCol];

        tempBoard[piece.currentRow, piece.currentCol] = null;
        tempBoard[targetRow, targetCol] = tempPiece;

        int oldRow = piece.currentRow;
        int oldCol = piece.currentCol;

        piece.currentRow = targetRow;
        piece.currentCol = targetCol;

        bool isSafe = !IsKingInCheck(piece.color, tempBoard);

        piece.currentRow = oldRow;
        piece.currentCol = oldCol;
        return isSafe;
    }

    private bool IsMoveLegal(ChessPiece piece, Vector2Int target)
    {
        return GetValidMoves(piece).Contains(target);
    }

    private void HandleCapture(ChessPiece target)
    {
        if (target != null && target.color != selectedPiece.color)
        {
            target.DisableCollider();
            CaptureManager.Instance.CapturePiece(target);
            pieceBoard[target.currentRow, target.currentCol] = null;

            // (Optional) play capture sound if added to AudioManager
            // AudioManager.Instance.PlayCaptureSound();
        }
    }

    private void MovePiece(ChessPiece piece, int row, int col)
    {
        int oldRow = piece.currentRow;
        int oldCol = piece.currentCol;

        // Nhập thành
        if (piece.type == PieceType.King && Mathf.Abs(col - oldCol) == 2)
        {
            int rookStartCol = col > oldCol ? 7 : 0;
            int rookTargetCol = col > oldCol ? col - 1 : col + 1;
            ChessPiece rook = pieceBoard[row, rookStartCol];

            if (rook != null)
            {
                pieceBoard[row, rookStartCol] = null;
                pieceBoard[row, rookTargetCol] = rook;
                rook.transform.position = BoardManager.Instance.GetWorldPosition(row, rookTargetCol);
                rook.currentCol = rookTargetCol;
                rook.UpdateOriginalPosition();
                rook.hasMoved = true;
            }
        }

        pieceBoard[oldRow, oldCol] = null;
        piece.transform.position = BoardManager.Instance.GetWorldPosition(row, col);
        piece.currentRow = row;
        piece.currentCol = col;
        pieceBoard[row, col] = piece;
        piece.UpdateOriginalPosition();
        piece.hasMoved = true;
    }

    private bool HandlePromotionIfNeeded(ChessPiece piece)
    {
        if (piece.type == PieceType.Pawn && (piece.currentRow == 0 || piece.currentRow == 7))
        {
            PromotionManager.Instance.StartPromotion(piece);
            // Optional: AudioManager.Instance.PlayPromoteSound();
            return true;
        }
        return false;
    }

    public void NotifyIfCheck()
    {
        PieceColor opponent = currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
        ChessPiece currentKing = FindKing(opponent, pieceBoard);
        if (currentKing == null) return;

        bool isInCheck = IsKingInCheck(opponent, pieceBoard);

        if (isInCheck)
        {
            currentKing.Highlight(false, true);
            StartCoroutine(new ShakeAnimation(0.25f, 0.1f).Play(currentKing));
            AudioManager.Instance.PlayCheckSound(); // 🔊 chiếu tướng

            foreach (var observer in checkObservers)
                observer.OnKingInCheck(opponent);

            lastCheckedKing = currentKing;
        }
        else
        {
            if (lastCheckedKing != null)
            {
                lastCheckedKing.ResetHighlight();
                lastCheckedKing = null;
            }
        }
    }

    private bool IsKingInCheck(PieceColor color, ChessPiece[,] board)
    {
        ChessPiece king = FindKing(color, board);
        if (king == null) return false;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                ChessPiece attacker = board[row, col];
                if (attacker != null && attacker.color != color)
                {
                    var rule = MoveRuleFactory.GetRule(attacker.type);
                    if (rule != null && rule.IsValidMove(attacker, king.currentRow, king.currentCol, board))
                        return true;
                }
            }
        }
        return false;
    }

    private ChessPiece FindKing(PieceColor color, ChessPiece[,] board)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                ChessPiece piece = board[row, col];
                if (piece != null && piece.color == color && piece.type == PieceType.King)
                    return piece;
            }
        }
        return null;
    }

    private void HighlightValidMoves(List<Vector2Int> positions)
    {
        ClearHighlights();
        foreach (Vector2Int pos in positions)
        {
            GameObject squareObj = BoardManager.Instance.squares[pos.x, pos.y];
            Square square = squareObj.GetComponent<Square>();
            square.SetHighlight(true);
            highlightedSquares.Add(square);

            ChessPiece target = pieceBoard[pos.x, pos.y];
            if (target != null && target.color != selectedPiece.color)
                target.Highlight(false, true);
        }
    }

    public bool IsSquareUnderAttack(int row, int col, PieceColor defenderColor)
    {
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                ChessPiece attacker = pieceBoard[r, c];
                if (attacker != null && attacker.color != defenderColor)
                {
                    var rule = MoveRuleFactory.GetRule(attacker.type);
                    if (rule != null && rule.IsValidMove(attacker, row, col, pieceBoard))
                        return true;
                }
            }
        }
        return false;
    }

    public bool IsCheckmate(PieceColor color)
    {
        if (!IsKingInCheck(color, pieceBoard)) return false;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                ChessPiece piece = pieceBoard[row, col];
                if (piece != null && piece.color == color)
                {
                    var moves = GetValidMoves(piece);
                    if (moves.Count > 0)
                        return false;
                }
            }
        }

        return true;
    }

    public void SwitchTurnAfterPromotion()
    {
        StartCoroutine(SwitchTurnWithCamera());
    }

    private IEnumerator SwitchTurnWithCamera()
    {
        isMoving = true;
        currentTurn = currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;

        if (enableCameraSwitch)
            CameraSwitcher.Instance.SwitchTurn(currentTurn);

        yield return new WaitForSeconds(1f);

        UpdatePieceColliders();
        isMoving = false;

        Debug.Log($"Turn: {currentTurn}");
    }


    private void ClearHighlights()
    {
        foreach (Square square in highlightedSquares)
        {
            square.SetHighlight(false);
            ChessPiece piece = pieceBoard[square.row, square.col];
            if (piece != null)
                piece.ResetHighlight();
        }
        highlightedSquares.Clear();
    }

    public bool IsMoving() => isMoving;
}
