using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Player
{
    public int maxHealth = 20;
    public int health;
    public int score;
    
    public int spiritValue;

    int playerID;
    string playerName;

    public Player(int id) {
        health = maxHealth;
        playerID = id;
        playerName = "Player" + (playerID + 1).ToString();
    }

    public void resetPlayer()
    {
        health          = maxHealth;
        spiritValue     = 0;
        score           = 0;
    }

    public string getPlayerName() {
        return playerName;
    }
    
}