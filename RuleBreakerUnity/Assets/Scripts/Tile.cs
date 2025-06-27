using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool hasPiece;
    public int pieceTeam;

    public string coords;

    public bool isLegal = true;

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
        setTileLegal(false);
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
        if (isLegal)
        {
            if (Input.GetMouseButtonDown(0))
            {
                placePiece();
            }
        }
    }

    public void setTileLegal(bool isLegal)
    {
        this.isLegal = isLegal;
        changeColour();
    }

    void changeColour()
    {
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
