using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    
    
    public CinemachineVirtualCamera startCam;
    private CinemachineVirtualCamera currentCam;

    

    private void Start()
    {
        currentCam = startCam;
        currentCam.Priority = 20;
    }

    public void SwitchCam(CinemachineVirtualCamera cam)
    {
        currentCam.Priority = 10;
        currentCam = cam;
        currentCam.Priority = 20;
    }

}
