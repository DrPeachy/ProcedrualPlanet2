using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class icotri : MonoBehaviour
{
    public Material material;
    private float radius;
    private int subdivisions;
    private string meshName;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    public float Radius
    {
        get { return radius; }
        set { radius = value; }
    }

    public int Subdivisions
    {
        get { return subdivisions; }
        set { subdivisions = value; }
    }

    public Vector3[] Vertices
    {
        get { return vertices.ToArray(); }
        set { vertices = new List<Vector3>(value); }
    }

    public string MeshName
    {
        get { return meshName; }
        set { meshName = value; }
    }

    void Start()
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateIcotri(radius, subdivisions);

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = material;
    }

    public

    Mesh CreateIcotri(float radius, int subdivisions)
    {
        Mesh mesh = new Mesh();
        mesh.name = meshName;

        // Initial triangle setup
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);

        if (subdivisions > 0)
        {
            // Subdivisions
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
        }

        // Assign vertices and triangles to mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        Vector3[] normals = new Vector3[vertices.Count];
        Vector2[] uvs = new Vector2[vertices.Count];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = vertices[i].normalized;  // Normal is the normalized position vector

            Vector3 vertex = vertices[i];
        float u = Mathf.Atan2(vertex.z, vertex.x) / (2 * Mathf.PI) + 0.5f;
        float v = vertex.y * 0.5f + 0.5f;
            uvs[i] = new Vector2(u, v);
        }
        mesh.normals = normals;
        mesh.uv = uvs;

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
