using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedControl : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    public SpringJoint2D left;
    public SpringJoint2D right;
    public float maxSpeed = 40;
    private GameObject node;
    public float forceAmountX = 0.1f;
    public float forceAmountY = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*if (left.connectedBody == null && right.connectedBody != null  || left.connectedBody != null && right.connectedBody == null)
        {
            
            if (left.connectedBody != null)
            {
                node = left.connectedBody.gameObject;
            }
            else if(right.connectedBody != null)
            {
                node = right.connectedBody.gameObject;
            }
            
          
        }
        else
        {
            node = null;
            forceAmountX = 0.1f;
        }

        if (node != null)
        {
            if (transform.position.x < node.transform.position.x - 1)
            {
                if (Mathf.Abs(rb.velocity.x) < maxSpeed)
                {
                    rb.AddForce(Vector2.right * forceAmountX * Time.fixedDeltaTime, ForceMode2D.Force);
                    forceAmountX += 0.1f;
                }
            }
            else if (transform.position.x > node.transform.position.x + 1)
            {
                if (Mathf.Abs(rb.velocity.x) < maxSpeed)
                {
                    rb.AddForce(Vector2.left * forceAmountX * Time.fixedDeltaTime, ForceMode2D.Force);
                   
                    forceAmountX += 0.1f;
                }
            }
            else
            {
                forceAmountX = 0.1f;
            }
        }

        */
        
        

    }

    private void FixedUpdate()
    {
        if (rb.velocity.y > 20)
        {
            rb.AddForce(Vector2.down*forceAmountY*Time.fixedDeltaTime, ForceMode2D.Force);
            forceAmountY += 0.1f;
        }
        else if (rb.velocity.y < -20)
        {
            rb.AddForce(Vector2.up*forceAmountY*Time.fixedDeltaTime,ForceMode2D.Force);
            forceAmountY += 0.1f;
        }
        else
        {
            forceAmountY = 0.1f;
        }
    }
}
