using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class PlayerLogic
{
    public enum eSteps { SCORE, SCRY, SPIN, SPEND, STRIKE, SECURE };
    public int playerTurn = 0;
    public eSteps currentStep;
    public int currentRound = 1;

    public int totalPlayers = 2;
    public Player[] players;


    // Add Market Vars:  # can buy at once (int), if can buy (bool)

    public string descStep = "";

    public Player currentPlayer() { return players[playerTurn]; }

    public void initPlayers()
    {
        if (GameState.newGame)
        {
            players = new Player[totalPlayers];
            for (int i = 0; i < totalPlayers; i++)
            {
                players[i] = new Player(i);

            }

            initStep();            
        }
    }

    public void cycleTurn()
    {
        playerTurn = nextPlayer(playerTurn);
        //CameraControl.instance.togglePlayerCam();

        if (playerTurn == 0)
            incRound();
    }

    public int nextPlayer(int p)
    {
        int nP = p + 1;
        if (nP == totalPlayers) nP = 0;
        return nP;
    }

    public void initRound() { currentRound = 1; }
    public void incRound() { currentRound++; }
    public void initStep() { currentStep = eSteps.SCORE; }
    public void incStep()
    {
        if (currentStep == eSteps.SECURE)
        {
            initStep();
            cycleTurn();
        }
        else
        {
            currentStep++;
        }
    }


    public void performStep()
    {
        switch (currentStep)
        {
            case eSteps.SCORE:
                descStep =
                    "Get Dice in Summoned\n" +
                    "Sum total of Die's Score value\n" +
                    "Move each Die to Stored";
                break;
            case eSteps.SCRY:
                descStep =
                    "Move 5 Die from Supply to Support\n" +
                    "If Supply = 0, move all Die from Stored to Supply\n" +
                    "If Supply = 0 and Store = 0, continue";
                break;
            case eSteps.SPIN:
                descStep =
                    "Roll all Die in Support\n" +
                    "Move all die to Servicable";
                break;
            case eSteps.SPEND:
                descStep =
                    "Total Energy on Die\n" +
                    "Buy from Market\n" +
                    "Summon creature\n" +
                    "Extra special things";
                break;
            case eSteps.STRIKE:
                descStep =
                    "Attack all players\n" +
                    "Other Players defend";
                break;
            case eSteps.SECURE:
                descStep =
                    "Move all Die from Servicable to Stored\n" +
                    "Move all Die from Spent to Store";
                break;
        }
    }
};


[System.Serializable]
public struct GameState
{
    public static bool newGame;

};

[System.Serializable]
public class GameLayout
{
    public GameArea[] gameAreas;

    public Transform scorePositions;
    public Transform[] positScores;

    public void initLayout()
    {
        // Init Areas/Zones
        foreach (GameArea item in gameAreas)
        {
            item.initZones();
        }

        // Init Scoreboard
        positScores = scorePositions.GetComponentsInChildren<Transform>();
    }

