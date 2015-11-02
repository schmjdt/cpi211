using UnityEngine;
using System.Collections;
using System;


//[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Draggable3D_Plane))]
public class Die : MonoBehaviour, IComparable {
    enum eDieColor { RED, ORANGE, YELLOW, GREEN, BLUE, PURPLE };


    public Color colorInside;
    public Color colorOutside;
    public int dieValue;

    //eDieColor dieColorValue;

    public Card card;

    public AudioClip[] audioclip;

    int sideStatValue;

    bool moving;

    [Range(0f, 1f)]
    public float validMargin = 0.45f;
    Vector3 localHitNormalized;

    bool localHit
    {
        get
        {
            Ray ray = new Ray(transform.position + (new Vector3(0, 2, 0) * transform.localScale.magnitude), Vector3.up * -1);
            RaycastHit hit = new RaycastHit();

            if (GetComponent<Collider>().Raycast(ray, out hit, 3 * transform.localScale.magnitude))
            {
                localHitNormalized = transform.InverseTransformPoint(hit.point.x, hit.point.y, hit.point.z).normalized;
                return true;
            }
            return false;
        }
    }

    bool rolling
    {
        get
        {
            return !(GetComponent<Rigidbody>().velocity.sqrMagnitude < .1f &&
                     GetComponent<Rigidbody>().angularVelocity.sqrMagnitude < .1f);
        }
    }


    // Use this for initialization
    void Start () {
        //rollDie();
        setDieColor();
    }
	
	// Update is called once per frame
	void Update () {

        if (!rolling && !moving && localHit)
            updateValue();

	}

