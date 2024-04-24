using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class SwitchCamera : MonoBehaviour
{
    public CinemachineVirtualCamera cam;
    public new PolygonCollider2D collider;
    public GameObject GameCondition;
    private CameraController controller;

    private void Awake()
    {
        if(GameCondition != null)
        {
            controller = GameCondition.GetComponent<CameraController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            cam.Priority = 20;
            controller.startCam.Priority = 10;
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            cam.Priority = 10;
            controller.startCam.Priority = 20;
        }
            

    }
}
