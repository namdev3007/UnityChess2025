using UnityEngine;
using Unity.Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public static CameraSwitcher Instance;

    public CinemachineCamera camWhite;
    public CinemachineCamera camBlack;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SwitchTurn(PieceColor currentTurn)
    {
        if (currentTurn == PieceColor.White)
        {
            camWhite.Priority = 10;
            camBlack.Priority = 5;
        }
        else
        {
            camWhite.Priority = 5;
            camBlack.Priority = 10;
        }
    }
}
