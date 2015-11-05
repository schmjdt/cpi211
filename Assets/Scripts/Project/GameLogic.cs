using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public struct GameState
{
    public static bool newGame;

    public static int cRound = 1;

};

public class GameLogic : MonoBehaviour
{
    public static GameLogic instance;

    public Card cardPrefab;
    public Die diePrefab;

    public GameObject draggedObject = null;
    
    
    public PlayerLogic playerLogic;
    public StepLogic stepLogic;

    public GameLayout gameLayout;
    public CardLayout cardLayout;

    public RollValues rollValues;

    void Awake()
    {
        Debug.Log("Game Awake()");
        instance = this;

        Sides.createSides();
        cardLayout.buildMesh();
        stepLogic.initSteps();
        //gameLayout.gatherDice();

    }

    void Start()
    {
        gameLayout.initLayout();
        GameState.newGame = true;
        {
            playerLogic.initPlayers();
            Debug.Log("Game Started");
            //stepLogic.performStep();

            moveDice("zoneMB", "areaPlayer1/zoneSupply", 8, false);
            moveDice("zoneMB", "areaPlayer2/zoneSupply", 8, false);
        }
        GameState.newGame = false;
        GameObject.Find("gameStateStats").GetComponent<Text>().text = getGameDesc();
    }

