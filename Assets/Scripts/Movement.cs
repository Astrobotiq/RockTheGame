using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    Input inputs;
    public GameObject pivot;
    Vector2 Rotation;
    private float Rotation2;
    private float Rotation3;
    public bool isFacingRight = true;
    public Rigidbody2D rb;
    public float force = 5f;
    public grapler rightGrapler;
    public grapler middleGrapler;
    public ColliderHandler handler;
    public GameObject node;

    public SpringJoint2D SpringJoint;
    public Transform firePoint;

    

    public bool isSwinging;

    public SpecialMove SM;

    



    // Start is called before the first frame update
    void Start()
    {
        inputs = new Input();
        inputs.Gameplay.Enable();
        pivot.transform.position = new Vector3(transform.position.x+1.46f,transform.position.y+0.14f,transform.position.z);
        SpringJoint.enabled = false;
        isSwinging = false;
    }

    // Update is called once per frame
    void Update()
    {
        updatePivotPosition();
        Rotation = inputs.Gameplay.RightRotation.ReadValue<Vector2>();
        Rotation3 = Rotation.x;

        

        if (Rotation3 > 0)
        {
            //Rotates the object if the player is facing right
            Rotation2 = Rotation.x + Rotation.y * 90;
            pivot.transform.rotation = Quaternion.Euler(0f, 0f, Rotation2);
        }
        else if (Rotation3 < 0)
        {
            //Rotates the object if the player is facing left
            Rotation2 = Rotation.x + Rotation.y * -90;
            pivot.transform.rotation = Quaternion.Euler(0f, 180f, -Rotation2);
        }

        
    }

    public void onJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            rb.velocity = Vector2.up * force;
        }
    }

    private void updatePivotPosition()
    {
        pivot.transform.position = new Vector3(transform.position.x + 0.46f, transform.position.y + 0.14f, transform.position.z);
    }

    public void onRightGrapling(InputAction.CallbackContext context)
    {
        
        if (context.performed )
        {
            if (handler.isMiddle)
            {
                node = middleGrapler.Node.gameObject;
            }
            else
            {
                node = rightGrapler.Node.gameObject;
            }
            if (node != null)
            {
                SpringJoint.connectedBody = node.GetComponent<Rigidbody2D>();
                SpringJoint.enabled = true;
                isSwinging = true;
            }
            else
            {
                SpringJoint.enabled = false;
                SpringJoint.connectedBody = null;
                isSwinging = false;
            }
        }
        else if (context.canceled)
        {
            SpringJoint.connectedBody = null;
            SpringJoint.enabled = false;
            SM.canForce = true;
            isSwinging = false;
        }
    }


    
}
