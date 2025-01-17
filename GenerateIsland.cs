using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateIsland : MonoBehaviour
{   
    // Properties of the terrain
    public int width = 15;
    public int depth = 10;
    public float height= 2f; 
    public float scale = 5f; 

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector3[] holes; 
     private Vector3[] normals; 
    private Vector3 normal; 
    private Vector2[] uv; 

    void Start()
    {
        GenerateTerrainMesh();
       //CreateHoles();
       // CreatePiles();
        CreateSlope(); 
        UpdateMesh();
    }

    // Generate Mesh 
    void GenerateTerrainMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        
    }
    
    //Triangle method 
     Vector3 CalculateNormal(Vector3 v0, Vector3 v1, Vector3 v2) {

        Vector3 normal = new Vector3();
        Vector3 edge1 = v1-v0;
        Vector3 edge2 = v2-v1; 

        normal = Vector3.Cross(edge1,edge2); 
        return normal; 

    }

//Create the basic shape 
    void CreateShape()
{
    vertices = new Vector3[(width + 1) * (depth + 1)];
    normals = new Vector3[vertices.Length];
    uv = new Vector2[vertices.Length]; 
    float rand = Random.Range(0.1f,1.5f);

   
    for (int z = 0; z <= depth; z++)
    {
        for (int x = 0; x <= width; x++)
        {
            int index = z * (width + 1) + x;

            // Use Perlin noise to determine height
            float y = Mathf.PerlinNoise(x * scale / width *0.5f, z * scale / depth*0.5f) * height*rand;
            vertices[index] = new Vector3(x, y, z);
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

   
    for (int i = 0; i < triangles.Length; i += 3) {

        int v0 = triangles[i];
        int v1 = triangles[i + 1];
        int v2 = triangles[i + 2];

         // Calculate normals using the triangle method
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

     void CreateHoles() {
    int numberOfHoles = 30; 
    float holeDepth = 0.1f; 

    for (int i = 0; i < numberOfHoles; i++)
    {

        int holeX = Random.Range(0, width + 1); 
        int holeZ = Random.Range(0, depth + 1); 
        int vertexIndex = holeZ * (width + 1) + holeX;
        vertices[vertexIndex].y -= holeDepth;
    }
}

 // Create piles
  void CreatePiles() {
    int numberOfPiles = 60; 
    float pileRadius = 6f; 
    float pileHeight = 0.8f; 

    for (int i = 0; i < numberOfPiles; i++)
    {
        
        int pileX = Random.Range(0, width -1); 
        int pileZ = Random.Range(0, depth -1); 
        int centerVertexIndex = pileZ * (width + 1) + pileX;
        vertices[centerVertexIndex].y += pileHeight * 0.75f; 

        // Modify vertices around the center for a rounded shape
        for (int angle = 0; angle < 360; angle += 5) 
        {
            float rad = angle * Mathf.Deg2Rad;
            int offsetX = Mathf.RoundToInt(Mathf.Cos(rad) * pileRadius);
            int offsetZ = Mathf.RoundToInt(Mathf.Sin(rad) * pileRadius);

            int nearbyX = pileX + offsetX;
            int nearbyZ = pileZ + offsetZ;

            // Ensure the indices are within bounds
            if (nearbyX >= 0 && nearbyX <= width && nearbyZ >= 0 && nearbyZ <= depth)
            {
                int nearbyVertexIndex = nearbyZ * (width + 1) + nearbyX;
                float distance = Mathf.Sqrt(offsetX * offsetX + offsetZ * offsetZ);
                float falloff = Mathf.Pow(1 - (distance / pileRadius), 2);
                vertices[nearbyVertexIndex].y += pileHeight * falloff;
            }
        }
    }
}

// Create slope
void CreateSlope()
{

    float maxDistance = Mathf.Min(width / 2f, depth / 2f);

    for (int z = 0; z <= depth; z++)
    {
        for (int x = 0; x <= width; x++)
        {
            int index = z * (width + 1) + x;

            // Calculate the distance to the closest edge
            float distToLeft = x;                  
            float distToRight = width - x;         
            float distToTop = z;                  
            float distToBottom = depth - z;  

            float minDistToEdge = Mathf.Min(distToLeft, distToRight, distToTop, distToBottom);
            float normalizedDistance = minDistToEdge / maxDistance;
            float falloff = Mathf.SmoothStep(0.0f, 1.0f, normalizedDistance)*3.0f;
            vertices[index].y *= falloff;

            // Gradually transition vertices near edges to underwater
            if (normalizedDistance < 0.2f) 
            {
                vertices[index].y = Mathf.Lerp(vertices[index].y, -1.0f, 1.0f - normalizedDistance / 0.2f);
            }
        }
    }
}


    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals; 
        mesh.uv = uv; 
        //mesh.RecalculateNormals(); // Smooth shading
        mesh.RecalculateBounds(); // Ensure correct mesh bounds
    }
 }


