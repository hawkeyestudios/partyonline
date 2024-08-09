using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafMovement : MonoBehaviour
{
    public float windStrength = 0.5f;  // Rüzgarýn þiddeti
    public float windSpeed = 1.0f;     // Rüzgarýn hýzý
    public Vector3 windDirection = new Vector3(1, 0, 0); // Rüzgarýn yönü (x ve z yönünde)

    private MeshFilter meshFilter;
    private Vector3[] originalVertices;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            originalVertices = meshFilter.mesh.vertices;
        }
    }

    void Update()
    {
        if (meshFilter != null)
        {
            Mesh mesh = meshFilter.mesh;
            Vector3[] vertices = new Vector3[originalVertices.Length];
            originalVertices.CopyTo(vertices, 0);

            float time = Time.time * windSpeed;

            for (int i = 0; i < vertices.Length; i++)
            {
                // Rüzgarýn etkisini yatay düzlemde hesaplayýn
                float windEffect = Mathf.Sin(Vector3.Dot(vertices[i], windDirection) + time) * windStrength;

                // Yalnýzca x ve z eksenlerindeki hareketi hesaplayýn
                vertices[i].x += windEffect * windDirection.x;
                vertices[i].z += windEffect * windDirection.z;
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
        }
    }
}
