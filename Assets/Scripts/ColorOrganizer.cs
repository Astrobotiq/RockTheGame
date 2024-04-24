using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorOrganizer : MonoBehaviour
{
    private GameObject left; 
    private GameObject right;
    grapler leftGrapler;
    grapler rightGrapler;


    private void Awake()
    {
        left = GameObject.FindWithTag("LeftFirePoint");
        right = GameObject.FindWithTag("RightFirePoint");
        leftGrapler = left.GetComponent<grapler>();
        rightGrapler = right.GetComponent<grapler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.ReferenceEquals(leftGrapler.Node, rightGrapler.Node))
        {
            leftGrapler.Node.GetComponent<SpriteRenderer>().color = Color.blue;
            rightGrapler.Node.GetComponent<SpriteRenderer>().color = Color.blue;
        }
        else if(leftGrapler.Node != null && rightGrapler.Node != null) 
        {
            leftGrapler.Node.GetComponent<SpriteRenderer>().color = Color.red;
            rightGrapler.Node.GetComponent<SpriteRenderer>().color = Color.red;
        }
        else if(rightGrapler.Node != null && leftGrapler.Node == null) 
        {
            rightGrapler.Node.GetComponent<SpriteRenderer>().color = Color.red;
        }
        else if (leftGrapler.Node != null && rightGrapler.Node == null)
        {
            leftGrapler.Node.GetComponent<SpriteRenderer>().color = Color.red;
        }

    }
}
