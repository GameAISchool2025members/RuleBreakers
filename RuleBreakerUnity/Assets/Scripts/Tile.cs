using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool hasPiece;
    public int pieceTeam;

    public (string, string) coords;

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

        if (isLegal)
        {
            rend.material.color = Color.blue;
        }
        else
        {
            rend.material.color = Color.red;
        }
    }

    (string, string) GetCoordinates()
    {
        var parentName = transform.parent.name;
        return (parentName, transform.name);
    }

    public void placePiece()
    {
        int currentPlayer = gameManager.currentPlayer;
        pieceTeam = currentPlayer;
        hasPiece = true;
        isLegal = false;
        rend.material.color = Color.red;
        GameObject piece = Instantiate(pieces[currentPlayer - 1], spawner.transform.position, spawner.transform.rotation);
        piece.transform.SetParent(gameManager.transform);
        
        if (currentPlayer == 1)
        {
            gameManager.addP1Piece(piece);
        }
        else
        {
            gameManager.addP2Piece(piece);
        }

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