    void OnMouseOver() {

        if (Input.GetMouseButtonDown(1))
        {
            //if (Input.GetKeyDown("r"))
            //{
        		if (Input.GetKey(KeyCode.LeftShift)) {
        			GameLogic.instance.rollSimilarDice(this);
        		}
        		else {
        			setAudio(3);
                	rollDie();
        		}
            //}
        }
    }


    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Die>() != null) {
        	//GetComponent<AudioSource>().enabled = false;
            positionDie();
        }
    }

    void OnTriggerExit(Collider other)
    {
    	//GetComponent<AudioSource>().enabled = true;
    }

    bool valid(float t, float v)
    {
        if (t > (v - validMargin) && t < (v + validMargin))
            return true;
        return false;
    }

    public int getSideValue(Side.valueTypes vt) {
        sideStatValue = card.getSide(dieValue).getStat(vt);
        return sideStatValue;
    }

    public void updateValue()
    {
        dieValue = 0;
        float delta = 1;
        int side = 1;

        Vector3 testHitVector;

        do
        {
            testHitVector = HitVector(side);
            if (testHitVector != Vector3.zero)
            {
                if (valid(localHitNormalized.x, testHitVector.x) &&
                    valid(localHitNormalized.y, testHitVector.y) &&
                    valid(localHitNormalized.z, testHitVector.z))
                {
                    float nDelta = Mathf.Abs(localHitNormalized.x - testHitVector.x) +
                                    Mathf.Abs(localHitNormalized.y - testHitVector.y) +
                                    Mathf.Abs(localHitNormalized.z - testHitVector.z);
                    if (nDelta < delta)
                    {
                        dieValue = side;
                        delta = nDelta;

                    }

                }
            }
            side++;
        } while (testHitVector != Vector3.zero);
    }

    
    public void toggleVisibility(bool b)
    {
        GetComponent<Renderer>().enabled = b;
        // Will cause Die to fall through table
        GetComponent<Collider>().enabled = b;
    }

    public bool isVisible() { return GetComponent<Renderer>().enabled; }

    public void setAudio(int i) {
    	GetComponent<AudioSource>().clip = audioclip[i];
    }

    public void playAudio() {
    	GetComponent<AudioSource>().Play();
    }


    public void rollDie()
    {
        Transform spawn = GameObject.Find("area" + GameLogic.instance.playerLogic.currentPlayer().getPlayerName() + "/zoneRoll").GetComponentInChildren<Zone>().transform;
        Vector3 force = getForce(spawn);

        moveDie(spawn);

        
        // Random Rotation
        this.transform.Rotate(new Vector3(  UnityEngine.Random.value * 360,
                                            UnityEngine.Random.value * 360,
                                            UnityEngine.Random.value * 360));

        // Add Force
        this.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);

        // Add Random Torque
        // -50
        float[] tV = GameLogic.instance.rollValues.tV;
        this.GetComponent<Rigidbody>().AddTorque(new Vector3(tV[0] * UnityEngine.Random.value * transform.localScale.magnitude,
                                                             tV[1] * UnityEngine.Random.value * transform.localScale.magnitude,
                                                             tV[2] * UnityEngine.Random.value * transform.localScale.magnitude), ForceMode.Impulse);


        playAudio();
    }

    Vector3 getForce(Transform t)
    {
        /*
         2  +  7
        .5f +  4
        -2  + -3
        -35     20
        */
        float[] f1V = GameLogic.instance.rollValues.f1V;
        float[] f2V = GameLogic.instance.rollValues.f2V;
        float[] lV = GameLogic.instance.rollValues.lV;

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


    public void rollDieValue()
    {
        dieValue = UnityEngine.Random.Range(1, 7);

        //dieColorValue = (eDieColor)(dieValue - 1);
        setDieColor();

        Debug.Log("Die rolled: " + dieValue);
    }


    public void moveDie(Zone z)
    {
        transform.SetParent(z.transform.parent.Find("Dice"));


        positionDie();
    }

    public void moveDie(Transform t)
    {
        Transform dP = t.parent.Find("Dice").transform;
        //this.GetComponent<Draggable3D_Plane>().transform.SetParent(dP);
        transform.SetParent(dP);
        positionDie();
    }

    void positionDie()
    {
        float offsetX = UnityEngine.Random.Range(-.05f, .05f);
        float offsetZ = UnityEngine.Random.Range(-.05f, .05f);
        this.transform.localPosition = new Vector3(offsetX, 0f, offsetZ);
    }


    /*
    void OnCollisionEnter(Collision collisionInfo)
    {
        moving = true;
    }

    void OnCollisionStay(Collision collisionInfo)
    {
        if (moving && collisionInfo.collider.transform.GetComponent<Die>())
        {
            positionDie();
        }
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        moving = false;
    }
    */
    


    public void setDieColor()
    {
        GetComponent<Renderer>().materials[0].color = colorOutside;
        GetComponent<Renderer>().materials[1].color = colorInside;
    }

    public void setDieColor(float[] outside, float[] inside)
    {
        GetComponent<Renderer>().materials[0].color = new Color(outside[0], outside[1], outside[2]);
        GetComponent<Renderer>().materials[1].color = new Color(inside[0], inside[1], inside[2]);
    }

    public void setDieColor(Color colorOutside, Color colorInside)
    {
        GetComponent<Renderer>().materials[0].color = colorOutside;
        GetComponent<Renderer>().materials[1].color = colorInside;
    }

    public Vector3 HitVector(int side)
    {
        switch (side)
        {
            case 1: return new Vector3(0F, 0F, 1F);
            case 2: return new Vector3(1F, 0F, 0F);
            case 3: return new Vector3(0F, -1F, 0F);
            case 4: return new Vector3(0F, 1F, 0F);
            case 5: return new Vector3(-1F, 0F, 0F);
            case 6: return new Vector3(0F, 0F, -1F);
        }
        return Vector3.zero;
    }
    /*
    Color getColor() { switch (dieColorValue) {
            case eDieColor.RED: return Color.red;
            case eDieColor.ORANGE: return new Color(1, .7f, 0, 1);
            case eDieColor.YELLOW: return Color.yellow;
            case eDieColor.GREEN: return Color.green;
            case eDieColor.BLUE: return Color.blue;
            case eDieColor.PURPLE: return new Color(.6f, 0, .4f, 1);
            default: return Color.black;
        }
    }
    */


    int IComparable.CompareTo(object obj) {
        Die d = (Die)obj;
        return sideStatValue.CompareTo(d.sideStatValue);
    }
}