    void Update()
    {
        //if (Input.GetKeyUp("r"))        rollDice();
        if (Input.GetKeyUp("t"))   playerLogic.cyclePlayer();
        else if (Input.GetKeyUp("p"))   printCards();
        else if (Input.GetKeyUp("m")) adminMoveDie();
        else if (Input.GetKeyUp("i")) adminIncScore();
        else if (Input.GetKey("d")) DieLogic.drawRollLine();

        else if (Input.GetMouseButtonDown(0))
        {
            string z = gameLayout.checkAreaClick();
            switch (z)
            {
                case "zoneSupply":
                    drawDice(5);
                    break;
                case "zoneRoll":
                    moveDice(getCurrentZoneS(z), getCurrentZoneS("zoneServicable"));
                    break;
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            //if (Input.GetKeyDown("r"))
            //{
            Die die = DieLogic.clickedDie();

            if (die)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    GameLogic.instance.rollSimilarDice(die);
                }
                else
                {
                    SoundControl.instance.setAudio("dice", "rollS");
                    DieLogic.rollDie(die);
                }
            }
            //}
        }


    }

    public void updateTurn()
    {
        if (stepLogic.cycleStep())
            if (playerLogic.cyclePlayer())
                GameState.cRound++;
    }

    public Zone getCurrentZone(string z)
    {
        return null;
    }
    public string getCurrentZoneS(string z)  { return getCurrentAreaS() + "/" + z;                          }
    public string getCurrentAreaS()          { return "area" + playerLogic.currentPlayer().getPlayerName(); }

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
        
        Die d = (Die)Instantiate(diePrefab, t.position, Quaternion.identity);
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
            DieLogic.rollDie(die);
        }
    }

    //     IEnumerator
    public void drawDice(int amount)
    {
        int count = amount;

        Transform supportT = GameObject.Find(getCurrentZoneS("zoneSupport/zoneOutline")).transform;
        string support = getCurrentZoneS("zoneSupport");
        string supply = getCurrentZoneS("zoneSupply");
        string stored = getCurrentZoneS("zoneStored");

        Die[] h = gameLayout.getDiceInZone(supply);
        Die[] d = gameLayout.getDiceInZone(stored);

        int iDraw;
        

        if (h.Length == 0 && d.Length != 0)
        {
            moveDice(stored, supply, false);
            h = gameLayout.getDiceInZone(supply);
            d = new Die[0];
        }

        //moveDice(supply, support, amount);
        /*
            What to do if can't move the asked amount?
                If 'from' is "supply" then
                    moveDice(stored, supply, false)
                    continue
         */
        while (count != 0 && h.Length != 0)
        {
            iDraw = UnityEngine.Random.Range(0, h.Length);

            DieLogic.moveDie(h[iDraw], supportT);
            h[iDraw].toggleVisibility(true);

            SoundControl.instance.playAudio("dice", "slide");

            count--;
            h = gameLayout.getDiceInZone(supply);

            if (count > 0 && h.Length == 0 && d.Length > 0)
            {
                //yield return new WaitForSeconds(h[iDraw].GetComponent<AudioSource>().clip.length);
                moveDice(stored, supply, false);
                h = gameLayout.getDiceInZone(supply);
                d = new Die[0];
            }
        }

        Debug.Log("Moved " + (amount - count) + " dice !");
    }

    public void rollSimilarDice(Die d) {
        Die[] dice = d.transform.parent.GetComponentsInChildren<Die>();
        string sAudio = "rollS";

        if (dice.Length > 1)
            sAudio = "rollM_1"; // index of the multiple dice roll sound
        

        foreach (Die die in dice) {
            SoundControl.instance.setAudio("dice", sAudio);
            DieLogic.rollDie(die);
        }
    }


    public void moveDice(string f, string t)                    { moveDice(f, t, 0,   true);    }
    public void moveDice(string f, string t, int amt)           { moveDice(f, t, amt, true);    }
    public void moveDice(string f, string t, bool b)            { moveDice(f, t, 0,   b);       }
    public void moveDice(string f, string t, int amt, bool b)
    {
        Die[] dice = gameLayout.getDiceInZone(f);

        int count = amt;
        if (count <= 0 || count > dice.Length) count = dice.Length;
                
        for (int i = 0; i < count; i++)
        {


            //dice[i].moveDie(gameLayout.getZoneDiceParent(t).transform);
            DieLogic.moveDie(dice[i], gameLayout.getZone(t));
            dice[i].toggleVisibility(b);

            if (b) {
                SoundControl.instance.playAudio("dice", "slide");
            } else
            {
                SoundControl.instance.playAudio("dice", "drop_cup");
            }
        }        
    }


    public Zone getDraggedPlaceholder()
    {
        Draggable drag = draggedObject.GetComponent<Draggable>();
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
        Draggable drag = draggedObject.GetComponent<Draggable>();
        if (!drag) return false;

        //Die d = draggedObject.GetComponent<Die>();
        //if (!d) return false;
        
        switch (stepLogic.cStep)
        {
            case 0:
                
                if ((playerLogic.cPlayer == 0 && 
                     z.typeOfSlot == Zone.Slot.P1_STORED && drag.getParentZone().typeOfSlot == Zone.Slot.P1_SUMMONED) ||
                    (playerLogic.cPlayer == 1 && 
                     z.typeOfSlot == Zone.Slot.P2_STORED && drag.getParentZone().typeOfSlot == Zone.Slot.P2_SUMMONED))
                    isValid = true;
                break;
            case 1:
                isValid = false;
                break;
            case 2:
                isValid = false;
                break;
            case 3:
                isValid = false;
                if (drag.getParentArea().name == "areaMarket" && 
                    ((playerLogic.cPlayer == 0 && z.typeOfSlot == Zone.Slot.P1_STORED) ||
                     (playerLogic.cPlayer == 1 && z.typeOfSlot == Zone.Slot.P2_STORED)))
                    isValid = true;
                /*
                if ((draggedObject) == Stall &&
                    (z.id == 6 + (currentPlayerID * 10)))
                        return true;
                */
                break;
            case 4:
                isValid = false;
                break;
            case 5:
                isValid = false;
                break;


        }

        // houseRules
        isValid = true;



        if (drag.getParentZone() == z) isValid = true;
        return isValid;
    }

    public bool isValidClick(Draggable drag)
    {
        bool canClick = false;
        try {
            Debug.Log("Area: " + drag.getParentArea().name);
            // If draggable not in current player's area or the market, then do not allow to drag.
            if (drag.getParentArea().name != getCurrentZoneS("") && drag.getParentArea().name != "areaMarket")
                canClick = false;

            // If draggable clicked is in the rolled zone, then move the dice to proper spot.
            if (drag.getParentZone().name == "zoneRoll")
            {
                moveDice(getCurrentZoneS("zoneRoll"),
                         getCurrentZoneS("zoneServicable"));
                canClick = false;
            }

            // Depending on step and zone, allow draggability
            switch (stepLogic.cStep)
            {
                case 0:
                    // Clicked die in zoneSummonedcore dice
                    if (drag.getParentZone().name == "zoneSummoned")
                    {
                        moveDice(getCurrentZoneS("zoneSummoned"),
                                 getCurrentZoneS("zoneStored"));
                        canClick = false;
                    }
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    if (drag.getParentArea().name == "areaMarket")
                    {
                        canClick = true;
                    }
                    break;
                case 4:
                    break;
                case 5:
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log("isValidDrag err:/n" + e);
        }
        finally
        {
            if (canClick)
                draggedObject = drag.gameObject;
        }

        if (canClick)
            return true;
        return false;

    }

    
    public void dieDropped()
    {
        Die d = draggedObject.GetComponent<Die>();
        gameLayout.checkZoneColor();



        // Special Zone Cases (If drop is valid)
        switch (draggedObject.transform.parent.parent.name)
        {
            case "zoneSupply":
                //draggedObject
                d.toggleVisibility(false);
                SoundControl.instance.playAudio("dice", "drop_cup");
                break;
            default:
                SoundControl.instance.playAudio("dice", "drop_table");
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


    public string getGameDesc()
    {
        return
            "Player: " + playerLogic.currentPlayer().getPlayerName() + "\n" +
            "Step: " + stepLogic.getName() + "\n" +
            "Round: " + GameState.cRound + "\n" +
            "Desc: " + stepLogic.getDesc();
    }

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

        Transform d = gameLayout.scoreTokens[p];
        d.localPosition = new Vector3(gameLayout.positScores[playerLogic.players[p].score + 1].localPosition.x + mod * (.004f + UnityEngine.Random.Range(0f, .004f)),
                                    d.localPosition.y,
                                    gameLayout.positScores[playerLogic.players[p].score + 1].localPosition.z + mod * (.004f + UnityEngine.Random.Range(0f, .004f)));
        }
    }

    public void adminIncStep() {
        //stepLogic.incStep();
        updateTurn();
        //stepLogic.performStep();

        GameObject.Find("gameStateStats").GetComponent<Text>().text = getGameDesc();
    }

    public void adminMoveDie()
    {
        //Transform dT;
        Transform zT = GameObject.Find("areaPlayer1/zoneSupport").transform;
        Transform dT = GameObject.Find("areaPlayer1/zoneSupport").transform;

        switch (stepLogic.cStep)
        {
            case 0:
                dT = GameObject.Find("areaPlayer1/zoneSupport").transform;
                zT = GameObject.Find("areaPlayer1/zoneServicable").transform;
                break;
            case 1:
                dT = GameObject.Find("areaPlayer1/zoneServicable").transform;
                zT = GameObject.Find("areaPlayer1/zoneSummoned").transform;
                break;
            case 2:
                dT = GameObject.Find("areaPlayer1/zoneSummoned").transform;
                zT = GameObject.Find("areaPlayer1/zoneSpent").transform;
                break;
            case 3:
                dT = GameObject.Find("areaPlayer1/zoneSpent").transform;
                zT = GameObject.Find("areaPlayer1/zoneStored").transform;
                break;
            case 4:
                dT = GameObject.Find("areaPlayer1/zoneStored").transform;
                zT = GameObject.Find("areaPlayer1/zoneSupport").transform;
                break;
            case 5:
                dT = GameObject.Find("areaPlayer1/zoneSupport").transform;
                zT = GameObject.Find("areaPlayer1/zoneSupport").transform;
                break;
        }

        foreach (Die die in dT.GetComponentsInChildren<Die>())
        {
            /*
            item.GetComponent<Draggable3D_Plane>().transform.SetParent(zT.Find("Dice"));
            float offsetX = UnityEngine.Random.Range(-.05f, .05f);
            float offsetY = UnityEngine.Random.Range(-.05f, .05f);
            item.transform.localPosition = new Vector3(offsetX, offsetY, -.01f);
            */
            DieLogic.moveDie(die, zT.GetComponentInChildren<Zone>());
        }

        adminIncStep();
    }
}


