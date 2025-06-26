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

    public string winCon;
    public string loseCon;

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
            gameWon = checkIfConditionMet(winCon);
            gameLost = checkIfConditionMet(loseCon);

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

        Debug.Log("The game is now over. Player " + winner + " wins!");
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

    bool checkIfConditionMet(string condition)
    {
        List<string> coordinates;
        if (currentPlayer == 1)
        {
            coordinates = new List<string>(p1Coords);
        }
        else
        {
            coordinates = new List<string>(p2Coords);
        }

        if (coordinates.Count < 3)
        {
            return false;
        }

        bool[,] board = new bool[8, 8];
        foreach (string coord in coordinates)
        {
            board[coord[0] - 'A', coord[1] - '1'] = true;
        }

        switch (condition)
        {
            case "3InARow":
                if (checkHorizontal(board, 3))
                    {  return true; }
                if (checkVertical(board, 3))
                    { return true; }
                if (checkDiagonal(board, 3))
                    { return true; }
                return false;

            case "3Horizontal":
                return checkHorizontal(board, 3);
            case "3Vertical":
                return checkVertical(board, 3);
            case "3Diagonal":
                return checkDiagonal(board, 3);

            default:
                return false;
        }
    }

    bool checkVertical(bool[,] board, int x)
    {
        for (int row = 0; row < 8; row++)
        {
            int count = 0;
            for (int col = 0; col < 8; col++)
            {
                if (board[row, col])
                {
                    count++;
                    if (count >= x)
                        return true;
                }
                else
                {
                    count = 0;
                }
            }
        }
        return false;
    }

    bool checkHorizontal(bool[,] board, int x)
    {
        for (int col = 0; col < 8; col++)
        {
            int count = 0;
            for (int row = 0; row < 8; row++)
            {
                if (board[row, col])
                {
                    count++;
                    if (count >= x)
                        return true;
                }
                else
                {
                    count = 0;
                }
            }
        }
        return false;
    }

    bool checkDiagonal(bool[,] board, int x)
    {
        for (int row = 0; row <= 8 - x; row++)
        {
            // Top-Left to Bottom-Right \
            for (int col = 0; col <= 8 - x; col++)
            {
                int count = 0;
                for (int i = 0; i < x; i++)
                {
                    if (board[row + i, col + i])
                        count++;
                    else
                        break;
                }

                if (count >= x)
                    return true;
            }

            // Bottom-Left to Top-Right /
            for (int col = x - 1; col < 8; col++)
            {
                int count = 0;
                for (int i = 0; i < x; i++)
                {
                    if (board[row + i, col - i])
                        count++;
                    else
                        break;
                }

                if (count == x)
                    return true;
            }
        }

        return false;
    }
}
