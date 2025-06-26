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
        gameManager.countTile();

        if (isLegal)
        {
            rend.material.color = Color.blue;
        }
        else
        {
            rend.material.color = Color.red;
        }
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
        isLegal = false;
        rend.material.color = Color.red;
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

        gameManager.swapPlayer();
    }

    void OnMouseOver()
    {
        if (isLegal)
        {
            if (Input.GetMouseButtonDown(0))
            {
                placePiece();
                rend.material.color = Color.red;
            }
        }
    }
}
