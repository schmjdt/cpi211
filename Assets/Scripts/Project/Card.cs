using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class Card : MonoBehaviour {

	public RawImage CardUI;
    public Sides.sideValue[] sides = new Sides.sideValue[6];

    int numberSides;
    public int totalDice;
    public int diceOnCard;
    public string cardName;

    public Texture cardImage;
    public CardInfo cardInfo;

    public bool randomizeStart;
    
    public bool visible;

    // Use this for initialization 
    void Start () {
    }

    // Update is called once per frame
    void Update () {
	}

    void OnMouseOver()
    {
        checkCardImage();
    }

    void OnMouseExit()
    {
        if (visible) toggleCardImage();
    }


    public void checkCardImage()
    {
        if (Input.GetMouseButton(1))
        {
            if (!visible) toggleCardImage();
        }
        else
        {
            if (visible) toggleCardImage();
        }
    }

    public void toggleCardImage()
    {
        updateCardImage();
        visible = !visible;
    }

    public void updateCardImage()
    {
        string audio;
        if (visible) audio = "place";
        else         audio = "pickup";

        GameLogic.instance.cardLayout.toggleImage(!visible, cardImage, cardName);
        SoundControl.instance.playAudio("card", audio);
    }

    public void setSides()
    {
        for (int i = 0; i < sides.Length; i++)
        {
            sides[i] = (Sides.sideValue)cardInfo.sideID[i];
        }
    }

    public void initCard()
    {
        //Debug.Log("Card.initCard() <-- GL.Start()");
        //sides = new Side[dieSize];
        if (randomizeStart) cardInfo.randomizeValues();
        numberSides = sides.Length;

        buildMesh(GameLogic.instance.cardLayout.mesh);
        //setDice();

        setSides();

    }


    public void buildMesh(Mesh mesh)
    {
        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        MeshCollider mesh_collider = GetComponent<MeshCollider>();

        mesh_filter.mesh = mesh;
        mesh_collider.sharedMesh = mesh;

        createTexture();
    }


    public void createTexture()
    {
        cardInfo.texInfo = TextureBuilder.ChopUpAllTextures(cardInfo.texInfo);
        cardImage = TextureBuilder.BuildTexture(cardInfo);
        GetComponent<Renderer>().material.mainTexture = cardImage;
    }
    

    public void createTextureEditor(Mesh mesh)
    {
        if (randomizeStart) cardInfo.randomizeValues();
        buildMesh(mesh);
    }
    

    public void createDie()
    { 
        /* 
        Die newDie = new Die(this);
        dice.Add(newDie); 
        */
    }

    /*
    public void setDice()
    {
        // Get this Card's Zone
        Die[] d = this.transform.parent.parent.GetComponent<Zone>().getZoneDice();

        totalDice = d.Length;
        diceOnCard = totalDice;

        foreach (Die die in d)
        {
            die.card = this;
            die.setDieColor(cardInfo.outsideColor, cardInfo.insideColor);
        }
        // Set each Die to this Card's colors
    }
    */

    public Side getSide(int i) {
        //Debug.Log(i);
        return Sides.getSide((int)sides[i]);
    }

    public int getSideValue(int i, Side.eValueTypes vt) {
        //Debug.Log(i + "/" + vt);
        return getSide(i).getStat(vt);
    }

    public Side.eSideType getSideType(int i)
    {
        return getSide(i).getType();
    }

    public override string ToString() { return cardName; }




}

[System.Serializable]
public class CardInfo
{
    public TextureInfo[] texInfo;

    [Range(0, 9)]
    public int cost;
    [Range(0, 9)]
    public int score;


    public Color insideColor;
    public Color outsideColor;
    /*
    [Range(0f, 1f)]
    public float[] insideColorRGB = new float[3];
    [Range(0f, 1f)]
    public float[] outsideColorRGB = new float[3];
    */


    [Range(0, 9)]
    public int[] sideID = new int[6];

    public void randomizeValues()
    {
        randomColor();
        randomValues();
        randomSides();
    }

    public void randomColor()
    {
        float[] insideColorRGB = new float[3];
        float[] outsideColorRGB = new float[3];

        for (int i = 0; i < 3; i++)
        {
            insideColorRGB[i] = UnityEngine.Random.Range(0f, 1f);
            outsideColorRGB[i] = UnityEngine.Random.Range(0f, 1f);
        }
        insideColor = new Color(insideColorRGB[0], insideColorRGB[1], insideColorRGB[2]);
        outsideColor = new Color(outsideColorRGB[0], outsideColorRGB[1], outsideColorRGB[2]);
    }

