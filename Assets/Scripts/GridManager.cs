using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int rows = 1; // Satýr sayýsý
    public int columns = 4; // Sütun sayýsý
    public float cellWidth = 2f; // Hücre geniþliði
    public float cellHeight = 2f; // Hücre yüksekliði

    public Vector3 startPos;

    private HashSet<int> occupiedCells = new HashSet<int>();

    private void Start()
    {
        startPos = transform.position;
    }

    public Vector3 GetNextAvailableSpawnPosition(Transform cameraTransform)
    {
        // Boþ hücreyi bul
        for (int i = 0; i < rows * columns; i++)
        {
            if (!occupiedCells.Contains(i))
            {
                // Bu hücreyi iþaretle
                occupiedCells.Add(i);

                // Satýr ve sütunu hesapla
                int column = i % columns;
                int row = i / columns;

                // Grid'deki pozisyonu hesapla
                Vector3 localOffset = new Vector3(column * cellWidth, 0, row * cellHeight);

                // Kameranýn yönüne göre pozisyonu ayarla
                Vector3 worldOffset = cameraTransform.rotation * localOffset;

                // Pozisyonu döndür
                return startPos + worldOffset;
            }
        }

        // Eðer tüm hücreler doluysa
        Debug.LogWarning("Tüm hücreler dolu.");
        return startPos; // Yedek pozisyon döndürebilirsiniz
    }

    public void FreeCell(Vector3 position, Transform cameraTransform)
    {
        // Pozisyondan satýr ve sütun hesapla ve hücreyi boþalt
        Vector3 localOffset = Quaternion.Inverse(cameraTransform.rotation) * (position - startPos);
        int column = Mathf.RoundToInt(localOffset.x / cellWidth);
        int row = Mathf.RoundToInt(localOffset.z / cellHeight);
        int cellIndex = row * columns + column;

        // Hücreyi tekrar kullanýlabilir hale getir
        occupiedCells.Remove(cellIndex);
    }
}
