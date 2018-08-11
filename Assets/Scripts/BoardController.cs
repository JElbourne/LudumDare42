using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class Tile
{
    bool movable = true;
    public GameObject gameObject;
    public bool destructable = false;
    public int value = 0;
    public bool wall = false;

    public Tile(bool _movable, bool _destructable, bool _wall, int _value, GameObject _gameObject)
    {
        movable = _movable;
        destructable = _destructable;
        value = _value;
        gameObject = _gameObject;
        wall = _wall;
    }

    public bool IsMovable(Vector3 direction)
    {
        if (!movable)
            return false;

        Vector2 neighbour = gameObject.transform.position + direction;
        return BoardController.instance.IsWalkable(neighbour);
    }

    public void Move(Vector3 direction)
    {
        gameObject.transform.position += direction;
    }

    public Vector3 GetPosition()
    {
        return gameObject.transform.position;
    }

}

public class BoardController : MonoBehaviour {

    public Vector2 boardSize;
    public GameObject obsticle;
    public GameObject bomb;

    public Color wallTint;
    public Color unbreakableTint;

    Dictionary<Vector3, Tile> board = new Dictionary<Vector3, Tile>();
    List<Vector3> openSpaces = new List<Vector3>();
    Vector3 nextSpawnSpace;

    #region instance

    public static BoardController instance;

    private void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Public Methods

    bool FindOpenSpace()
    {
        int index = 0;
        int attempts = 0;

        while (attempts < (boardSize.x * boardSize.y))
        {
            index = Random.Range(0, openSpaces.Count);
            nextSpawnSpace = openSpaces[index];
            if (nextSpawnSpace != GameController.instance.player.transform.position)
            {
                openSpaces.RemoveAt(index);
                return true;
            }
            attempts++;
        }
        return false;
    }

    public void SpawnBomb()
    {
        Debug.Log("Spawn Bomb");
        if (openSpaces.Count < 2)
            return;

        if (FindOpenSpace())
        {
            GameObject obj = Instantiate(bomb, nextSpawnSpace, Quaternion.identity);
            board[nextSpawnSpace] = new Tile(true, false, false, 0, obj);
        }
    }

    public bool SpawnNewPiece()
    {
        if (openSpaces.Count < 2)
            return false;

        if (FindOpenSpace())
        {
            GameObject obj = Instantiate(obsticle, nextSpawnSpace, Quaternion.identity);
            bool destructable = true;
            int pieceValue = GameController.instance.regularPieceValue;
            if (Random.value < GameController.instance.indestructableRatio)
            {
                destructable = false;
                obj.GetComponent<SpriteRenderer>().color = unbreakableTint;
                pieceValue = GameController.instance.unBreakablePieceValue;
            }
            board[nextSpawnSpace] = new Tile(true, destructable, false, pieceValue, obj);
            GameController.instance.piecesDropped.value++;
            CheckForCompletedLines();
            return true;
        }

        return false;
    }

    public void MoveTile(Vector3 _coord, Vector3 _direction)
    {
        UpdateBoardData(_coord, _direction);
        CheckForCompletedLines();
    }

    public bool IsWalkable(Vector3 coord)
    {
        return openSpaces.Contains(coord);
    }

    public bool IsMovable(Vector3 _coord, Vector3 _direction)
    {
        if (board.ContainsKey(_coord))
        {
            return board[_coord].IsMovable(_direction);
        }
        return false;
    }

    public bool IsWall(Vector3 _coord)
    {
        if (board.ContainsKey(_coord))
        {
            return board[_coord].wall;
        }
        return false;
    }

    public bool IsTile(Vector3 _coord)
    {
        return board.ContainsKey(_coord);
    }

