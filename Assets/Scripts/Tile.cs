using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int XIndex;
    public int YIndex;

    public TileType TilesType = TileType.Normal;

    private Board _board;



    public enum TileType
    {
        Normal,
        Obstacle
    }
   
    public void Init(int x, int y, Board board)
    {
        XIndex = x;
        YIndex = y;

        _board = board;
    }

    private void OnMouseDown()
    {
        if (_board != null)
        {
            if (_board.FirstClick)
            {
                _board.ClickTile(this);
            }                                  
            else
            {
                _board.SecondClickTile(this);
            }                                                  
        }
    }

}
