using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    public int currentPlayer;

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

        currentPlayer = Random.Range(1, 3); // Choose between 1 and 2
        Debug.Log("Player " + currentPlayer + " goes first.");
    }

    public void swapPlayer()
    {
        if (currentPlayer == 1)
        {
            currentPlayer = 2;
        }
        else
        {
            currentPlayer = 1;
        }
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
