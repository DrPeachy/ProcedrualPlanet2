using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class icosphere : MonoBehaviour
{
    [Range(0, 8)]
    [SerializeField]
    private int _subdivisions = 3;
    public long radius = 1;
    // public List<GameObject> icotris = new List<GameObject>();
    public Dictionary<int, List<GameObject>> icotrisMipMap = new Dictionary<int, List<GameObject>>();

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    public GameObject icotriPrefab;
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
            CreateIcosphere(radius, subdivisions);
        }
    }

    void CreateIcosphere(float radius, int subdivisions)
    {
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

        // create separate game object for each triangle
        for (int i = 0; i < triangles.Count; i += 3)
        {
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
}
