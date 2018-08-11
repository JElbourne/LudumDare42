using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

    Transform player;

	// Use this for initialization
	public void Setup (Transform _player) {
        player = _player;
        SetNewPosition();
    }
	
	// Update is called once per frame
	void LateUpdate () {
        if (player)
        {
            SetNewPosition();
        }
	}

    void SetNewPosition()
    {
        transform.position = new Vector3(
            player.position.x,
            player.position.y,
            transform.position.z);
    }
}
