using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class GameMenu : MonoBehaviour {
    
    public GameObject menuPanel;
    public bool panelOpen;
    public KeyCode menuHotkey;

	// Use this for initialization
	void Start ()
    {
        Debug.Log("Menu Launched");
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(menuHotkey))
        {
            togglePanel(!panelOpen);
        }
    }

    public void togglePanel(bool b)
    {
        panelOpen = b;
        menuPanel.SetActive(b);
    }
    
    public void btnExit()
    {
        Debug.Log("Exiting");
        Application.Quit();
    }


    public void gotoScene(string s)
    {
        Application.LoadLevel(s);
    }
}
