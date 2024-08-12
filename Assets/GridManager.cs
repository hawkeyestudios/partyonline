using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int rows = 2; // Satýr sayýsý
    public int columns = 2; // Sütun sayýsý
    public float cellWidth = 2f; // Hücre geniþliði
    public float cellHeight = 2f; // Hücre yüksekliði

    public Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    public Vector3 GetNextSpawnPosition(int playerIndex)
    {
        int row = playerIndex / columns;
        int column = playerIndex % columns;
        return startPos + new Vector3(column * cellWidth, 0, row * cellHeight);
    }
}
