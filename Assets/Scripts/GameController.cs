using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public GameObject menuUI;
    public GameObject gameOverUI;
    public GameObject highScoreFlashUI;
    public GameObject player;
    public IntVariable score;
    public IntVariable destruction;
    public IntVariable piecesDropped;
    public int regularPieceValue = 1;
    public int unBreakablePieceValue = 5;
    public int matchGoal = 4;
    public int scoreLeveling = 50;
    public float maxPieceDelay = 2f;
    public float minPieceDelay = 0.5f;
    public float pieceDelayDecrement = 0.5f;
    public float minIndestructableRatio = 0.0f;
    public float maxIndestructableRatio = 0.25f;
    public float indestructableIncrement = 0.05f;
    public float destroyPiecesDelay = 0.35f;
    public int bombRewardScore = 10;
    public float bombTimeLimit = 5f;

    [HideInInspector]
    public float indestructableRatio;

    float timeUntilNextPiece = 0f;
    float newPieceDelay;
    int highScore;
    int highDestruction;
    int highDropped;

    public enum GameState {
        Menu,
        Play,
        GameOver,
    }

    public GameState gameState;

    #region instance

    public static GameController instance;

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

    // Use this for initialization
    private void Start () {
        newPieceDelay = timeUntilNextPiece = maxPieceDelay;
        indestructableRatio = minIndestructableRatio;
        gameState = GameState.Menu;
        gameOverUI.SetActive(false);
        menuUI.SetActive(true);
        SetHighScores();
    }

    // Update is called once per frame
    private void Update () {
        if(gameState == GameState.Play)
        {
            PlayUpdate();
        }
        else if(gameState == GameState.Menu)
        {
            MenuUpdate();
        }
        else if (gameState == GameState.GameOver)
        {
            GameOverUpdate();
        }
    }

    private void PlayUpdate()
    {
        timeUntilNextPiece -= Time.deltaTime;
    }

    private void MenuUpdate()
    {
        if(Input.anyKeyDown)
        {
            NewGame();
        }
    }

    private void GameOverUpdate()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void LateUpdate()
    {
        if (gameState == GameState.Play && timeUntilNextPiece <= 0f)
        {
            timeUntilNextPiece = newPieceDelay;
            SpawnNewPiece();
        }
    }

    private void SetHighScores()
    {
        highScore = 0;
        highDropped = 0;
        highDestruction = 0;
    }

    public void NewGame()
    {
        if (gameState != GameState.Play)
        {
            gameOverUI.SetActive(false);
            menuUI.SetActive(false);
            score.value = 0;
            CreateBoard();
            CreatePlayer();
            CheckMatchGoalToBoardSize();
            gameState = GameState.Play;
        }
    }

    public void IncreaseDifficulty()
    {
        Debug.Log("Increaing Dificulty");
        newPieceDelay = Mathf.Clamp(newPieceDelay - pieceDelayDecrement, minPieceDelay, maxPieceDelay);
        indestructableRatio = Mathf.Clamp(indestructableRatio + indestructableIncrement, minIndestructableRatio, maxIndestructableRatio);
    }

    private void CheckMatchGoalToBoardSize()
    {
        if (matchGoal > BoardController.instance.boardSize.x - 2)
        {
            matchGoal = (int)BoardController.instance.boardSize.x - 2;
        }
        if (matchGoal > BoardController.instance.boardSize.y - 2)
        {
            matchGoal = (int)BoardController.instance.boardSize.y - 2;
        }
    }

    private void SpawnNewPiece()
    {
        bool succeed = BoardController.instance.SpawnNewPiece();
        if (!succeed)
            GameOver();
    }

    private void CreateBoard()
    {
        BoardController.instance.CreateBoard();
    }

    private void CreatePlayer()
    {
        Vector2 boardSize = BoardController.instance.boardSize;
        Vector3 startPos = new Vector3((boardSize.x - 1) / 2, (boardSize.y - 1) / 2, 0);
        Instantiate(player, startPos, Quaternion.identity);
    }

    private void GameOver()
    {
        gameState = GameState.GameOver;
        gameOverUI.SetActive(true);
        highScoreFlashUI.SetActive(false);
        if (score.value > highScore)
        {
            highScoreFlashUI.SetActive(true);
        }

        Debug.Log("Game Over!");
    }
}
