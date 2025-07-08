using UnityEngine;

public enum PieceType { Pawn, Rook, Knight, Bishop, Queen, King }
public enum PieceColor { White, Black }

public class ChessPiece : MonoBehaviour
{
    public PieceType type;
    public PieceColor color;
    public int currentRow;
    public int currentCol;

    private Renderer pieceRenderer;
    public Material normalMaterial;
    public Material highlightMaterial;

    private void Start()
    {
        pieceRenderer = GetComponent<Renderer>();
        if (pieceRenderer != null && normalMaterial != null)
        {
            pieceRenderer.material = normalMaterial;
        }
    }

    public void Highlight(bool isOn)
    {
        if (pieceRenderer != null && highlightMaterial != null && normalMaterial != null)
        {
            pieceRenderer.material = isOn ? highlightMaterial : normalMaterial;
        }
    }
}
