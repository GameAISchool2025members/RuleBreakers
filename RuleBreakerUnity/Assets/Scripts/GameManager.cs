using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    public List<GameObject> p1Pieces = new List<GameObject>();
    public List<GameObject> p2Pieces = new List<GameObject>();

    void Awake()
    {
        if (gameManager == null) // If there is no instance already
        {
            DontDestroyOnLoad(gameObject); // Keep the GameObject, this component is attached to, across different scenes
            gameManager = this;
        }
        else if (gameManager != this) // If there is already an instance and it's not `this` instance
        {
            Destroy(gameObject); // Destroy the GameObject, this component is attached to
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        p1Pieces.Clear();
        p2Pieces.Clear();
    }

    public void addP1Piece(GameObject piece)
    {
        p1Pieces.Add(piece);
    }

    public void addP2Piece(GameObject piece)
    {
        p2Pieces.Add(piece);
    }
}
