using UnityEngine;
using TMPro;

public class UpdateScore : MonoBehaviour {

    public TextMeshProUGUI ui;
    public IntVariable score;
	
	// Update is called once per frame
	void Update () {
        ui.text = score.value.ToString();
	}
}
