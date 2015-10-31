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
    int sideStatValue;

    bool moving;

    // Use this for initialization
    void Start () {
        rollDie();
        setDieColor();
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(1)) {
			if (Input.GetKeyDown("r")) {
				rollDie();
			}
		}
	}
    public int getSideValue(Side.valueTypes vt) {
        sideStatValue = card.getSide(dieValue).getStat(vt);
        return sideStatValue;
    }

    // Requires collider
    void OnMouseDown() {

		//GetComponent<Renderer>().materials[0].color = dieColor;
		
	}

    public void moveDie(Transform z)
    {
        Transform dP = z.Find("Dice").transform;
        this.GetComponent<Draggable3D_Plane>().transform.SetParent(dP);
        positionDie();
    }

    void positionDie()
    {
        float offsetX = UnityEngine.Random.Range(-.05f, .05f);
        float offsetY = UnityEngine.Random.Range(-.05f, .05f);
        this.transform.localPosition = new Vector3(offsetX, offsetY, -.01f);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Die>() != null)
            positionDie();
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
    
	public void rollDie() {
        dieValue = UnityEngine.Random.Range(1, 7);

        //dieColorValue = (eDieColor)(dieValue - 1);
        setDieColor();

        Debug.Log("Die rolled: " + dieValue);
	}


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
