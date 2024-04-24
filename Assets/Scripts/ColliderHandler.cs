using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class ColliderHandler : MonoBehaviour
{
    Input input;
    private Vector2 leftR;
    private Vector2 rightR;
    private GameObject leftFP;
    private GameObject rightFP;
    private GameObject middlePivot;
    private PolygonCollider2D leftCol;
    private PolygonCollider2D rightCol;
    private PolygonCollider2D middleCol;
    public GameObject pivot;
    public bool isMiddle;
    // Start is called before the first frame update
    void Start()
    {
        input = new Input();
        input.Gameplay.Enable();
        pivot.transform.position = new Vector3(transform.position.x,transform.position.y + 1.40f,transform.position.z);
    }

    private void Awake()
    {
        leftFP = GameObject.FindWithTag("LeftFirePoint");
        leftCol = leftFP.GetComponent<PolygonCollider2D>();
        rightFP = GameObject.FindWithTag("RightFirePoint");
        rightCol = rightFP.GetComponent<PolygonCollider2D>();
        middlePivot = GameObject.FindWithTag("MiddlePivot");
        middleCol = middlePivot.GetComponent<PolygonCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //pivot's place
        pivot.transform.position = new Vector3(transform.position.x, transform.position.y + 1.40f, transform.position.z);
        //reading other 2 pivot vector value
        leftR = input.Gameplay.LeftRotation.ReadValue<Vector2>();
        rightR = input.Gameplay.RightRotation.ReadValue<Vector2>();

        calculateRotate();
        calculateDegree();


    }

    private void calculateRotate()
    {
        float x = (leftR.x + rightR.x)/2;
        float y = (leftR.y + rightR.y)/2;
        //Debug.Log("x: " + x + " y: " + y);
        float angle = Mathf.Atan2(y,x)*Mathf.Rad2Deg;
        pivot.transform.rotation = Quaternion.Euler(0f,0f,angle);
    }

    private void calculateDegree()
    {
        var left = Mathf.Atan2(leftR.x,leftR.y)*Mathf.Rad2Deg;
        var right = Mathf.Atan2(rightR.x,rightR.y)*Mathf.Rad2Deg;
        if ( Mathf.Abs( left - right) < 30)
        {
            //kollarýn collider'ýný deaktive et
            leftCol.enabled = false;
            rightCol.enabled = false;
            //middle pivot'un collider'ýný aktive et
            middleCol.enabled = true;
            isMiddle = true;
        }
        else
        {
            //middle pivot'un collider'ýný deaktive et
            middleCol.enabled = false;
            isMiddle=false;
            //kollarýn collider'ýný aktive et
            leftCol.enabled = true;
            rightCol.enabled = true;
        }
    }
}