    public void checkZoneColor()
    {
        foreach (GameArea item in gameAreas)
        {
            item.checkZoneColor();
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
    
    public void gatherDice()
    {
        foreach (GameArea area in gameAreas)
            foreach (GameZone zone in area.areaZones)
                zone.gatherDice();
    }
        
    public Die[] getDice(int a, int z)
    {
        gameAreas[a].areaZones[z].gatherDice();
        return gameAreas[a].getDice(z);
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
        string obj = a + "/";
        if (obj == "/") obj = "";
        obj = z + "/Dice";
        Debug.Log(obj);
        return GameObject.Find(obj);
    }
}

[System.Serializable]
public class RollValues
{
    public float[] tV;
    public float[] f1V;
    public float[] f2V;
    public float[] lV;
}


public class GameLogic : MonoBehaviour
{
    public static GameLogic instance;

    public GameObject card;
    public Die dillydoe;

    public GameObject draggedObject = null;
    
    public PlayerLogic playerLogic;
    public GameLayout gameLayout;
    public CardLayout cardLayout;

    public RollValues rollValues;

    // Card Mesh
    /*
    [Header("Card Layout")]
    public int size_x;
    public int size_z;
	public Mesh mesh;
    public RawImage imgCard;
    public Text txtCard;
    */

    void Awake()
    {
        Debug.Log("Game Awake()");
        instance = this;

        Sides.createSides();
        cardLayout.buildMesh();
        //gameLayout.gatherDice();
    }

    void Start()
    {
        gameLayout.initLayout();
        GameState.newGame = true;
        {
            Debug.Log("Game Started");
            playerLogic.initPlayers();
            playerLogic.performStep();

            moveDice("zoneMB", "areaPlayer1/zoneSupply", 8, false);
            moveDice("zoneMB", "areaPlayer2/zoneSupply", 8, false);
        }
        GameState.newGame = false;
    }

    void Update()
    {
        //if (Input.GetKeyUp("r"))        rollDice();
        if (Input.GetKeyUp("t"))   playerLogic.cycleTurn();
        else if (Input.GetKeyUp("p"))   printCards();
        else if (Input.GetKeyUp("m")) adminMoveDie();
        else if (Input.GetKeyUp("i")) adminIncScore();
        
        
        else if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log("Name = " + hit.collider.name);
                try
                {
                    if (hit.collider.transform.parent.parent.name == "area" + playerLogic.currentPlayer().getPlayerName())
                    {
                        string colParName = hit.collider.transform.parent.name;
                        switch (colParName)
                        {
                            case "zoneSupply":
                                drawDice(5);
                                break;
                            case "zoneRoll":
                                moveDice(getCurrentName(colParName),
                                         getCurrentName("zoneServicable"));
                                break;
                        }
                    }
                }
                catch(Exception e)
                {
                    Debug.Log("GameLogic Update err:/n" + e);
                }


            }
        }



        GameObject.Find("gameStateStats").GetComponent<Text>().text =
                "Player: " + playerLogic.currentPlayer().getPlayerName() + "\n" +
                "Step: " + playerLogic.currentStep + "\n" +
                "Round: " + playerLogic.currentRound + "\n" +
                "Desc: " + playerLogic.descStep;

    }

    public string getCurrentName(string z)
    {
        return "area" + playerLogic.currentPlayer().getPlayerName() + "/" + z;
    }

    public Card[] getCards()
    {
        //return zones[0].GetComponentsInChildren<Card>();
        return null;
    }

    public void printCards()
    {
        foreach (Card card in getCards())
        {
            Debug.Log("Card: " + card);
        }
    }

    public void instantiateDie(Transform t)
    {
        // 'zoneCard3' would be current cards: transform.parent.parent.name
        //		or look go through areaMarket's list of zones looking for <Card>, in which already of Zone's Name
        //Transform pA = GameObject.Find("areaMarket/zoneM1/Dice").transform;
        //Transform dS = pA.gameObject.Find("areaPlayer1/zoneSummoned/Dice").transform;
        
        Die d = (Die)Instantiate(dillydoe, t.position, Quaternion.identity);
        d.transform.SetParent(t);
        d.transform.position = t.position;
        //d.transform.localPosition = dS.position;
        //d.transform.localScale = new Vector3(.5f, 1.25f, .5f);
        //d.transform.localScale = die.transform.localScale;

    }

    public void rollDice()
    {
        Debug.Log(playerLogic.currentPlayer().getPlayerName() + " Rolling Dice");
        foreach (Die die in gameLayout.getDiceInZone(playerLogic.currentPlayer().getPlayerName()))
        {
            die.rollDie();
        }
    }

