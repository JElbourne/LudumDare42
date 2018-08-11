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

    public Tile(bool _movable, bool _destructable, int _value, GameObject _gameObject)
    {
        movable = _movable;
        destructable = _destructable;
        value = _value;
        gameObject = _gameObject;
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

    public Color wallTint;
    public Color unbreakableTint;

    Dictionary<Vector3, Tile> board = new Dictionary<Vector3, Tile>();
    List<Vector3> openSpaces = new List<Vector3>();

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
	
	void Update () {
        // The game will check for connecting pieces to explode
		
	}

    #region Public Methods

    public bool SpawnNewPiece()
    {
        if (openSpaces.Count < 2)
            return false;

        int attempts = 0;
        bool foundCoord = false;
        int index = 0;
        Vector3 coord = openSpaces[index];
        
        while (attempts < (boardSize.x * boardSize.y))
        {
            index = Random.Range(0, openSpaces.Count);
            coord = openSpaces[index];
            if (coord != GameController.instance.player.transform.position)
            {
                foundCoord = true;
                openSpaces.RemoveAt(index);
                break;
            }
        }
        if (!foundCoord)
            return false;

        GameObject obj = Instantiate(obsticle, coord, Quaternion.identity);
        bool destructable = true;
        if (Random.value < GameController.instance.indestructableRatio)
        {
            destructable = false;
            obj.GetComponent<SpriteRenderer>().color = unbreakableTint;
        }
        board[coord] = new Tile(true, destructable, 1, obj);
        return true;
    }

    public void MoveTile(Vector3 _coord, Vector3 _direction)
    {
        UpdateBoardData(_coord, _direction);
        BoardController.instance.CheckForCompletedLines();
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
                    board[pos] = new Tile(false, false, 0, obj);
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

        foreach(Vector3 coord in _toBeDestroyed)
        {
            if (board.ContainsKey(coord))
            {
                Tile tile = board[coord];
                board.Remove(coord);
                openSpaces.Add(coord);
                GameController.instance.Score += tile.value;
                Destroy(tile.gameObject);
            }
        }
    }
}
