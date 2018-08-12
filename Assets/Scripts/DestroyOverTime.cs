using UnityEngine;

public class DestroyOverTime : MonoBehaviour {

    public float destroyDelay;

    float cooldown;

	// Use this for initialization
	void Start () {
        cooldown = destroyDelay;
	}
	
	// Update is called once per frame
	void Update () {
        cooldown -= Time.deltaTime;

        if (cooldown <= 0)
        {
            RemoveObject();
        }
	}

    void RemoveObject()
    {
        Destroy(gameObject);
    }
}
