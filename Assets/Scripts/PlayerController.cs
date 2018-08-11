using System.Collections;
using System.Collections.Generic;
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
            transform.position = newPos;
        }
        else if (BoardController.instance.IsMovable(newPos, direction))
        {
            BoardController.instance.MoveTile(newPos, direction);
            transform.position = newPos;
        }
    }
}
