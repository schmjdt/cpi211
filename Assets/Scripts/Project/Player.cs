using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Player
{
    public int maxHealth = 20;
    public int health;
    public int score;
    public int attackValue;
    public int defenseValue;
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
        attackValue     = 0;
        defenseValue    = 0;
        spiritValue     = 0;
        score           = 0;
    }

    public string getPlayerName() {
        return playerName;
    }


    public void attack()
    {
        attackValue = getDiceValue(getDice("zoneSummoned"), Side.valueTypes.ATTACK);
        Debug.Log(playerName + " deals " + attackValue + " damage!");
    }

    public int getAttackValue()     { return attackValue;                                   }
    public int getDefenseValue()    { return defenseValue;                                  }
    public Die[] getDice(string zone)          { return GameLogic.instance.gameLayout.getDiceInZone(playerName, zone);  }


    public int getDiceValue(Die[] dice, Side.valueTypes vt) {
        int val = 0;
        foreach (Die die in dice) {
            val += die.getSideValue(vt);
        }
        return val;
    }

    public void defend(int attackValue)
    { //defenseValue = getDiceValue(getDice(), Side.valueTypes.DEFENSE); Die[] sDice = getDice(); Array.Sort(sDice);
        Debug.Log(playerName + " defends against " + defenseValue + " damage!");
    }
}