using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class GameLayout
{
    public GameArea[] gameAreas;

    public Transform scorePositions;
    public Transform[] positScores;

    public void initLayout()
    {
        GameObject[] areas = GameObject.FindGameObjectsWithTag("AreaHolder");

        gameAreas = new GameArea[areas.Length];

        for (int i = 0; i < gameAreas.Length; i++)
        {
            gameAreas[i] = new GameArea(areas[i].transform);
        }
        /*
        foreach (GameObject item in areas)
        {
            item.initZones();
        }
        */
        // Init Scoreboard
        positScores = scorePositions.GetComponentsInChildren<Transform>();
    }

    public Zone clickedZone()
    {
        //bool b = false;
        Zone z = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            z = hit.collider.transform.GetComponent<Zone>();
        }

        return z;
        //return b;
    }

    public string checkAreaClick()
    {
        Zone z = clickedZone();
        if (z)
            return z.zoneName;
        return null;
    }

    public void checkZoneColor()
    {
        foreach (GameArea gA in gameAreas)
        {
            foreach (GameZone gZ in gA.areaZones)
            {
                gZ.checkZoneColor();
            }
        }
    }

    public GameArea getArea(string s)
    {
        for (int i = 0; i < gameAreas.Length; i++)
        {
            if (s == gameAreas[i].getName()) return gameAreas[i];
        }
        return null;
    }

    public Zone getZone(string s)
    {
        if (!s.Contains("/"))
            s = GameLogic.instance.getCurrentZoneS(s);

        string[] ss = s.Split('/');

        GameArea gA = getArea(ss[0]);

        foreach (GameZone zone in gA.areaZones)
        {
            if (zone.zoneName == ss[1])
                return zone.zone;
        }
        return null;
    }

    public void gatherDice()
    {
        foreach (GameArea area in gameAreas)
            foreach (GameZone zone in area.areaZones)
                zone.gatherDice();
    }

    public Die[] getDiceInZone(int a, int z)
    {
        // Update to the current dice in Zone
        gameAreas[a].areaZones[z].gatherDice();
        // Return the current dice in Zone
        return gameAreas[a].areaZones[z].dice;
    }

    public Die[] getDiceInZone(Zone z)
    {
        return getDiceInZone(z.name);
    }

    public Die[] getDiceInZone(string zoneName)
    {
        return getDiceInZone("", zoneName);
    }

    public Die[] getDiceInZone(string a, string z)
    {
        string obj = a + "/";
        if (obj == "/") obj = "";
        obj = z;
        Die[] dice = null;
        GameObject zone = GameObject.Find(obj);
        if (zone != null)
        {
            dice = zone.GetComponentsInChildren<Die>();
        }
        return dice;
    }

    public GameObject getZoneDiceParent(string z)
    {
        return getZoneDiceParent("", z);
    }

    public GameObject getZoneDiceParent(string a, string z)
    {
        GameArea gA = getArea(a);
        if (gA != null)
        {
            GameZone gZ = gA.getZone(z);
            if (gZ != null)
                return gZ.diceHolder.gameObject;
        }
        return null;
        
        /*
        string obj = a + "/";
        if (obj == "/") obj = "";
        obj = z + "/Dice";
        return GameObject.Find(obj);
        */
    }
}



[System.Serializable]
public class GameArea
{
    public enum eAreaType { PLAYER, MARKET, OTHER };

    public string areaName;
    public Transform areaHolder;
    public List<GameZone> areaZones = new List<GameZone>();
    public eAreaType areaType;

    public GameArea(Transform t)
    {
        areaHolder = t;
        areaName = t.name;

        switch (areaName.Substring(4, 1))
        {
            case "P":
                areaType = eAreaType.PLAYER;
                break;
            case "M":
                areaType = eAreaType.MARKET;
                break;
            default:
                areaType = eAreaType.OTHER;
                break;
        }

        createZones();
    }

    public string getName() { return areaName; }

    public GameZone getZone(string s)
    {
        for (int i = 0; i < areaZones.Count; i++)
        {
            if (s == areaZones[i].getName()) return areaZones[i];
        }
        return null;
    }
    
    public void initZones()
    {
        areaName = areaHolder.name;
        createZones();
    }

    public void createZones()
    {
        Transform[] zones = LayoutLogic.getSpecificTransformsTag("ZoneHolder", areaHolder);
        //GameObject[] zones = GameObject.FindGameObjectsWithTag("ZoneHolder");
        
        for (int i = 0; i < zones.Length; i++)
        {
            areaZones.Add(new GameZone(zones[i]));
        }
    }

// createArea via code (instead of Unity drag-drop)
#if false
    void createArea()
    {
        string[] zones = { };
        switch (areaType)
        {
            case eAreaType.PLAYER:
                areaZones = new GameZone[6];
                zones = "zoneSupport,zoneServicable,zoneSummoned,zoneSpent,zoneStored,zoneSupply".Split(',');
                break;
            case eAreaType.MARKET:
                areaZones = new GameZone[3];
                zones = "zoneCard1,zoneCard2,zoneCard3".Split(',');
                break;
        }

        for (int i = 0; i < areaZones.Length; i++)
        {
            areaZones[i] = new GameZone(zones[i]);
        }
    }
#endif

}

