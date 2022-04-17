using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    public int Width;
    public int Height;
    public int BorderSize;

    public bool FirstClick = true;

    [SerializeField] private GameObject[] _gamePiecePrefabs;
    [SerializeField] private GameObject _tileNormalPrefab;
    [SerializeField] private GameObject _tileObstaclePrefab;

    public float swapTime = 0.1f;

    private Tile[,] _allTiles;
    private GamePiece[,] _allGamePieces;

    private Tile _clickedTile;
    private Tile _targetTile;

    private bool _playerInputEnabled = true;

    private List<int> usedValues = new List<int>();
  

    private void Start()
    {
        _allTiles = new Tile[Width, Height];
        _allGamePieces = new GamePiece[Width, Height];

        SetupTiles();
        SetupCamera();
        FillBoard(10, 0.5f);

    }

    private void SetupTiles()
    {
        int obstaclesNumber = 3;

        while (obstaclesNumber > 0)
        {         
            var xPos = UniqueRandomInt(0, Width);
            var yPos = UniqueRandomInt(0, Height);

            usedValues.Add(xPos);
            usedValues.Add(yPos);

            MakeTile(_tileObstaclePrefab, xPos, yPos);
            obstaclesNumber--;
        }
         

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (_allTiles[i, j] == null)
                {
                    MakeTile(_tileNormalPrefab, i, j);
                }

            }
        }
    }

    private int UniqueRandomInt(int min, int max)
    {
        int val = Random.Range(min, max);
        while (usedValues.Contains(val))
        {
            val = Random.Range(min, max);
        }
        return val;
    }

    private void MakeTile(GameObject prefab, int x, int y, int z = 0)
    {
        if (prefab != null)
        {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            tile.name = "Tile (" + x + "," + y + ")";

            _allTiles[x, y] = tile.GetComponent<Tile>();
            tile.transform.parent = transform;

            _allTiles[x, y].Init(x, y, this);
        }
    }

    private void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)(Width - 1) / 2f, (float)(Height - 1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)Height / 2f + (float)BorderSize;
        float horizontalSize = ((float)Width / 2f + (float)BorderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    private GameObject GetRandomGamePiece()
    {
        int randomInx = Random.Range(0, _gamePiecePrefabs.Length);

        return _gamePiecePrefabs[randomInx];
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        if (isWithinBounds(x, y))
        {
            _allGamePieces[x, y] = gamePiece;
        }
        gamePiece.SetCoordinates(x, y);
    }

    bool isWithinBounds(int x, int y)
    {
        return (x >= 0 && x < Width && y >= 0 && y < Height);
    }

    private void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxInteration = 100;
        int iterations = 0;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (_allGamePieces[i, j] == null && _allTiles[i,j].TilesType != Tile.TileType.Obstacle)
                {
                    GamePiece piece = FillRandomAt(i, j, falseYOffset, moveTime);
                    iterations = 0;

                    while (HasMatchOnFill(i, j))
                    {
                        ClearPieceAt(i, j);
                        piece = FillRandomAt(i, j, falseYOffset, moveTime);
                        iterations++;

                        if (iterations > maxInteration)
                        {
                            Debug.Log("MaxInteration");
                            break;
                        }
                    }
                }
            }
        }

    }

    private bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        if (leftMatches == null)
            leftMatches = new List<GamePiece>();

        if (downwardMatches == null)
            downwardMatches = new List<GamePiece>();

        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
    }

    private GamePiece FillRandomAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;

        if (randomPiece != null)
        {
            randomPiece.GetComponent<GamePiece>().Init(this);
            PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), x, y);

            if (falseYOffset != 0)
            {
                randomPiece.transform.position = new Vector3(x, y - falseYOffset, 0);
                randomPiece.GetComponent<GamePiece>().Move(x, y, moveTime);
            }


            randomPiece.transform.parent = transform;
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    public void ClickTile(Tile tile)
    {
        if (_clickedTile == null)
        {
            _clickedTile = tile;
            HighlightTileOn(tile.XIndex, tile.YIndex, Color.yellow);
            FirstClick = false;
        }

    }

    public void SecondClickTile(Tile tile)
    {
        if (_clickedTile != null && IsNextTo(tile, _clickedTile))
        {
            _targetTile = tile;
            FirstClick = true;
            CheckSwitchTile();
        }
        else
        {
            HighlightTileOff(_clickedTile.XIndex, _clickedTile.YIndex);
            _clickedTile = tile;
            HighlightTileOn(tile.XIndex, tile.YIndex, Color.yellow);
            FirstClick = false;
        }
    }

    public void CheckSwitchTile()
    {
        if (_clickedTile != null && _targetTile != null)
        {
            SwitchTiles(_clickedTile, _targetTile);
            HighlightTileOff(_clickedTile.XIndex, _clickedTile.YIndex);
        }

        HighlightTileOff(_clickedTile.XIndex, _clickedTile.YIndex);

        _clickedTile = null;
        _targetTile = null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (_playerInputEnabled)
        {
            GamePiece clickedPiece = _allGamePieces[clickedTile.XIndex, clickedTile.YIndex];
            GamePiece targetPiece = _allGamePieces[targetTile.XIndex, targetTile.YIndex];

            if (targetPiece != null && clickedPiece != null)
            {
                clickedPiece.Move(targetTile.XIndex, targetTile.YIndex, swapTime);
                targetPiece.Move(clickedTile.XIndex, clickedTile.YIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.XIndex, clickedTile.YIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.XIndex, targetTile.YIndex);

                if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.XIndex, clickedTile.YIndex, swapTime);
                    targetPiece.Move(targetTile.XIndex, targetTile.YIndex, swapTime);
                }
                else
                {
                    yield return new WaitForSeconds(swapTime);

                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());

                }
            }
        }
    }

    private bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.XIndex - end.XIndex) == 1 && start.YIndex == end.YIndex)
        {
            return true;
        }

        if (Mathf.Abs(start.YIndex - end.YIndex) == 1 && start.XIndex == end.XIndex)
        {
            return true;
        }

        return false;
    }

    private List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        GamePiece startPiece = null;

        if (isWithinBounds(startX, startY))
        {
            startPiece = _allGamePieces[startX, startY];
        }

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;

        int maxValue = (Width > Height) ? Width : Height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!isWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = _allGamePieces[nextX, nextY];

            if (nextPiece == null)
            {
                break;
            }
            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
            }


        }

        if (matches.Count >= minLength)
        {
            return matches;
        }

        return null;
    }

    private List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    private List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        var combinedMatches = rightMatches.Union(leftMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    private void HighlightMatches()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    private void HighlightPieces(List<GamePiece> gamePieces)
    {
        foreach (var item in gamePieces)
        {
            if (item != null)
                HighlightTileOn(item.XIndex, item.YIndex, item.GetComponent<SpriteRenderer>().color);
        }
    }

    private void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);

        List<GamePiece> combinedMatches = FindMatchesAt(x, y);

        if (combinedMatches.Count > 0)
        {
            foreach (var item in combinedMatches)
            {
                HighlightTileOn(item.XIndex, item.YIndex, item.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void HighlightTileOff(int x, int y)
    {
        SpriteRenderer spriteRenderer = _allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }

    private void HighlightTileOn(int x, int y, Color col)
    {
        SpriteRenderer spriteRenderer = _allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = col;
    }

    private List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> verticMatches = FindVerticalMatches(x, y, minLength);

        if (horizMatches == null)
            horizMatches = new List<GamePiece>();

        if (verticMatches == null)
            verticMatches = new List<GamePiece>();

        var combinedMatches = horizMatches.Union(verticMatches).ToList();
        return combinedMatches;
    }

    private List<GamePiece> FindMatchesAt(List<GamePiece> gamePiece, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (var item in gamePiece)
        {
            matches = matches.Union(FindMatchesAt(item.XIndex, item.YIndex, minLength)).ToList();
        }

        return matches;
    }

    private List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }

        return combinedMatches;
    }

    private void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = _allGamePieces[x, y];

        if (pieceToClear != null)
        {
            _allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }

        HighlightTileOff(x, y);
    }

    private void ClearBoard()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }

    private void ClearPieceAt(List<GamePiece> gamePiece)
    {
        AddScore(gamePiece);

        foreach (var item in gamePiece)
        {
            if (item != null)
            {
                ClearPieceAt(item.XIndex, item.YIndex);
            }
        }
    }

    private static void AddScore(List<GamePiece> gamePiece)
    {       
        if (gamePiece.Count  == 3)       
            ScoreManager.Instance.AddScore(10);     

        if (gamePiece.Count == 4)       
            ScoreManager.Instance.AddScore(15);      

        if (gamePiece.Count == 5 || gamePiece.Count == 6)        
            ScoreManager.Instance.AddScore(20);
        
        if (gamePiece.Count == 7)        
            ScoreManager.Instance.AddScore(25);
        
        if (gamePiece.Count == 8 || gamePiece.Count == 9)       
            ScoreManager.Instance.AddScore(30);
        
        if (gamePiece.Count == 10 || gamePiece.Count == 11 || gamePiece.Count == 12)       
            ScoreManager.Instance.AddScore(40);
        
    }

    private List<GamePiece> CollapseColumn(int column, float collapseTime = 0.25f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = Height - 1; i >= 0; i--)
        {
            if (_allGamePieces[column, i] == null && _allTiles[column, i].TilesType!=Tile.TileType.Obstacle)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (_allGamePieces[column, j] != null)
                    {
                        _allGamePieces[column, j].Move(column, i, collapseTime * (i - j));
                        _allGamePieces[column, i] = _allGamePieces[column, j];
                        _allGamePieces[column, i].SetCoordinates(column, i);

                        if (!movingPieces.Contains(_allGamePieces[column, i]))
                        {
                            movingPieces.Add(_allGamePieces[column, i]);
                        }

                        _allGamePieces[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    private List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> columnsToCollapse = GetColumns(gamePieces);

        foreach (var item in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(item)).ToList();
        }

        return movingPieces;
    }

    private List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (var item in gamePieces)
        {
            if (!columns.Contains(item.XIndex))
            {
                columns.Add(item.XIndex);
            }
        }
        return columns;
    }

    private void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        _playerInputEnabled = false;
        List<GamePiece> matches = gamePieces;

        do
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return null;

            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();

            yield return new WaitForSeconds(0.5f);
        }
        while (matches.Count != 0);

        _playerInputEnabled = true;
    }

    IEnumerator RefillRoutine()
    {
        FillBoard(3, 1f);
        yield return null;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        HighlightPieces(gamePieces);

        yield return new WaitForSeconds(0.5f);
        bool isFinished = false;

        while (!isFinished)
        {
            ClearPieceAt(gamePieces);

            yield return new WaitForSeconds(0.25f);
            movingPieces = CollapseColumn(gamePieces);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);
            matches = FindMatchesAt(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }

    private bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (var item in gamePieces)
        {
            if (item != null)
            {
                if (item.transform.position.y - (float)item.YIndex > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