[System.Serializable]
public class PlayerLogic
{
    public int cPlayer = 0;
    public int totalPlayers = 2;
    public Player[] players;

    // Add Market Vars:  # can buy at once (int), if can buy (bool)


    public Player currentPlayer() { return players[cPlayer]; }

    public void initPlayers()
    {
        if (GameState.newGame)
        {
            players = new Player[totalPlayers];
            for (int i = 0; i < totalPlayers; i++)
            {
                players[i] = new Player(i);
            }
        }
    }

    public bool cyclePlayer()
    {
        bool rBool = false;
        int start = 0;
        cPlayer = CommonLogic.cycleValue(cPlayer, players.Length, start);
        if (cPlayer == start)
            rBool = true;
        return rBool;
    }

    public int nextPlayer(int p)
    {
        int nP = p + 1;
        if (nP == totalPlayers) nP = 0;
        return nP;
    }
    
    /*
    public string getStepDesc()
    {
        return
            "Player: " + currentPlayer().getPlayerName() + "\n" +
            "Step: " + currentStep + "\n" +
            "Round: " + currentRound + "\n" +
            "Desc: " + descStep;
    }
    */


};

[System.Serializable]
public class StepLogic
{
    public enum eSteps { SCORE, SCRY, SPIN, SPEND, STRIKE, SECURE };
    public eSteps currentStep;

