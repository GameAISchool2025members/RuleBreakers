using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool hasPiece;
    public int pieceTeam;

    public string coords;

    public bool isLegalForP1 = true;
    public bool isLegalForP2 = true;

    public List<GameObject> pieces = new List<GameObject>();
    public GameManager gameManager;
    public GameObject spawner;

    public Renderer rend;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rend = GetComponent<Renderer>();
        coords = GetCoordinates();
        // Debug.Log(coords);

        hasPiece = false;
        pieceTeam = 0;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        changeColour();
    }

    string GetCoordinates()
    {
        var parentName = transform.parent.name;
        return (parentName + transform.name);
    }

    public void placePiece()
    {
        int currentPlayer = gameManager.currentPlayer;
        pieceTeam = currentPlayer;
        hasPiece = true;
        setTileLegal(false, currentPlayer);
        GameObject newPiece = Instantiate(pieces[currentPlayer - 1], spawner.transform.position, spawner.transform.rotation);
        newPiece.transform.SetParent(gameManager.transform);

        Piece piece = newPiece.GetComponent<Piece>();
        piece.SetPlayer(currentPlayer);
        piece.SetCoords(coords);

        if (currentPlayer == 1)
        {
            gameManager.addP1Piece(newPiece);
            gameManager.addP1Coords(coords);
        }
        else
        {
            gameManager.addP2Piece(newPiece);
            gameManager.addP2Coords(coords);
        }

        gameManager.checkIfGameOver();
    }

    void OnMouseOver()
    {
        int player = GameManager.gameManager.currentPlayer;
        if ((player == 1 && isLegalForP1) || (player == 2 && isLegalForP2))
        {
            if (Input.GetMouseButtonDown(0))
            {
                placePiece();
            }
        }
    }

    public void setTileLegal(bool isLegal, int player)
    {
        if (player == 1)
        {
            this.isLegalForP1 = isLegal;
        }
        else if (player == 2)
        {
            this.isLegalForP2 = isLegal;
        }

        changeColour();
    }

    public void changeColour()
    {
        int player = GameManager.gameManager.currentPlayer;
        bool isLegal = false;

        if (player == 1 && isLegalForP1)
        {
            isLegal = isLegalForP1;
        }
        else if (player == 2 && isLegalForP2)
        {
            isLegal = isLegalForP2;
        }

        if (isLegal)
        {
            rend.material.color = Color.blue;
        }
        else
        {
            rend.material.color = Color.red;
        }
    }
}
