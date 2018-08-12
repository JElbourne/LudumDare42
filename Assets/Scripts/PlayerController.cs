
using UnityEngine;

public class PlayerController : MonoBehaviour {

    // Use this for initialization
    private void Start () {
        //Camera.main.GetComponent<FollowPlayer>().Setup(transform);
	}

    // Update is called once per frame
    private void Update () {
        if (GameController.instance.gameState == GameController.GameState.Play)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Move(0f, 1f);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Move(0f, -1f);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Move(-1f, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Move(1f, 0f);
            }
        }
    }

    private void Move(float x, float y)
    {
        Vector3 newPos = new Vector3(transform.position.x + x, transform.position.y + y, 0);
        Vector3 direction = new Vector3(x, y, 0);
        if (BoardController.instance.IsWalkable(newPos))
        {
            BoardController.instance.UpdateOpenSpacesWithPlayer(newPos, transform.position);
            transform.position = newPos;
            return;
        }
        else if (BoardController.instance.IsMovable(newPos, direction))
        {
            BoardController.instance.MoveTile(newPos, direction);
            BoardController.instance.UpdateOpenSpacesWithPlayer(newPos, transform.position);
            transform.position = newPos;
            return;
        }

        // If we got here that means we couldnt move so lets check if we are stuck
        if (!CheckNeighboursForMoveOption())
        {
            GameController.instance.GameOver();
        }
    }

    private bool CheckNeighboursForMoveOption()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                Vector3 pos = new Vector3(transform.position.x + x, transform.position.y + y, 0);

                if (BoardController.instance.IsWalkable(pos))
                    return true;
                if (BoardController.instance.IsMovable(pos, new Vector3(-1, 0, 0)))
                    return true;
                if (BoardController.instance.IsMovable(pos, new Vector3(1, 0, 0)))
                    return true;
                if (BoardController.instance.IsMovable(pos, new Vector3(0, -1, 0)))
                    return true;
                if (BoardController.instance.IsMovable(pos, new Vector3(0, 1, 0)))
                    return true;
            }
        }
        return false;
    }
}