    public int cStep = 0;

    public StepInfo[] steps;
    

    public bool cycleStep()
    {
        bool rBool = false;
        int start = 0;
        cStep = CommonLogic.cycleValue(cStep, steps.Length, start);
        currentStep = (eSteps) cStep;
        if (cStep == start)
            rBool = true;
        return rBool;
    }

    public string getName() { return steps[cStep].getName(); }
    public string getDesc() { return steps[cStep].getDesc(); }
    
    public void initSteps()
    {
        string[] sName = Enum.GetNames(typeof(eSteps));
        steps = new StepInfo[sName.Length];
        steps[0] = new StepInfo(sName[0],
                    "Get Dice in Summoned\n" +
                    "Sum total of Die's Score value\n" +
                    "Move each Die to Stored");
        steps[1] = new StepInfo(sName[1],
                    "Move 5 Die from Supply to Support\n" +
                    "If Supply = 0, move all Die from Stored to Supply\n" +
                    "If Supply = 0 and Store = 0, continue");
        steps[2] = new StepInfo(sName[2],
                    "Roll all Die in Support\n" +
                    "Move all die to Servicable");
        steps[3] = new StepInfo(sName[3],
                    "Total Energy on Die\n" +
                    "Buy from Market\n" +
                    "Summon creature\n" +
                    "Extra special things");
        steps[4] = new StepInfo(sName[4],
                    "Attack all players\n" +
                    "Other Players defend");
        steps[5] = new StepInfo(sName[5],
                    "Move all Die from Servicable to Stored\n" +
                    "Move all Die from Spent to Store");
    }
}

[System.Serializable]
public class StepInfo
{
    [SerializeField]
    private string stepName;

    [SerializeField]
    private string stepDesc;

    public StepInfo(string name, string desc)
    {
        stepName = name;
        stepDesc = desc;
    }

