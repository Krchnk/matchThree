using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int XIndex;
    public int YIndex;
    public MatchValue matchValue;

    private Board _board;
    private bool _isMoving;

    public void SetCoordinates(int x, int y)
    {
        XIndex = x;
        YIndex = y;
    }

    public enum MatchValue
    {
        yellow,
        blue,
        red,
        green
    }



    public void Init( Board board)
    {
        _board = board;
    }

    public void Move(int destX, int destY, float timeToMove)
    {
        if(!_isMoving)
        StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
    }

    IEnumerator MoveRoutine(Vector3 dest, float timeToMove)
    {
        Vector3 startPos = transform.position;
        bool reachedDest = false;
        float elapsedTime = 0f;

        _isMoving = true;

        while (!reachedDest)
        {
            if(Vector3.Distance(transform.position, dest) < 0.01f)
            {
                reachedDest = true;

                if (_board != null)
                {
                    _board.PlaceGamePiece(this, (int)dest.x, (int)dest.y);
                }

                break;
            }

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp(elapsedTime / timeToMove, 0,1);
            transform.position = Vector3.Lerp(startPos, dest, t);

            yield return null;
        }
        _isMoving = false;
    }

}
