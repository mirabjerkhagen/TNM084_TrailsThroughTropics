using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateWater : MonoBehaviour
{   
    // Properties of the water plane
    public int width = 50; 
    public int depth = 50; 
    public float waveSpeed = 0.5f; 
    public float waveHeight = 1.5f; 
    public float waveFrequency = 10.0f; 

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv; 
    private Vector3[] normals; 
    private Vector3 normal; 


    void Start()
    {
        GenerateWaterMesh();
        UpdateMesh();
    }

    void Update()
    {
       AnimateWaves();
        UpdateMesh();
    }

    // Generate the water mesh
    void GenerateWaterMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
    }

    //Triangle Method
    Vector3 CalculateNormal(Vector3 v0, Vector3 v1, Vector3 v2) {

        Vector3 normal = new Vector3();

        Vector3 edge1 = v1-v0;
        Vector3 edge2 = v2-v1; 

        normal = Vector3.Cross(edge1,edge2); 
        return normal; 

    }
    

//Create shape of the water plane
  void CreateShape()
{
    vertices = new Vector3[(width + 1) * (depth + 1)];
    uv = new Vector2[vertices.Length]; 
    normals = new Vector3[vertices.Length];



    for (int z = 0; z <= depth; z++)
    {
        for (int x = 0; x <= width; x++)
        {
            int index = z * (width + 1) + x;
            vertices[index] = new Vector3(x, 0, z);
            uv[index] = new Vector2((float)x / width, (float)z / depth); 
        }
    }

    // Create triangles
    triangles = new int[width * depth * 6];
    int vert = 0;
    int tris = 0;

    for (int z = 0; z < depth; z++)
    {
        for (int x = 0; x < width; x++)
        {
            // Triangle 1
            triangles[tris + 0] = vert + 0;
            triangles[tris + 1] = vert + width + 1;
            triangles[tris + 2] = vert + 1;

            // Triangle 2
            triangles[tris + 3] = vert + 1;
            triangles[tris + 4] = vert + width + 1;
            triangles[tris + 5] = vert + width + 2;

            vert++;
            tris += 6;
        }
        vert++;
    }

    // Calculate normals using the triangle method
        for (int i = 0; i < triangles.Length; i += 3)
        {
            
            int v0 = triangles[i];
            int v1 = triangles[i + 1];
            int v2 = triangles[i + 2];

            Vector3 normal = CalculateNormal(vertices[v0], vertices[v1], vertices[v2]);

          
            normals[v0] += normal;
            normals[v1] += normal;
            normals[v2] += normal;
        }

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = normals[i].normalized;
        }

}


    // Animate the waves
    void AnimateWaves()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            // Use Perlin noise for smooth wave animation
            Vector3 vertex = vertices[i];
            vertex.y = Mathf.PerlinNoise(Time.time * waveSpeed ,vertex.x * waveFrequency*0.2f) * waveHeight;
            vertex.y += Mathf.PerlinNoise(Time.time * waveSpeed, vertex.z * waveFrequency*0.2f) * waveHeight;
            vertices[i] = vertex;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = normals; 
      //  mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