    public string getName() { return stepName; }
    public string getDesc() { return stepDesc; }
}


public static class DieLogic
{
    public static void moveDie(Die d, Zone z)
    {
        // HARD: Know that a "Zone"'s parent contains a GameObject called "Dice" that will parent the "Die"
        d.transform.SetParent(z.transform.parent.Find("Dice"));

        d.offsetDie();
    }

    public static void moveDie(Die d, Transform t)
    {
        Transform dP = t.parent.Find("Dice").transform;

        if (dP)
        {
            d.transform.SetParent(dP);
            d.offsetDie();
        }
        else
        {
            Debug.Log("Die not moved, no 'Dice' transform found");
        }
    }


    public static Die clickedDie()
    {
        //bool b = false;
        Die die = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            die = hit.collider.transform.GetComponent<Die>();
        }

        return die;
        //return b;
    }

    public static Transform getCurrentSpawn()
    {
        //return GameObject.Find(GameLogic.instance.getCurrentZoneS("zoneRoll")).GetComponentInChildren<Zone>().transform;
        return GameLogic.instance.gameLayout.getZone("zoneRoll").transform;
    }

    public static void drawRollLine()
    {
        Transform s = getCurrentSpawn();
        Debug.DrawLine(s.position, getForce(s));
    }

    public static void rollDie(Die d)
    {
        float dieMag = d.transform.localScale.magnitude;

        Transform spawn = getCurrentSpawn();
        Vector3 force = getForce(spawn);

        moveDie(d, spawn);


        // Random Rotation
        d.transform.Rotate(new Vector3(UnityEngine.Random.value * 360,
                                       UnityEngine.Random.value * 360,
                                       UnityEngine.Random.value * 360));

        // Add Force
        d.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);

        // Add Random Torque
        // -50
        Vector3 tV = GameLogic.instance.rollValues.vtV;
        d.GetComponent<Rigidbody>().AddTorque(new Vector3(tV[0] * UnityEngine.Random.value * dieMag,
                                                          tV[1] * UnityEngine.Random.value * dieMag,
                                                          tV[2] * UnityEngine.Random.value * dieMag), ForceMode.Impulse);


        SoundControl.instance.playAudio();
    }

    static Vector3 getForce(Transform t)
    {
        /*
         2  +  7
        .5f +  4
        -2  + -3
        -35     20
        */
        Vector3 f1V = GameLogic.instance.rollValues.vf1V;
        Vector3 f2V = GameLogic.instance.rollValues.vf2V;
        Vector2 lV = GameLogic.instance.rollValues.vLV;

        //Transform rT = GameObject.Find("area" + GameLogic.instance.playerLogic.currentPlayer().getPlayerName() + "/zoneServicable/Dice").transform;

        Vector3 rollTarget = Vector3.zero + new Vector3(t.position.x * UnityEngine.Random.value,
                                                        f1V[1] + f2V[0] * UnityEngine.Random.value,
                                                        t.position.z * UnityEngine.Random.value);
        /*
        Vector3 rollTarget = Vector3.zero + new Vector3(f1V[0] + f2V[0] * UnityEngine.Random.value,
                                                        f1V[1] + f2V[0] * UnityEngine.Random.value,
                                                        f1V[2] + f2V[0] * UnityEngine.Random.value);
        */
        return Vector3.Lerp(t.position, rollTarget, 1).normalized * (lV[0] - UnityEngine.Random.value * lV[1]);
    }

}

[System.Serializable]
public class RollValues
{
    public float[] tV;
    public float[] f1V;
    public float[] f2V;
    public float[] lV;
    public Vector2 vLV;
    public Vector3 vtV, vf1V, vf2V;

}




public static class CommonLogic
{
    public static int cycleValue(int value, int max) { return cycleValue(value, max, 0); }
    public static int cycleValue(int value, int max, int start)
    {
        int rValue = ++value;
        if (rValue == max)
            rValue = start;
        return rValue;
    }
}