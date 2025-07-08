using UnityEngine;

public class Square : MonoBehaviour
{
    public int row; // 0 to 7
    public int col; // 0 to 7

    private void OnMouseDown()
    {
        string colLetter = ((char)('a' + col)).ToString(); // chuyển số 0–7 thành a–h
        int rowNumber = row + 1; // chuyển 0–7 thành 1–8
        //Debug.Log($"Clicked square at {colLetter}{rowNumber} (row={row}, col={col})");
    }

    public void SetPosition(int r, int c)
    {
        row = r;
        col = c;
    }
}
