using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static JsonConverter;

public static class ListExtenstions
{
    public static void AddMany<T>(this List<T> list, params T[] elements)
    {
        list.AddRange(elements);
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    public RulesManager rulesManager;

    public bool gameWon = false;
    public bool gameLost = false;
    public bool gameOngoing = true;

    public GameObject allTiles;
    public GroqCloudClient groqClient;
    public TextManager textManager;

    public int currentPlayer;
    public TMP_Text currentPlayerText;
    public TMP_Text winConText;
    public TMP_Text loseConText;

    public List<GameObject> p1Pieces = new List<GameObject>();
    public List<GameObject> p2Pieces = new List<GameObject>();
    public List<string> p1Coords = new List<string>();
    public List<string> p2Coords = new List<string>();

    public List<GameObject> tiles = new List<GameObject>();

    public string winCon;
    public string loseCon;
    public string currentJokerRule = ""; // Store the current joker rule from TextManager
    public List<string> conditions = new List<string>();
    public bool randomConditions = false;
    public bool allow3Wins = true;
    public bool allow4Wins = true;
    public bool allow5Wins = true;

    void Awake()
    {
        if (gameManager == null) // If there is no instance already
        {
            // DontDestroyOnLoad(gameObject); // Keep the GameObject, this component is attached to, across different scenes
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
        if (rulesManager == null)
        {
            rulesManager = GameObject.Find("RulesManager").GetComponent<RulesManager>();
        }

        currentPlayer = Random.Range(1, 3); // Choose between 1 and 2
        Debug.Log("Player " + currentPlayer + " goes first.");
        currentPlayerText.text = "Player " + currentPlayer;

        if (currentPlayer == 1)
        {
            currentPlayerText.color = Color.cyan;
        }

        else
        {
            currentPlayerText.color = Color.red;
        }

        p1Pieces.Clear();
        p2Pieces.Clear();

        p1Coords.Clear();
        p2Coords.Clear();

        conditions.Clear();
        tiles.Clear();

        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }

        allTiles = GameObject.Find("Tiles");

        // Get all the tiles in the game
        foreach (Transform row in allTiles.transform)
        {
            {
                foreach (Transform tile in row)
                {
                    if (tile != null)
                    {
                        tiles.Add(tile.gameObject);
                    }
                }
            }
        }

        allow3Wins = rulesManager.Get3Wins();
        allow4Wins = rulesManager.Get4Wins();
        allow5Wins = rulesManager.Get5Wins();
        randomConditions = rulesManager.GetRandomConditions();

        // Debug.Log(allow3Wins + ", " + allow4Wins + ", " + allow5Wins + ", " + randomConditions);

        if (!randomConditions)
        {
            winCon = rulesManager.GetWinCon();
            loseCon = rulesManager.GetLoseCon();
        }

        else
        {

            // 21 Conditions currently implemented
            // By default, if all 3 permissions are disabled, we go for 3 piece wins only.

            if (!allow3Wins && !allow4Wins && !allow5Wins)
            {
                allow3Wins = true;
            }

            if (allow3Wins)
            {
                conditions.AddMany("3InARow", "3Horizontal", "3Vertical", "3Diagonal", "3L");
            }
            if (allow4Wins)
            {
                conditions.AddMany("4InARow", "4Horizontal", "4Vertical", "4Diagonal",
                                    "4L", "4J", "4LJ", "4T", "Square", "Diamond");
            }
            if (allow5Wins)
            {
                conditions.AddMany("5InARow", "5Horizontal", "5Vertical", "5Diagonal", "Plus", "Cross");
            }

            if (randomConditions)
            {
                int winInt = getNewCondition();
                winCon = conditions[winInt];

                int loseInt = -1;

                while (loseInt < 0)
                {
                    loseInt = getNewCondition();
                    loseCon = conditions[loseInt];

                    // We make sure that we do not have any conflicts/unwanted wincons
                    if (conditionConflict())
                    {
                        loseInt = -1;
                    }
                }
            }
        }

        winConText.text = winCon;
        loseConText.text = loseCon;
    }

    int getNewCondition()
    {
        return Random.Range(0, conditions.Count);
    }

    bool conditionConflict()
    {
        if (winCon == loseCon)
        {
            return true;
        }

        return false;
    }

    public void checkIfGameOver()
    {
        if (p1Pieces.Count + p2Pieces.Count >= tiles.Count)
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

        else
        {
            StartCoroutine(swapPlayer());
        }
    }

    void gameOver()
    {
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<Tile>().setTileLegal(false, 1);
            tile.GetComponent<Tile>().setTileLegal(false, 2);
        }

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

        StartCoroutine(LoadMainMenu()); 
    }

