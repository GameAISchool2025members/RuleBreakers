using UnityEngine;

public class Piece : MonoBehaviour
{
    public int player;
    public string coords;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void SetPlayer(int player)
    {
        this.player = player;
    }

    public void SetCoords(string coords)
    {
        this.coords = coords;
        Debug.Log(this.coords);
    }
}