[System.Serializable]
public class GameZone
{
    public string zoneName;
    public Zone zone;
    public Die[] dice;
    public Card[] cards;
    //public Token[] tokens;

    public Transform zoneHolder;
    public Transform diceHolder;
    public Transform cardHolder;
    public Transform tokenHolder;

    public GameZone(Transform z)
    {
        this.zone = z.GetComponentInChildren<Zone>();
        this.zoneName = z.name;

        zoneHolder = z;
        
        diceHolder = LayoutLogic.getSpecificTransformTag("DieHolder", z);
        cardHolder = LayoutLogic.getSpecificTransformTag("CardHolder", z);
        tokenHolder = LayoutLogic.getSpecificTransformTag("TokenHolder", z);

        //diceHolder = z.Find("Dice");
        //cardHolder = z.Find("Cards");
        //tokenHolder = z.Find("Tokens");

        this.zone.zoneHolder = z;
        this.zone.diceHolder = diceHolder;

        checkIfCard();
    }

    public string getName() { return zoneName; }
    public Transform getDiceT() { return diceHolder; }
    public Transform getCardT() { return cardHolder; }

    public Card getCard() { return getCard(0); }
    public Card getCard(int i) { return cards[i]; }
    //public Transform getTokenT() { return tokenHolder; }
    

    public void gatherDice()
    {
        this.dice = diceHolder.GetComponentsInChildren<Die>();
    }

    public void checkZoneColor()
    {
        this.zone.checkOwnColor = true;
    }

    public void checkIfCard()
    {
        if (cardHolder == null || cardHolder.childCount == 0)
            return;

        Card card = cardHolder.GetComponentInChildren<Card>();
        if (card)
        {
            card.initCard();
            
            for (int i = 0; i < card.totalDice; i++)
            {
                GameLogic.instance.instantiateDie(diceHolder);
            }

            gatherDice();

            foreach (Die die in dice)
            {
                die.setCard(card);
            }
        }
        else
        {
            gatherDice();
        }
    }
}



[System.Serializable]
public class CardLayout : MeshLayout
{
    //public RawImage imgCard;
    //public Text txtCard;
    public GameObject pnlCard;

    public List<Texture2D> texCards = new List<Texture2D>();


    public void toggleImage(bool b)
    {
        toggleImage(b, null, "");
    }

    public void toggleImage(bool b, Texture i, string s)
    {
        pnlCard.SetActive(b);
        if (b)
        {
            setImage(i);
            setText(s);
        }
    }

    // HARD: Assums there is a panel (gameobject) with a rawimage and text for the card
    public void setImage(Texture img)
    {
        pnlCard.GetComponentInChildren<RawImage>().texture = img;
    }

    public void setText(string s)
    {
        pnlCard.GetComponentInChildren<Text>().text = s;
    }
}

public struct LayoutLogic
{

    public static List<gameTag> gameObjects = new List<gameTag>();

    public struct gameTag
    {
        public string tagName;
        public GameObject[] tagObjects;

        
        public gameTag(string tag) {
            tagName = tag;
            tagObjects = GameObject.FindGameObjectsWithTag(tag);
        }

        public void createObjects(string tag)
        {
            tagObjects = GameObject.FindGameObjectsWithTag(tag);
        }
    }

    static GameObject[] newTagObject(string tag)
    {
        gameTag gT = new gameTag(tag);
        gameObjects.Add(gT);
        return gT.tagObjects;
    }

    static GameObject[] checkTagged(string tag)
    {
        foreach (gameTag item in gameObjects)
        {
            if (item.tagName == tag)
                return item.tagObjects;
        }
        return null;
    }


    public static Transform getSpecificTransformTag(string tag, Transform p)
    {
        GameObject[] dT = checkTagged(tag);
        if (dT == null)
            dT = newTagObject(tag);
        
        foreach (GameObject item in dT)
        {
            if (item.transform.IsChildOf(p))
                return item.transform;
        }
        return null;
        
    }

    public static Transform[] getSpecificTransformsTag(string tag, Transform p)
    {
        List<Transform> t = new List<Transform>();
        GameObject[] dT = checkTagged(tag);
        if (dT == null)
            dT = newTagObject(tag);

        foreach (GameObject item in dT)
        {
            if (item.transform.IsChildOf(p))
                t.Add(item.transform);
        }

        return t.ToArray();
    }
}