    public void drawDice(int amount)
    {
        int count = amount;

        Transform support = GameObject.Find(getCurrentName("zoneSupport/zoneOutline")).transform;
        string supply = getCurrentName("zoneSupply");
        string stored = getCurrentName("zoneStored");

        Die[] h = gameLayout.getDiceInZone(supply);
        Die[] d = gameLayout.getDiceInZone(stored);

        int iDraw;
        

        if (h.Length == 0 && d.Length != 0)
        {
            moveDice(stored, supply, false);
            h = gameLayout.getDiceInZone(supply);
            d = new Die[0];
        }

        while (count != 0 && h.Length != 0)
        {
            iDraw = UnityEngine.Random.Range(0, h.Length);

            h[iDraw].moveDie(support);
            h[iDraw].toggleVisibility(true);

            count--;
            h = gameLayout.getDiceInZone(supply);

            if (count > 0 && h.Length == 0 && d.Length > 0)
            {
                moveDice(stored, supply, false);
                h = gameLayout.getDiceInZone(supply);
                d = new Die[0];
            }
        }

        Debug.Log("Moved " + (amount - count) + " dice !");
    }


    public void moveDice(string f, string t) { moveDice(f, t, 0, true); }
    public void moveDice(string f, string t, int amt) { moveDice(f, t, amt, true); }
    public void moveDice(string f, string t, bool b) { moveDice(f, t, 0, b); }
    public void moveDice(string f, string t, int amt, bool b)
    {
        Die[] dice = gameLayout.getDiceInZone(f);

        int count = amt;
        if (count <= 0 || count > dice.Length) count = dice.Length;
                
        for (int i = 0; i < count; i++)
        {
            dice[i].moveDie(gameLayout.getZoneDiceParent(t).transform);
            dice[i].toggleVisibility(b);
        }        
    }


    public Zone getDraggedPlaceholder()
    {
        Draggable3D_Plane drag = draggedObject.GetComponent<Draggable3D_Plane>();
        if (!drag) return null;
        return drag.placeHolderParent.GetComponent<Zone>();
        //return drag.getParentZone();
    }

    public bool isValidDrop(Zone z)
    {
        bool isValid = false;
        // know draggedObject
        //      know original zone/area
        //      if is a Die, know side
        // know current step

        if (!draggedObject) return false;
        Draggable3D_Plane drag = draggedObject.GetComponent<Draggable3D_Plane>();
        if (!drag) return false;

        //Die d = draggedObject.GetComponent<Die>();
        //if (!d) return false;
        
        switch (playerLogic.currentStep)
        {
            case PlayerLogic.eSteps.SCORE:
                
                if ((playerLogic.playerTurn == 0 && 
                     z.typeOfSlot == Zone.Slot.P1_STORED && drag.getParentZone().typeOfSlot == Zone.Slot.P1_SUMMONED) ||
                    (playerLogic.playerTurn == 1 && 
                     z.typeOfSlot == Zone.Slot.P2_STORED && drag.getParentZone().typeOfSlot == Zone.Slot.P2_SUMMONED))
                    isValid = true;
                break;
            case PlayerLogic.eSteps.SCRY:
                isValid = false;
                break;
            case PlayerLogic.eSteps.SPIN:
                isValid = false;
                break;
            case PlayerLogic.eSteps.SPEND:
                isValid = false;
                if (drag.getParentArea().name == "areaMarket" && 
                    ((playerLogic.playerTurn == 0 && z.typeOfSlot == Zone.Slot.P1_STORED) ||
                     (playerLogic.playerTurn == 1 && z.typeOfSlot == Zone.Slot.P2_STORED)))
                    isValid = true;
                /*
                if ((draggedObject) == Stall &&
                    (z.id == 6 + (currentPlayerID * 10)))
                        return true;
                */
                break;
            case PlayerLogic.eSteps.STRIKE:
                isValid = false;
                break;
            case PlayerLogic.eSteps.SECURE:
                isValid = false;
                break;


        }

        // houseRules
        isValid = true;



        if (drag.getParentZone() == z) isValid = true;
        return isValid;
    }

