using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridScript : MonoBehaviour
{
    //3d objects
    public Transform CellPrefab;
    public Transform TreePrefab;
    public Transform Bush;
    public Transform[] Fruits;


    //Grid info
    public UnityEngine.Vector3 Size;
    public Transform[,] Grid;

    //Algorithm
    public List<Transform> Set;
    public List<List<Transform>> AdjSet;
    public float FruitSpawnChance = 0.2f;  // 20% chance


    void Start() {
        Debug.Log("GridScript Start executed");
        Debug.Log("Terrain generation complete. Proceeding to create grid...");
        CreateGrid();
        Debug.Log("Grid creation invoked.");
        
        SetRandomNumbers();
        SetAdjacents();
        SetStart(); 
        FindNext();
    }
    void CreateGrid() {
        if (CellPrefab == null) {
            Debug.LogError("CellPrefab is not assigned!");
            return;
        }

        float cellSize = 2.0f;
        float xOffset = Size.x / 2f; // Center offset for x
        float zOffset = Size.z / 2f; // Center offset for z


        Grid = new Transform[(int)Size.x, (int)Size.z];
        
        for (int x = 0; x < Size.x; x++) {
            for (int z = 0; z < Size.z; z++) {

                float x_displace = Size.x/2;
                float z_displace = Size.z/2;

                Transform newCell = Instantiate(CellPrefab, new UnityEngine.Vector3(x-x_displace, 1, z-z_displace), UnityEngine.Quaternion.identity);
                newCell.localScale = new UnityEngine.Vector3(cellSize, cellSize, cellSize);

                newCell.name = $"Cell ({x}, {z})";
                newCell.parent = transform;
                    

                var cellScript = newCell.GetComponent<CellScript>();
                if (cellScript == null) {
                    Debug.LogError($"Cell at ({x}, {z}) is missing CellScript!");
                    continue;
                }

                cellScript.Position = new UnityEngine.Vector3(x, 0, z);
                Grid[x, z] = newCell;
            }
        }
    }

    void PlaceFruit(UnityEngine.Vector3 position)
    {
        if (Random.Range(0f, 1f) <= FruitSpawnChance)
        {
            if (Fruits.Length > 0)
            {
                //Random fruit
                Transform fruitPrefab = Fruits[Random.Range(0, Fruits.Length)];

                //Offest placement
                UnityEngine.Vector3 fruitPosition = position + new UnityEngine.Vector3(0, 0.5f, 0);

                //Correct rotation
                UnityEngine.Quaternion uprightRotation = UnityEngine.Quaternion.Euler(-80, 0, 0); // No rotation, straight up
                Instantiate(fruitPrefab, fruitPosition, uprightRotation, transform);
            }
            else
            {
                Debug.LogWarning("No fruit prefabs assigned!");
            }
        }
    }

    void SetRandomNumbers() {
        foreach (Transform child in transform) {
            if (child == null) continue;

            int weight = Mathf.Clamp(Random.Range(0, 10), 0, 9);
            var cellScript = child.GetComponent<CellScript>();
            if (cellScript != null) {
                cellScript.weight = weight;

                var textMesh = child.GetComponentInChildren<TextMeshPro>();
                if (textMesh != null) {
                    textMesh.text = weight.ToString();
                } else {
                    Debug.LogWarning($"No TextMeshPro found in {child.name}");
                }
            }
        }
    }

    void SetAdjacents() {
        for (int x = 0; x < Size.x; x++) {
            for (int z = 0; z < Size.z; z++) {
                Transform cell = Grid[x, z];
                if (cell == null) {
                    Debug.LogError($"Cell at ({x},{z}) is null!");
                    continue;
                }

                CellScript cScript = cell.GetComponent<CellScript>();
                if (cScript == null) {
                    Debug.LogError($"Cell at ({x},{z}) is missing CellScript!");
                    continue;
                }

                // Assign adjacents
                if (x - 1 >= 0) cScript.Adjacents.Add(Grid[x - 1, z]);
                if (x + 1 < Size.x) cScript.Adjacents.Add(Grid[x + 1, z]);
                if (z - 1 >= 0) cScript.Adjacents.Add(Grid[x, z - 1]);
                if (z + 1 < Size.z) cScript.Adjacents.Add(Grid[x, z + 1]);

                cScript.Adjacents.Sort(SortByLowestWeight);
            }
        }
    }


    int SortByLowestWeight(Transform inputA, Transform inputB){
        int a = inputA.GetComponent<CellScript>().weight; //a's weight
        int b = inputB.GetComponent<CellScript>().weight; //b's weight
        return a.CompareTo(b);
    }

    void SetStart() {
        Set = new List<Transform>();
        AdjSet = new List<List<Transform>>();
        for (int i = 0; i < 10; i++) {
            AdjSet.Add(new List<Transform>());
        }

        Transform startCell = Grid[0, 0];
        Renderer cellRenderer = startCell.GetComponentInChildren<Renderer>();
        if (cellRenderer == null) {
            Debug.LogWarning($"Cell at {startCell.name} is missing a Renderer on its child!");
            return;
        }

        // Adding start cell to the set
        AddToSet(startCell);
        Debug.Log("Starting maze generation...");

        // Begin the algorithm
        Invoke("FindNext", 0);
    }

    void AddToSet(Transform toAdd){
        Set.Add(toAdd);

        foreach(Transform adj in toAdd.GetComponent<CellScript>().Adjacents){
            adj.GetComponent<CellScript>().AdjacentsOpened++;

            if(!Set.Contains(adj) && !AdjSet[adj.GetComponent<CellScript>().weight].Contains(adj)){
                AdjSet[adj.GetComponent<CellScript>().weight].Add(adj);
            }
        }
    }

    void FindNext() {
    Transform next;

    do {
        bool empty = true;
        int lowestList = 0;

        // Find the lowest weighted list with cells
        for (int i = 0; i < 10; i++) {
            lowestList = i;
            if (AdjSet[i].Count > 0) {
                empty = false;
                break;
            }
        }

        if (empty) {
            Debug.Log("Maze generation complete!");
            CancelInvoke("FindNext");

            // Mark remaining cells as walls
            foreach (Transform cell in Grid) {
                if (!Set.Contains(cell)) {
                    // Remove the original cell
                    Destroy(cell.gameObject);

                    // Random generation of palm tree
                    if (Random.Range(0f, 1f) <= 0.08f) {
                        if (TreePrefab != null) {
                            Transform newTree = Instantiate(TreePrefab, cell.position, UnityEngine.Quaternion.identity, transform);
                            newTree.name = "Tree at " + cell.position;
                            newTree.localScale = new UnityEngine.Vector3(1f, 1f, 1f);
                        } else {
                            Debug.LogWarning("Tree prefab is not assigned!");
                        }
                    }

                    //Generate bush
                    if (Bush != null) {
                        Transform newBush = Instantiate(Bush, cell.position, UnityEngine.Quaternion.identity, transform);
                        RandomizeBush(newBush);
                    } else {
                        Debug.LogWarning("Bush prefab is not assigned!");
                    }

                    // Check for fruit placement
                    PlaceFruit(cell.position);

                    // Remove original cell object
                    Destroy(cell.gameObject);

                } else {
                    // Remove the path cells
                    Destroy(cell.gameObject);
                }
            }
            return;
        }

        next = AdjSet[lowestList][0];
        AdjSet[lowestList].Remove(next);

        Debug.Log($"Processing cell: {next.name} with weight: {lowestList}");

    } while (next.GetComponent<CellScript>().AdjacentsOpened >= 2);

    // Mark next cell as part of the maze
    next.GetComponentInChildren<TextMeshPro>().GetComponent<Renderer>().enabled = false;
    next.GetComponentInChildren<Renderer>().enabled = false;
    AddToSet(next);

    Debug.Log($"Added cell {next.name} to the set. Total cells in set: {Set.Count}");

    Invoke("FindNext", 0);
}


    void RandomizeBush(Transform bush){
        if (bush == null)
        {
            Debug.LogWarning("Bush is null, cannot randomize.");
            return;
        }

        // Randomize rotation
        float randomYRotation = Random.Range(0f, 360f);
        bush.rotation = UnityEngine.Quaternion.Euler(0, randomYRotation, 0);

        // Randomize scale
        float randomScale = Random.Range(0.8f, 1.2f);
        bush.localScale = new UnityEngine.Vector3(randomScale, randomScale, randomScale);

    }


    void Update() {

    }

    
}
