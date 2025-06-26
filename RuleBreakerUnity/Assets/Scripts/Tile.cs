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
    }

    (string, string) GetCoordinates()
    {
        var parentName = transform.parent.name;
        return (parentName, transform.name);
    }

    public void placePiece(int currentPlayer)
    {
        pieceTeam = currentPlayer;
        hasPiece = true;
        isLegal = false;
        GameObject piece = Instantiate(pieces[currentPlayer - 1], spawner.transform.position, spawner.transform.rotation);
        
        if (currentPlayer == 1)
        {
            gameManager.addP1Piece(piece);
        }
        else
        {
            gameManager.addP2Piece(piece);
        }
    }

    void OnMouseEnter()
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

    void OnMouseOver()
    {
        if (isLegal)
        {
            rend.material.color -= new Color(0, 0, 0.1F) * Time.deltaTime;
            if (Input.GetMouseButtonDown(0))
            {
                placePiece(1);
                rend.material.color = Color.red;
            }
        }
    }

    void OnMouseExit()
    {
        rend.material.color = Color.white;
    }
}