    public void randomValues()
    {
        cost = UnityEngine.Random.Range(0, 9);
        score = UnityEngine.Random.Range(0, 9);
    }

    public void randomSides()
    {
        for (int i = 0; i < 6; i++)
        {
            sideID[i] = UnityEngine.Random.Range(0, 3);
        }
    }
}

public static class Sides
{
    public enum sideValue { E_1, E_2, C_112, C_232, REROLL };
    //static Side[] sides = new Side[3];
    static List<Side> sides = new List<Side>();

    public static void createSides() {
        sides.Add(new Side(1));
        sides.Add(new Side(2));
        sides.Add(new Side(1, 1, 2));
        sides.Add(new Side(2, 3, 2));
        sides.Add(new Side(0));
        sides[sides.Count - 1].setStar(Abilities.eAbilityType.REROLL, 1);
        /*
        sides[0] = new Side(1);
        sides[1] = new Side(2);
        sides[2] = new Side(1, 1, 2);
        sides[3] = new Side(2, 3, 2);
        */
    }
    public static Side getSide(int i) {
        return sides[i];
    }
}


public class Side
{
    public enum eValueTypes { ENERGY, COST, ATTACK, DEFENSE, LEVEL, STAR };
    public enum eSideType { ENERGY, SUMMONABLE, CASTABLE };

    int energy, cost, attack, defense, level;

    Abilities.eAbilityType star;
    public int starValue;

    eSideType sideType;

    public Side(int e)                              { setVals(e, 0, 0, 0, 0); }
    public Side(       int c, int a, int d)         { setVals(0, c, a, d, 0); }
    public Side(       int c, int a, int d, int l)  { setVals(0, c, a, d, l); }
    public Side(int e, int c, int a, int d, int l)  { setVals(e, c, a, d, l); }

    public void setVals(int e, int c, int a, int d, int l)
    {
        energy = e;
        cost = c;
        attack = a;
        defense = d;
        level = 1;

        if (e > 0)
            sideType = eSideType.ENERGY;
        else if (c > 0 || a > 0 || d > 0)
            sideType = eSideType.SUMMONABLE;
        else
            sideType = eSideType.CASTABLE;
    }

    public void setStar(Abilities.eAbilityType aT, int i) { star = aT; starValue = i; }

    public int getStat(eValueTypes vt) {
        switch (vt) {
            case eValueTypes.ENERGY:     return energy;
            case eValueTypes.COST:       return cost;
            case eValueTypes.ATTACK:     return attack;
            case eValueTypes.DEFENSE:    return defense;
            case eValueTypes.LEVEL:      return level;
            case eValueTypes.STAR:       return (int)star;
            default:                     return 0;
        }
    }

    public int getEnergy()  { return energy;    }
    public int getCost()    { return cost;      }
    public int getAttack()  { return attack;    }
    public int getDefense() { return defense;   }
    public int getLevel()   { return level;     }
    public Ability getStar() { return Abilities.abilities[(int)star]; }

    public eSideType getType() { return sideType;  }
}

public static class Abilities
{
    public enum eAbilityType { REROLL, EXTRA_DRAW, EXTRA_POINTS, CULL, REVIVE };

    public static List<Ability> abilities = new List<Ability>();

    public static void initAbilities()
    {
        abilities.Add(new Ability("ReRoll", "Allows player to reroll this and an additional die"));
        abilities.Add(new Ability("Scry+", "Allows player to draw additional die"));
        abilities.Add(new Ability("Score+", "Allows player to score additional points"));
        abilities.Add(new Ability("Send", "Allows player to send a die from within Stored back to Stalls"));
        abilities.Add(new Ability("Suppress", "Allows player to send defeated creature to Support"));
    }
}

public class Ability
{
    public enum eAbilityTrigger { CAST, SCORE, ATTACK, DEFEATED, PASSIVE };

    public string abilityName;
    public string abilityDesc;

    //public int abilityValue;

    public eAbilityTrigger abilityTrigger;

    public Ability(string n, string d)
    {
        abilityName = n;
        abilityDesc = d;
    }
}