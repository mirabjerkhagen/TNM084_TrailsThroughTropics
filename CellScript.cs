using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellScript : MonoBehaviour
{
    public List<Transform> Adjacents; 
    public Vector3 Position;
    public int weight;
    public int AdjacentsOpened;

    void Awake()
    {
        Adjacents = new List<Transform>();
        AdjacentsOpened = 0;
    }

    void Start()
    {

    }

    public void ResetCell()
    {
        Adjacents.Clear();
        weight = 0;
        AdjacentsOpened = 0;
    }
}
