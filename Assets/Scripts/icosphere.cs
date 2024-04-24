using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class icosphere : MonoBehaviour
{
    [Range(0, 8)]
    [SerializeField]
    private int _subdivisions = 3;
    public float radius = 3.5f;
    // public List<GameObject> icotris = new List<GameObject>();
    public Dictionary<int, List<GameObject>> icotrisMipMap = new Dictionary<int, List<GameObject>>();

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    public GameObject icotriPrefab;
    private bool initiated = false;
    public int subdivisions {
        get => _subdivisions;
        set {
            if (_subdivisions != value) { // Check if the value is actually changing
                _subdivisions = Mathf.Clamp(value, 0, 8); // Ensure subdivisions are within a valid range
                UpdateIcosphere(); // Call the update method if there's a change
            }
        }
    }
    private void Start()
    {
        UpdateIcosphere();
    }

    void UpdateIcosphere()
    {
        print("Subdivisions: " + subdivisions);
        var keys = icotrisMipMap.Keys;
        foreach (int key in keys){
            if (key != subdivisions){
                foreach (GameObject icotriObj in icotrisMipMap[key]){
                    icotriObj.SetActive(false);
                }
            }
        }
        // cache exists
        if (keys.Contains(subdivisions)){
            foreach (GameObject icotriObj in icotrisMipMap[subdivisions]){
                icotriObj.SetActive(true);
            }
            
        }else{
            StartCoroutine(CreateIcosphere(radius, subdivisions));
        }
    }

    IEnumerator CreateIcosphere(float radius, int subdivisions)
    {
        if (!initiated){
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
            // subdivide current triangles for twice once
            for(int i = 0; i < 2; i++){
                List<int> newTriangles = new List<int>();
                Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
                for (int j = 0; j < triangles.Count; j += 3)
                {
                    yield return new WaitForSeconds(0.0001f * (j / 3) );
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
            // set flag to true
            initiated = true;
        }



        // create separate game object for each triangle
        for (int i = 0; i < triangles.Count; i += 3)
        {
            yield return new WaitForSeconds(0.0001f * i);
            // then create icotri object for each triangle
            GameObject icotriObj = Instantiate(icotriPrefab, Vector3.zero, Quaternion.identity);
            icotriObj.transform.parent = transform;
            icotri icotriScript = icotriObj.GetComponent<icotri>();
            icotriScript.Radius = radius;
            icotriScript.Subdivisions = subdivisions;
            icotriScript.Vertices = new Vector3[]{
                vertices[triangles[i]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]
            };
            icotriScript.MeshName = "Icotri" + i;
            if (icotrisMipMap.ContainsKey(subdivisions)){
                icotrisMipMap[subdivisions].Add(icotriObj);
            } else {
                icotrisMipMap.Add(subdivisions, new List<GameObject>{icotriObj});
            }
        }
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
