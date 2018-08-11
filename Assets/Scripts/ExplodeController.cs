using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeController : MonoBehaviour {

    float countDownTimer = 0;

	// Use this for initialization
	void Start () {
        countDownTimer = GameController.instance.bombTimeLimit;
	}
	
	// Update is called once per frame
	void Update () {
        if (GameController.instance.gameState == GameController.GameState.Play)
        {
            countDownTimer -= Time.deltaTime;
            if (countDownTimer <= 0)
            {
                ExplodeBomb();
            }
        }
	}

    void ExplodeBomb()
    {
        List<Vector3> neighbours = GetNeighbours();
        BoardController.instance.RemovePiecesFromBoard(neighbours, 1);
    }

    List<Vector3> GetNeighbours()
    {
        List<Vector3> neighbours = new List<Vector3>();
        for(int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 pos = new Vector3(transform.position.x + x, transform.position.y + y, 0);
                if (!BoardController.instance.IsWall(pos) && BoardController.instance.IsTile(pos))
                    neighbours.Add(pos);
            }
        }
        return neighbours;
    }
}
