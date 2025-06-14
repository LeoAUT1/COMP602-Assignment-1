using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public LayerMask clickableLayer;
    public Board board; // The game board
    private Camera mainCamera; // Reference to the camera


    void Start()
    {
        // Get and store camera reference
        mainCamera = Camera.main;

        // Fallback if no MainCamera tag exists
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
            Debug.LogWarning("No camera tagged 'MainCamera' found. Using first camera found instead.");

            if (mainCamera == null)
                Debug.LogError("No cameras found in scene! ClickManager will not function.");
        }
    }

    void Update()
    {
        // Debug Feautre to move the player to an arbitrary tile
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
            {
                BoardTile boardTile = hit.collider.GetComponent<BoardTile>();
                if (boardTile != null)
                {
                    Player.Instance.SetCurrentBoardTile(boardTile);
                    board.playerAnimator.MovePlayerPieceInstantly(boardTile);
                }
            }
        }
    }
}
