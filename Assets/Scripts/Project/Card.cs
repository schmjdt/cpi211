using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[ExecuteInEditMode]
public class Card : MonoBehaviour {

	public RawImage CardUI;
    public Sides.sideValue[] sides = new Sides.sideValue[6];

    int numberSides;
    public int totalDice;
    public int diceOnCard;
    public List<Die> dice = new List<Die>();
    public string cardName;

    public Texture cardImage;
    public CardInfo cardTileInfo;

    // Use this for initialization 
    void Start () {
        //sides = new Side[dieSize]; 
        Debug.Log("Card Start()");
        numberSides = sides.Length;
        
        buildMesh(GameLogic.instance.cardLayout.mesh);
    }

    // Update is called once per frame
    void Update () {
	}

    public void buildMesh(Mesh mesh)
    {
        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        MeshCollider mesh_collider = GetComponent<MeshCollider>();

        mesh_filter.mesh = mesh;
        mesh_collider.sharedMesh = mesh;

        createTexture();

        setDice();
    }


    public void createTexture()
    {
        Debug.Log("Card Cost: " + cardTileInfo.cost);
        cardTileInfo.texInfo = TextureBuilder.ChopUpAllTextures(cardTileInfo.texInfo);
        Debug.Log("Card Cost: " + cardTileInfo.cost);
        //cardImage = GameLogic.instance.cardLayout.createTexture(cardTileInfo);
        cardImage = TextureBuilder.BuildTexture(cardTileInfo);
        GetComponent<Renderer>().material.mainTexture = cardImage;
    }
    

    public void createTextureEditor(Mesh mesh)
    {
        cardTileInfo.randomizeValues();
        buildMesh(mesh);
    }

    public void setTexture(Texture tex)
    {
        GetComponent<Renderer>().material.mainTexture = tex;
    }


    void OnMouseEnter()
    {
        //CardUI.texture = GetComponent<Renderer>().material.mainTexture;
        //GameLogic.instance.cardLayout.setImage(cardImage);
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Mouse down");
            GameLogic.instance.cardLayout.pnlCard.SetActive(true);
            GameLogic.instance.cardLayout.setImage(cardImage);
            GameLogic.instance.cardLayout.setText(cardName);
        }
        if (Input.GetMouseButton(1))
        {
            GameLogic.instance.cardLayout.pnlCard.SetActive(true);//CardUI.enabled = true;
        }
        else GameLogic.instance.cardLayout.pnlCard.SetActive(false); //CardUI.enabled = false;
    }

    void OnMouseExit()
    {
        //CardUI.enabled = false;
        GameLogic.instance.cardLayout.pnlCard.SetActive(false);
    }

    public void createDie()
    { 
        /* 
        Die newDie = new Die(this);
        dice.Add(newDie); 
        */
    }

    public void setDice()
    {
        // Get this Card's Zone
        Die[] d = this.transform.parent.parent.GetComponent<Zone>().getZoneDice();

        totalDice = d.Length;
        diceOnCard = totalDice;

        foreach (Die die in d)
        {
            die.card = this;
            die.setDieColor(cardTileInfo.outsideColor, cardTileInfo.insideColor);
        }
        // Set each Die to this Card's colors
    }

    public Side getSide(int i) {
        Debug.Log(i);
        return Sides.getSide((int)sides[i]);
    }

    public int getSideValue(int i, Side.valueTypes vt) {
        Debug.Log(i + "/" + vt);
        return getSide(i).getStat(vt);
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
    public int points;


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
        points = UnityEngine.Random.Range(0, 9);
    }

    public void randomSides()
    {
        for (int i = 0; i < 6; i++)
        {
            sideID[i] = UnityEngine.Random.Range(0, 9);
        }
    }
}

public static class Sides
{
    public enum sideValue { E_1, E_2, MINION };
    static Side[] sides = new Side[3];

    public static void createSides() {
        sides[0] = new Side(1, 0, 0, 0, 0);
        sides[1] = new Side(2, 0, 0, 0, 0);
        sides[2] = new Side(0, 1, 1, 2, 1);
    }
    public static Side getSide(int i) {
        return sides[i];
    }
}


public class Side
{
    public enum valueTypes { ENERGY, COST, ATTACK, DEFENSE, LEVEL };

    int energy, cost, attack, defense, level;

    public Side(int e, int c, int a, int d, int l) {
        energy  = e;
        cost    = c;
        attack  = a;
        defense = d;
        level   = 1;
    }
    public int getStat(valueTypes vt) {
        switch (vt) {
            case valueTypes.ENERGY:     return energy;
            case valueTypes.COST:       return cost;
            case valueTypes.ATTACK:     return attack;
            case valueTypes.DEFENSE:    return defense;
            case valueTypes.LEVEL:      return level;
            default:                    return 0;
        }
    }

    public int getEnergy()  { return energy;    }
    public int getCost()    { return cost;      }
    public int getAttack()  { return attack;    }
    public int getDefense() { return defense;   }
    public int getLevel()   { return level;     }
}