    public void CreateBoard()
    {
        for(int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                if (x==0 || x== boardSize.x-1 || y==0 || y== boardSize.y-1)
                {
                    GameObject obj = Instantiate(obsticle, pos, Quaternion.identity);
                    obj.GetComponent<SpriteRenderer>().color = wallTint;
                    board[pos] = new Tile(false, false, true, 0, obj);
                } else
                {
                    openSpaces.Add(pos);
                }
            }
        }
    }

    #endregion
    void CheckForCompletedLines()
    {
        int matchGoal = GameController.instance.matchGoal;
        List<Vector3> toBeDestroyed = new List<Vector3>();
        toBeDestroyed = CheckForCompletedRows(toBeDestroyed, matchGoal);
        toBeDestroyed = CheckForCompletedColumns(toBeDestroyed, matchGoal);
        if(toBeDestroyed.Count > 0)
            StartCoroutine(DestroyPieces(toBeDestroyed));
    }

    List<Vector3> CheckForCompletedRows(List<Vector3> _toBeDestroyed, int _matchGoal)
    {
        List<Vector3> joinedToBeDestroyed = _toBeDestroyed;
        List<Vector3> localToBeDestroyed;
        // Check Vertical first
        for (int x = 0; x < boardSize.x; x++)
        {
            localToBeDestroyed = new List<Vector3>();
            for (int y = 0; y < boardSize.y; y++)
            {
                Vector3 coord = new Vector3(x, y, 0);
                if (board.ContainsKey(coord) && board[coord].destructable)
                {
                    localToBeDestroyed.Add(coord);
                }
                else
                {
                    if (localToBeDestroyed.Count >= _matchGoal)
                    {
                        joinedToBeDestroyed = _toBeDestroyed.Union<Vector3>(localToBeDestroyed).ToList<Vector3>();
                    }
                    // Clear to try and find a new match set in line.
                    localToBeDestroyed = new List<Vector3>();
                }
            }
        }
        return joinedToBeDestroyed;
    }

    List<Vector3> CheckForCompletedColumns(List<Vector3> _toBeDestroyed, int _matchGoal)
    {
        List<Vector3> joinedToBeDestroyed = _toBeDestroyed;
        List<Vector3> localToBeDestroyed;
        // Check Vertical first
        for (int y = 0; y < boardSize.y; y++)
        {
            localToBeDestroyed = new List<Vector3>();
            for (int x = 0; x < boardSize.x; x++)
            {
                Vector3 coord = new Vector3(x, y, 0);
                if (board.ContainsKey(coord) && board[coord].destructable)
                {
                    localToBeDestroyed.Add(coord);
                }
                else
                {
                    if (localToBeDestroyed.Count >= _matchGoal)
                    {
                        joinedToBeDestroyed = _toBeDestroyed.Union<Vector3>(localToBeDestroyed).ToList<Vector3>();
                    }
                    // Clear to try and find a new match set in line.
                    if (localToBeDestroyed.Count > 0)
                        localToBeDestroyed = new List<Vector3>();
                }
            }
        }
        return joinedToBeDestroyed;
    }

    void UpdateBoardData(Vector3 _coord, Vector3 _direction)
    {
        Tile tile = board[_coord];
        tile.Move(_direction);
        board[tile.GetPosition()] = tile;
        board.Remove(_coord);
        openSpaces.Add(_coord);
        openSpaces.Remove(tile.GetPosition());
    }

    IEnumerator DestroyPieces(List<Vector3> _toBeDestroyed)
    {
        yield return new WaitForSeconds(GameController.instance.destroyPiecesDelay);
        int scoreMultiplier = DetermineScoreMultiplier(_toBeDestroyed.Count);
        RemovePiecesFromBoard(_toBeDestroyed, scoreMultiplier);
        GameController.instance.IncreaseDifficulty();
        if (_toBeDestroyed.Count >= GameController.instance.bombRewardScore)
            Invoke("SpawnBomb", 2);
    }

    public void RemovePiecesFromBoard(List<Vector3> _toBeDestroyed, int scoreMultiplier)
    {
        
        foreach (Vector3 coord in _toBeDestroyed)
        {
            RemovePieceFromBoard(coord, scoreMultiplier);
        }

    }

    public void RemovePieceFromBoard(Vector3 coord, int scoreMultiplier)
    {
        if (board.ContainsKey(coord))
        {
            Tile tile = board[coord];
            board.Remove(coord);
            openSpaces.Add(coord);
            // Update Scores
            GameController.instance.score.value += (tile.value * scoreMultiplier);
            GameController.instance.destruction.value++;
            // Destroy the tile game object
            Destroy(tile.gameObject);
        }
    }

    private int DetermineScoreMultiplier(int _matchCount)
    {
        int multiplier = _matchCount - GameController.instance.matchGoal + 1;
        return multiplier;
    }
}
