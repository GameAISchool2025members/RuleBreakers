using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    public bool gameWon = false;
    public bool gameLost = false;
    public bool gameOngoing = true;

    public int currentPlayer;

    public List<GameObject> p1Pieces = new List<GameObject>();
    public List<GameObject> p2Pieces = new List<GameObject>();
    public List<string> p1Coords = new List<string>();
    public List<string> p2Coords = new List<string>();

    public int tileCount = 0;

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
        currentPlayer = Random.Range(1, 3); // Choose between 1 and 2
        Debug.Log("Player " + currentPlayer + " goes first.");

        p1Pieces.Clear();
        p2Pieces.Clear();

        p1Coords.Clear();
        p2Coords.Clear();

        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void checkIfGameOver()
    {
        if (p1Pieces.Count + p2Pieces.Count >= tileCount)
        {
            // Tie! No more pieces can be placed.
            gameOngoing = false;
        }

        if (gameOngoing)
        {
            if (gameWon || gameLost)
            {
                // A Win/Lose condition has been met.
                gameOngoing = false;
            }
        }

        if (!gameOngoing)
        {
            gameOver();
        }
    }

    void gameOver()
    {
        if (!gameWon && !gameLost)
        {
            Debug.Log("Tie! No more pieces can be placed!");
            return;
        }

        int winner = 0;
        int loser = 0;

        if (gameWon)
        {
            winner = currentPlayer;
            loser = 3 - currentPlayer;
        }
        else if (gameLost)
        {
            loser = currentPlayer;
            winner = 3 - currentPlayer;
        }

        Debug.Log("The game is now over. Player " + winner + "wins!");
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

    public void addP1Coords(string coord)
    {
        p1Coords.Add(coord);
    }

    public void addP2Coords(string coord)
    {
        p2Coords.Add(coord);
    }

    public void countTile()
    {
        tileCount++;
    }
}