    public bool isValidDrag(Draggable3D_Plane drag)
    {
        try {
            if (drag.transform.parent.parent.name == "zoneRoll")
            {
                moveDice(getCurrentName("zoneRoll"),
                         getCurrentName("zoneServicable"));
                return false;
            }
            if (drag.transform.parent.parent.name == "zoneSupply")
            {
                //drawDice(5);
                return false;
            }

            switch (playerLogic.currentStep)
            {
                case PlayerLogic.eSteps.SCORE:
                    break;
                case PlayerLogic.eSteps.SCRY:
                    break;
                case PlayerLogic.eSteps.SPIN:
                    break;
                case PlayerLogic.eSteps.SPEND:
                    break;
                case PlayerLogic.eSteps.STRIKE:
                    break;
                case PlayerLogic.eSteps.SECURE:
                    break;
            }
            draggedObject = drag.gameObject;
        }
        catch (Exception e)
        {
            Debug.Log("isValidDrag err:/n" + e);
        }
        return true;
    }

    public bool checkStep(PlayerLogic.eSteps eS) { return playerLogic.currentStep == eS;  }



    public void dieDropped()
    {
        gameLayout.checkZoneColor();


        // Special Zone Cases (If drop is valid)
        switch (draggedObject.transform.parent.parent.name)
        {
            case "zoneSupply":
                //draggedObject
                draggedObject.GetComponent<Die>().toggleVisibility(false);
                break;
        }


        draggedObject = null;
    }
    


    // ========= ------ ~~~ PLAYER _ CONTROL ~~~ ------ =========
    /*
    void createPlayers()
    {
        playerLogic.players = new Player[playerLogic.totalPlayers]; for (int i = 0; i < playerLogic.totalPlayers; i++)
        {
            playerLogic.players[i] = new Player(i);
        }
    }
    */

    public void currentPlayerAttack()
    {
        playerLogic.currentPlayer().attack();
    }

    public void currentPlayerDefend(int dmg)
    {
        playerLogic.currentPlayer().defend(dmg);
    }
    // ----------------------------------------------------------- }


    // Admin

    public void adminCloseNGWindow()
    {
        GameObject.Find("NewGame").SetActive(false);
    }

    public void adminIncScore()
    {
        //int p = UnityEngine.Random.Range(0, 2);
        
        int mod;
        int winCondition = 24;

        for (int p = 0; p < 2; p++)
        {
            mod = -1;
            if (p == 1)
                mod = 1;

            //int s = UnityEngine.Random.Range(1, 4);
            int s = 1;
        if (playerLogic.players[p].score == winCondition)
            playerLogic.players[p].score = 0;
        else
            playerLogic.players[p].score += s;

        if (playerLogic.players[p].score >= winCondition) {
            playerLogic.players[p].score = winCondition;
            Debug.Log(playerLogic.players[p].getPlayerName() + " Wins!");
        }

        Transform d = GameObject.Find("token" + playerLogic.players[p].getPlayerName()).transform;
            d.localPosition = new Vector3(gameLayout.positScores[playerLogic.players[p].score + 1].localPosition.x + mod * (.004f + UnityEngine.Random.Range(0f, .004f)),
                                     d.localPosition.y,
                                     gameLayout.positScores[playerLogic.players[p].score + 1].localPosition.z + mod * (.004f + UnityEngine.Random.Range(0f, .004f)));
        }
    }

    public void adminIncStep() { playerLogic.incStep(); playerLogic.performStep(); }

