using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SpecialMove : MonoBehaviour
{
    public leftMovement left;
    public Movement right;
    public SpringJoint2D leftSpring;
    public SpringJoint2D rightSpring;
    public Rigidbody2D rb;

    private GameObject node;
    public int forceAmountX = 1;
    public int forceAmountY = 1;

    public int amount = 50;

    public bool canForce = true;






    // Start is called before the first frame update
    private void Update()
    {
        
       
    }

    private void FixedUpdate()
    {
        if (leftSpring.connectedBody == null && rightSpring.connectedBody != null  || leftSpring.connectedBody != null && rightSpring.connectedBody == null)
        {
            if (leftSpring.connectedBody != null)
            {
                
                node = leftSpring.connectedBody.gameObject;
            }
            else if (rightSpring.connectedBody != null)
            {
                
                node = rightSpring.connectedBody.gameObject;
            }


            if (transform.position.x < node.transform.position.x - 1)
            {
                if (Mathf.Abs(rb.velocity.x) < 40)
                {
                    rb.AddForce(Vector2.right * forceAmountX * Time.fixedDeltaTime, ForceMode2D.Force);
                    rb.AddForce(Vector2.down * forceAmountX * Time.fixedDeltaTime, ForceMode2D.Force);
                    forceAmountX += 1;


                }
            }
            else if (transform.position.x > node.transform.position.x + 1)
            {
                if (Mathf.Abs(rb.velocity.x) < 20)
                {
                    rb.AddForce(Vector2.left * forceAmountX * Time.fixedDeltaTime, ForceMode2D.Force);
                    rb.AddForce(Vector2.up * forceAmountX * Time.fixedDeltaTime, ForceMode2D.Force);
                    forceAmountX += 1;


                }
            }
            else
            {
                forceAmountX = 1;
            }

            if (rb.velocity.y > 20)
            {
                rb.AddForce(Vector2.down * forceAmountY * Time.fixedDeltaTime, ForceMode2D.Force);
                forceAmountY += 1;
            }
            else if (rb.velocity.y < -20)
            {
                rb.AddForce(Vector2.up * forceAmountY * Time.fixedDeltaTime, ForceMode2D.Force);
                forceAmountY += 1;
            }
            else
            {
                forceAmountY = 1;
            }


        }
        else if (GameObject.ReferenceEquals(leftSpring.connectedBody.gameObject, rightSpring.connectedBody.gameObject))
        {
            
                Vector3 direction = leftSpring.connectedBody.gameObject.transform.position- transform.position ;
                Vector2 realDir = new Vector2 (direction.x, direction.y);
               
                leftSpring.enabled = false;
                rightSpring.enabled = false;

            
                
                rb.AddForce(direction * Time.fixedDeltaTime * 3, ForceMode2D.Impulse);
            
            
            
            }
        else
        {
            node = null;
            
        }

        

    }

   

   
}
