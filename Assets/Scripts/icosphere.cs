using UnityEngine;
using System.Collections.Generic;

public class HighResIcosphere : MonoBehaviour
{
    [Range(0, 7)]
    public int subdivisions = 3;
    public float radius = 1f;

    private void Start()
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateIcosphere(radius, subdivisions);

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));
    }

    Mesh CreateIcosphere(float radius, int subdivisions)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        // Add vertices
        vertices.Add(new Vector3(-1f,  t,  0f).normalized * radius);
        vertices.Add(new Vector3( 1f,  t,  0f).normalized * radius);
        vertices.Add(new Vector3(-1f, -t,  0f).normalized * radius);
        vertices.Add(new Vector3( 1f, -t,  0f).normalized * radius);

        vertices.Add(new Vector3( 0f, -1f,  t).normalized * radius);
        vertices.Add(new Vector3( 0f,  1f,  t).normalized * radius);
        vertices.Add(new Vector3( 0f, -1f, -t).normalized * radius);
        vertices.Add(new Vector3( 0f,  1f, -t).normalized * radius);

        vertices.Add(new Vector3( t,  0f, -1f).normalized * radius);
        vertices.Add(new Vector3( t,  0f,  1f).normalized * radius);
        vertices.Add(new Vector3(-t,  0f, -1f).normalized * radius);
        vertices.Add(new Vector3(-t,  0f,  1f).normalized * radius);

        // Add triangles
        triangles.AddRange(new List<int>{
            0, 11, 5,  0, 5, 1,  0, 1, 7,  0, 7, 10,  0, 10, 11,
            1, 5, 9,  5, 11, 4,  11, 10, 2,  10, 7, 6,  7, 1, 8,
            3, 9, 4,  3, 4, 2,  3, 2, 6,  3, 6, 8,  3, 8, 9,
            4, 9, 5,  2, 4, 11,  6, 2, 10,  8, 6, 7,  9, 8, 1
        });

        // Subdivide triangles
        for (int i = 0; i < subdivisions; i++)
        {
            List<int> newTriangles = new List<int>();
            Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
            for (int j = 0; j < triangles.Count; j += 3)
            {
                int a = triangles[j];
                int b = triangles[j + 1];
                int c = triangles[j + 2];

                int ab = GetMiddlePoint(a, b, vertices, middlePointIndexCache, radius);
                int bc = GetMiddlePoint(b, c, vertices, middlePointIndexCache, radius);
                int ca = GetMiddlePoint(c, a, vertices, middlePointIndexCache, radius);

                newTriangles.AddRange(new int[] { a, ab, ca });
                newTriangles.AddRange(new int[] { b, bc, ab });
                newTriangles.AddRange(new int[] { c, ca, bc });
                newTriangles.AddRange(new int[] { ab, bc, ca });
            }
            triangles = newTriangles;
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    int GetMiddlePoint(int p1, int p2, List<Vector3> vertices, Dictionary<long, int> cache, float radius)
    {
        long smallerIndex = p1 < p2 ? p1 : p2;
        long greaterIndex = p1 > p2 ? p1 : p2;
        long key = (smallerIndex << 32) + greaterIndex;

        if (cache.TryGetValue(key, out int ret))
        {
            return ret;
        }

        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = Vector3.Lerp(point1, point2, 0.5f).normalized * radius;

        int i = vertices.Count;
        vertices.Add(middle);
        cache.Add(key, i);

        return i;
    }
}