    public void adminMoveDie()
    {
        //Transform dT;
        Transform zT = GameObject.Find("areaPlayer1/zoneSupport").transform;
        Transform dT = GameObject.Find("areaPlayer1/zoneSupport").transform;

        switch (playerLogic.currentStep)
        {
            case PlayerLogic.eSteps.SCORE:
                dT = GameObject.Find("areaPlayer1/zoneSupport").transform;
                zT = GameObject.Find("areaPlayer1/zoneServicable").transform;
                break;
            case PlayerLogic.eSteps.SCRY:
                dT = GameObject.Find("areaPlayer1/zoneServicable").transform;
                zT = GameObject.Find("areaPlayer1/zoneSummoned").transform;
                break;
            case PlayerLogic.eSteps.SPIN:
                dT = GameObject.Find("areaPlayer1/zoneSummoned").transform;
                zT = GameObject.Find("areaPlayer1/zoneSpent").transform;
                break;
            case PlayerLogic.eSteps.SPEND:
                dT = GameObject.Find("areaPlayer1/zoneSpent").transform;
                zT = GameObject.Find("areaPlayer1/zoneStored").transform;
                break;
            case PlayerLogic.eSteps.STRIKE:
                dT = GameObject.Find("areaPlayer1/zoneStored").transform;
                zT = GameObject.Find("areaPlayer1/zoneSupport").transform;
                break;
            case PlayerLogic.eSteps.SECURE:
                dT = GameObject.Find("areaPlayer1/zoneSupport").transform;
                zT = GameObject.Find("areaPlayer1/zoneSupport").transform;
                break;
        }

        foreach (Die item in dT.GetComponentsInChildren<Die>())
        {
            /*
            item.GetComponent<Draggable3D_Plane>().transform.SetParent(zT.Find("Dice"));
            float offsetX = UnityEngine.Random.Range(-.05f, .05f);
            float offsetY = UnityEngine.Random.Range(-.05f, .05f);
            item.transform.localPosition = new Vector3(offsetX, offsetY, -.01f);
            */
            item.moveDie(zT.GetComponentInChildren<Zone>());
        }

        adminIncStep();
    }

    public void adminMoveDie2()
    {
        Transform zT = GameObject.Find("areaPlayer1/zoneStored").transform; ;
        foreach (Die item in GameObject.Find("areaMarket/zoneCard1").GetComponent<Zone>().getZoneDice())
        {
            item.GetComponent<Draggable3D_Plane>().transform.SetParent(zT.Find("Dice"));
            float offsetX = UnityEngine.Random.Range(-.05f, .05f);
            float offsetY = UnityEngine.Random.Range(-.05f, .05f);
            item.transform.localPosition = new Vector3(offsetX, offsetY, -.01f);
        }
        
    }
}


[System.Serializable]
public class GameArea
{
    public enum eAreaType { PLAYER, MARKET, OTHER };

    public GameObject area;
    public GameZone[] areaZones;
    public string areaName;
    public eAreaType areaType;
    

    public string getName() { return areaName; }

    public GameZone getZone(string s)
    {
        for (int i = 0; i < areaZones.Length; i++)
        {
            if (s == areaZones[i].getName()) return areaZones[i];
        }
        return null;
    }

    public Die[] getDice(int z)
    {
        return areaZones[z].dice;
    }

    public void checkZoneColor()
    {
        foreach (GameZone item in areaZones)
        {
            item.checkZoneColor();
        }
    }
    /*
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
    */

    public void initZones()
    {
        createZones();
    }

    public void createZones()
    {
        areaName = area.name;
        Zone[] zones = area.GetComponentsInChildren<Zone>();
        areaZones = new GameZone[zones.Length];

        for (int i = 0; i < zones.Length; i++)
        {
            areaZones[i] = new GameZone(zones[i]);
        }
    }
}

[System.Serializable]
public class GameZone
{
    public Die[] dice;
    public Zone zone;
    public string zoneName;

    public GameZone(Zone z)
    {
        this.zone = z;
        this.zoneName = z.transform.parent.name;
        checkIfCard();
    }

    public string getName() { return zoneName; }

    public void gatherDice()
    {
        this.dice = this.zone.transform.parent.GetComponentsInChildren<Die>();
    }

    public void checkZoneColor()
    {
        this.zone.checkOwnColor = true;
    }

    public void checkIfCard()
    {
        Card card = this.zone.transform.parent.GetComponentInChildren<Card>();
        if (card)
        {
            card.initCard();

            Transform cT = card.transform.parent.Find("Dice");
            for (int i = 0; i < card.totalDice; i++)
            {
                GameLogic.instance.instantiateDie(cT);
            }

            gatherDice();

            foreach (Die die in dice)
            {
                die.card = card;
                die.colorOutside = card.cardInfo.outsideColor;
                die.colorInside = card.cardInfo.insideColor;
                //die.setDieColor(card.cardInfo.outsideColor, card.cardInfo.insideColor);
            }
        } else
        {
            gatherDice();
        }
    }
}