    public IEnumerator swapPlayer()
    {
        if (currentPlayer == 1)
        {
            currentPlayer = 2;
            currentPlayerText.color = Color.red;
        }
        else
        {
            currentPlayer = 1;
            currentPlayerText.color = Color.cyan;
        }

        string prompt = textManager.CreatetileValidatorPrompt(currentJokerRule);

        string tileValidationResponse = "";
        bool success = false;
        string llmResponse = "";
        bool responseReceived = false;

        groqClient.SendMessage(prompt, (response, isSuccess) =>
        {
            tileValidationResponse = response;
            success = isSuccess;
            responseReceived = true;
        });
        
        //Wait for response
        while (!responseReceived)
        {
            yield return null;
        }

        Dictionary<string, JsonConverter.PlacementEntry> validationResponse = JsonConverter.ParseValidPlacementJSONToDictionary(tileValidationResponse, out bool valid);
        Debug.Log("tileValidationResponse: " + validationResponse);
        
        currentPlayerText.text = "Player " + currentPlayer;
        foreach (GameObject tile in tiles)
        {
            tile.GetComponent<Tile>().changeColour();
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

    public void addTile(GameObject tile)
    {
        tiles.Add(tile);
    }

    public void setTileLegalFromDictionary(Dictionary<string, PlacementEntry> dict)
    {
        foreach (GameObject g in tiles)
        {
            Tile t = g.GetComponent<Tile>();
            PlacementEntry p = dict[t.coords];
            t.isLegalForP1 = p.P1;
            t.isLegalForP2 = p.P2;
        }
    }

    public void setTileLegal(GameObject tile, bool isLegal)
    {
        tile.GetComponent<Tile>().setTileLegal(isLegal, currentPlayer);
    }

    public void SetCurrentJokerRule(string rule)
    {
        currentJokerRule = rule;
        Debug.Log($"Joker rule received from TextManager: {rule}");
        // You can add additional logic here to process the rule
        // For example, validate tiles based on the new rule
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

        int x = 0;

        switch (condition)
        {
            case "3InARow":
                x = 3;
                if (checkHorizontal(board, x))
                { return true; }
                if (checkVertical(board, x))
                { return true; }
                return (checkDiagonal(board, x));

            case "3Horizontal":
                x = 3;
                return checkHorizontal(board, x);
            case "3Vertical":
                x = 3;
                return checkVertical(board, x);
            case "3Diagonal":
                x = 3;
                return checkDiagonal(board, x);

            case "3L":
                return check3L(board);

            // 4 In A Row

            case "4InARow":
                x = 4;
                if (checkHorizontal(board, x))
                { return true; }
                if (checkVertical(board, x))
                { return true; }
                return (checkDiagonal(board, x));

            case "4Horizontal":
                x = 4;
                return checkHorizontal(board, x);
            case "4Vertical":
                x = 4;
                return checkVertical(board, x);
            case "4Diagonal":
                x = 4;
                return checkDiagonal(board, x);

            case "4L":
                return check4L(board);
            case "4J":
                return check4J(board);
            case "4LJ":
                if (check4L(board))
                    { return true; }
                return (check4J(board));

            case "4T":
                return check4T(board);
            case "Square":
                return checkSquare(board);
            case "Diamond":
                return checkDiamond(board);

            // 5 In A Row

            case "5InARow":
                x = 5;
                if (checkHorizontal(board, x))
                { return true; }
                if (checkVertical(board, x))
                { return true; }
                return (checkDiagonal(board, x));

            case "5Horizontal":
                x = 5;
                return checkHorizontal(board, x);
            case "5Vertical":
                x = 5;
                return checkVertical(board, x);
            case "5Diagonal":
                x = 5;
                return checkDiagonal(board, x);

            case "Plus":
                return checkPlus(board);
            case "Cross":
                return checkCross(board);

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

    bool check3L(bool[,] board)
    {
        // Possible L-Block/J-Block patterns (relative to top-left)
        int[][][] lBlocks = new int[][][]
        {
            new[] { new[] {0,0}, new[] {1,0}, new[] {1,1}},
            new[] { new[] {0,0}, new[] {1,0}, new[] {1,-1}},

            new[] { new[] {0,0}, new[] {0,1}, new[] {1,1}},
            new[] { new[] {0,0}, new[] {0,1}, new[] {-1,1}},

            new[] { new[] {0,0}, new[] {-1,0}, new[] {-1,1}},
            new[] { new[] {0,0}, new[] {-1,0}, new[] {-1,-1}},

            new[] { new[] {0,0}, new[] {0,-1}, new[] {-1,1}},
            new[] { new[] {0,0}, new[] {0,-1}, new[] {-1,-1}},
        };

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                foreach (var l in lBlocks)
                {
                    bool match = true;

                    foreach (var offset in l)
                    {
                        int r = row + offset[0];
                        int c = col + offset[1];

                        if (r < 0 || r >= 8 || c < 0 || c >= 8 || !board[r, c])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                        return true;
                }
            }
        }
        return false;
    }

    bool check4L(bool[,] board)
    {
        // Possible L-Block patterns (relative to top-left)
        int[][][] lBlocks = new int[][][]
        {
            new[] { new[] {0,0}, new[] {1,0}, new[] {2,0}, new[] {2,1}},
            new[] { new[] {0,0}, new[] {0,1}, new[] {0,2}, new[] {1,0}},
            new[] { new[] {0,0}, new[] {0,1}, new[] {1,1}, new[] {2,1}},
            new[] { new[] {0,2}, new[] {1,0}, new[] {1,1}, new[] {1,2}},
        };

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                foreach (var l in lBlocks)
                {
                    bool match = true;

                    foreach (var offset in l)
                    {
                        int r = row + offset[0];
                        int c = col + offset[1];

                        if (r < 0 || r >= 8 || c < 0 || c >= 8 || !board[r, c])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                        return true;
                }
            }
        }
        return false;
    }

    bool check4J(bool[,] board)
    {
        // Possible J-Block patterns (relative to top-left)
        int[][][] jBlocks = new int[][][]
        {
            new[] { new[] {0,1}, new[] {1,1}, new[] {2,1}, new[] {2,0}},
            new[] { new[] {0,0}, new[] {1,0}, new[] {1,1}, new[] {1,2}},
            new[] { new[] {0,0}, new[] {0,1}, new[] {1,0}, new[] {2,0}},
            new[] { new[] {0,0}, new[] {0,1}, new[] {0,2}, new[] {1,2}},
        };

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                foreach (var j in jBlocks)
                {
                    bool match = true;

                    foreach (var offset in j)
                    {
                        int r = row + offset[0];
                        int c = col + offset[1];

                        if (r < 0 || r >= 8 || c < 0 || c >= 8 || !board[r, c])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                        return true;
                }
            }
        }
        return false;
    }

    bool check4T(bool[,] board)
    {
        // Possible T-Block patterns
        int[][][] tBlocks = new int[][][]
        {
            // Up
            new[] { new[] {0,0}, new[] {0,-1}, new[] {0,1}, new[] {1,0} },
            // Down
            new[] { new[] {0,0}, new[] {0,-1}, new[] {0,1}, new[] {-1,0} },
            // Left
            new[] { new[] {0,0}, new[] {-1,0}, new[] {1,0}, new[] {0,1} },
            // Right
            new[] { new[] {0,0}, new[] {-1,0}, new[] {1,0}, new[] {0,-1} },
        };

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                foreach (var t in tBlocks)
                {
                    bool match = true;
                    foreach (var offset in t)
                    {
                        int r = row + offset[0];
                        int c = col + offset[1];

                        if (r < 0 || r >= 8 || c < 0 || c >= 8 || !board[r, c])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                        return true;
                }
            }
        }

        return false;
    }

    bool checkSquare(bool[,] board)
    {
        for (int row = 0; row < 7; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if (board[row, col] &&
                    board[row, col + 1] &&
                    board[row + 1, col] &&
                    board[row + 1, col + 1])
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool checkDiamond(bool[,] board)
    {
        for (int row = 1; row < 7; row++)
        {
            for (int col = 1; col < 7; col++)
            {
                if (board[row - 1, col] &&
                    board[row + 1, col] &&
                    board[row, col - 1] &&
                    board[row, col + 1])
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool checkPlus(bool[,] board)
    {
        for (int row = 1; row < 7; row++)
        {
            for (int col = 1; col < 7; col++)
            {
                if (board[row, col] &&        
                    board[row - 1, col] &&    
                    board[row + 1, col] &&    
                    board[row, col - 1] &&    
                    board[row, col + 1])      
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool checkCross(bool[,] board)
    {
        for (int row = 1; row < 7; row++)
        {
            for (int col = 1; col < 7; col++)
            {
                if (board[row, col] &&
                    board[row - 1, col - 1] &&
                    board[row - 1, col + 1] &&
                    board[row + 1, col - 1] &&
                    board[row + 1, col + 1])
                {
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator LoadMainMenu()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("MainMenu");
    }

}
