using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeController : MonoBehaviour {

    public GameObject explosion;

    void ExplodeBomb()
    {
        if (GameController.instance.gameState == GameController.GameState.Play)
        {
            List<Vector3> neighbours = GetNeighbours();

            FindObjectOfType<AudioController>().Play("BombSound");
            Instantiate(explosion, transform.position, Quaternion.identity);
            BoardController.instance.RemovePiecesFromBoard(neighbours, 1);
        }
    }

    List<Vector3> GetNeighbours()
    {
        List<Vector3> neighbours = new List<Vector3>();
        for(int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 pos = new Vector3(transform.position.x + x, transform.position.y + y, 0);

                if (CheckIfPlayerTooClose(pos))
                    break;

                if (!BoardController.instance.IsWall(pos) && BoardController.instance.IsTile(pos))
                    neighbours.Add(pos);
            }
        }
        return neighbours;
    }

    private bool CheckIfPlayerTooClose(Vector3 coord)
    {
        if (coord == GameController.instance.player.transform.position)
        {
            GameController.instance.GameOver();
            return true;
        }
        return false;
    }
}