public class MeshLayout
{
    public int size_x;
    public int size_z;
    public Mesh mesh;

    public void buildMesh()
    {
        mesh = MeshBuilder.BuildMesh(new Mesh(), size_x, size_z);
    }

    public int[] getMeshSize()
    {
        int[] i = { size_x, size_z };
        return i;
    }
}



[System.Serializable]
public class CardLayout : MeshLayout
{
    //public RawImage imgCard;
    //public Text txtCard;
    public GameObject pnlCard;
    
    public List<Texture2D> texCards = new List<Texture2D>();   
    
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


[System.Serializable]
public class TextureInfo
{
    public Texture2D texSheet;
    public int resolution;
    public Color[][] texColors;

    public void chopTexture()
    {
        texColors = TextureBuilder.ChopUpTiles(this);
    }
}



public struct TextureBuilder
{
    public static Color[][] ChopUpTiles(TextureInfo texInfo)
    {
        Texture2D tex = texInfo.texSheet;
        int tileResolution = texInfo.resolution;

        int numTilesPerRow = tex.width / tileResolution;
        int numRows = tex.height / tileResolution;

        Color[][] tiles = new Color[numTilesPerRow * numRows][];

        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numTilesPerRow; x++)
            {
                tiles[y * numTilesPerRow + x] = tex.GetPixels(x * tileResolution,
                                                              y * tileResolution,
                                                              tileResolution,
                                                              tileResolution);
            }
        }

