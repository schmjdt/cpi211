using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GameMenu : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        Debug.Log("Menu Launched");
    }
	
	// Update is called once per frame
	void Update ()
    {
    }


    public void btnNewGame()
    {
        Debug.Log("Loading Game");
        Application.LoadLevel(1);
    }

    public void btnExit()
    {
        Debug.Log("Exiting");
        Application.Quit();
    }
}
