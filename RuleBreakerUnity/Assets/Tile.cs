using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool hasPiece;
    public int pieceTeam;

    public (string, string) coords;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        coords = GetCoordinates();

    }

    (string, string) GetCoordinates()
    {
        var parentName = transform.parent.name;
        return (parentName, transform.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