        return tiles;
    }

    public static TextureInfo[] ChopUpAllTextures(TextureInfo[] texInfos)
    {
        for (int i = 0; i < texInfos.Length; i++)
        {
            texInfos[i].chopTexture();
        }
        return texInfos;
    }


    
    /*
    public static Texture2D BuildTexture(TextureInfo[] texInfos)
    {
        return BuildTexture(texInfos, GameLogic.instance.gameLayout.scoreArea.getMeshSize(), null);
    }
    */

    public static Texture2D BuildTexture(CardInfo cardInfo)
    {
        return BuildTexture(cardInfo.texInfo, GameLogic.instance.cardLayout.getMeshSize(), cardInfo);
    }

    public static Texture2D BuildTexture(TextureInfo[] texInfos, 
                                              int[] size_mesh,
                                              CardInfo cardInfo)
    {
        
        //TextureInfo texInfo = texInfos[0];  
        int size_x = size_mesh[0];
        int size_z = size_mesh[1];
        
        // HARD: Using the first textures resolution (for now) assuming all will be same (for now)
        int tileResolution = texInfos[0].resolution;
        int texWidth = size_x * tileResolution;
        int texHeight = size_z * tileResolution;
        Texture2D texture = new Texture2D(texWidth, texHeight);
        
        for (int y = 0; y < size_z; y++)
        {
            for (int x = 0; x < size_x; x++)
            {
                // Creating the color array, p
                // Color[] p = texInfo.texColors[0];

                Color[] p;
                if (cardInfo != null)
                    p = getCardColors(x, y, size_x, size_z, texInfos, cardInfo);
                else
                    p = getColors(x, y, size_x, size_z, texInfos);

                // Setting the texture's pixels using the color array, p
                texture.SetPixels(x * tileResolution, 
                                  y * tileResolution, 
                                  tileResolution, 
                                  tileResolution, 
                                  p);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        return texture;
    }

    // FUTURE USE: Will be used for customization
    public static Color[] getTexColors()
    {
        Color[] p = { new Color(0f, 0f, 0f, 1f) };

        return p;
    }

    static Color[] getColors(int x, int y, int size_x, int size_z, TextureInfo[] texInfos)
    {
        Color[] p;
        if (x == 0 && y == 0)
            p = texInfos[0].texColors[0];
        else if (x == 3 && y == 2)
            p = texInfos[0].texColors[21];
        else
            p = texInfos[0].texColors[11];
        return p;
    }

    static Color[] getCardColors(int x, int y, int size_x, int size_z, TextureInfo[] texInfos, CardInfo cardInfo)
    {
        Color[] p;
        // HARD: Using magic values for now knowing the size of textures
        //      NEED: x, z, size_x, size_z, texInfos, cardInfo
        // ::Texture [0]::
        // Cost:
        if (x == 1 && y == size_z - 2)
        {
            //p = texInfos[0].texColors[Math.Abs(9 - cardInfo.cost)];
            p = texInfos[0].texColors[cardInfo.cost];
        }
        // Points:
        else if (x == size_x - 2 && y == size_z - 2)
        {
            p = texInfos[0].texColors[cardInfo.points];
        }
        // ::Texture [1]::
        // Sides:
        else if (y == 1 && (x > 0 && x < size_x - 1))
        {
            //p = texInfos[1].texColors[Math.Abs(9 - cardInfo.sideID[Math.Abs(5 - (x - 1))])];
            p = texInfos[1].texColors[cardInfo.sideID[x - 1]];
        }
        // ::Solid Colors::
        // outsideColor:
        else if (x == 0 || y == 0 || y == size_z - 1 || x == size_x - 1)
        {
            p = new Color[texInfos[0].texColors[0].Length];
            for (int i = 0; i < texInfos[0].texColors[0].Length; i++)
            {
                //p[i] = new Color(cardInfo.outsideColorRGB[0], cardInfo.outsideColorRGB[1], cardInfo.outsideColorRGB[2], 1);
                p[i] = cardInfo.outsideColor;
            }
        }
        // insideColor:
        else
        {
            p = new Color[texInfos[0].texColors[0].Length];
            for (int i = 0; i < texInfos[0].texColors[0].Length; i++)
            {
                //p[i] = new Color(cardInfo.insideColorRGB[0], cardInfo.insideColorRGB[1], cardInfo.insideColorRGB[2], 1);
                p[i] = cardInfo.insideColor;
            }
        }

        return p;
    }
}


public struct MeshBuilder
{

    public static Mesh BuildMesh(Mesh mesh, int size_x, int size_z)
    {
        int numTiles = size_x * size_z;
        int numTris = numTiles * 2;

        int vsize_x = size_x + 1;
        int vsize_z = size_z + 1;
        int numVerts = vsize_x * vsize_z;

        float tileSize = 1.0f;
        

        // Generate the mesh data
        Vector3[] vertices = new Vector3[numVerts];
        Vector3[] normals = new Vector3[numVerts];
        Vector2[] uv = new Vector2[numVerts];

        int[] triangles = new int[numTris * 3];

        int x, z;
        for (z = 0; z < vsize_z; z++)
        {
            for (x = 0; x < vsize_x; x++)
            {
                vertices[z * vsize_x + x] = new Vector3(x * tileSize, 0, -z * tileSize);
                normals[z * vsize_x + x] = Vector3.up;
                uv[z * vsize_x + x] = new Vector2((float)x / size_x, 1f - (float)z / size_z);
            }
        }
        Debug.Log("Done Verts!");

        for (z = 0; z < size_z; z++)
        {
            for (x = 0; x < size_x; x++)
            {
                int squareIndex = z * size_x + x;
                int triOffset = squareIndex * 6;
                triangles[triOffset + 0] = z * vsize_x + x + 0;
                triangles[triOffset + 2] = z * vsize_x + x + vsize_x + 0;
                triangles[triOffset + 1] = z * vsize_x + x + vsize_x + 1;

                triangles[triOffset + 3] = z * vsize_x + x + 0;
                triangles[triOffset + 5] = z * vsize_x + x + vsize_x + 1;
                triangles[triOffset + 4] = z * vsize_x + x + 1;
            }
        }

        Debug.Log("Done Triangles!");

        // Create a new Mesh and populate with the data
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv; 

        Debug.Log("Done Mesh!");

        return mesh;
    }
}