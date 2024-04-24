using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCondition : MonoBehaviour
{
    public Vector3 lastCheckPoint;
    public GameObject player;
    public bool canFinish;
    private int SBNum;
   

    private void Awake()
    {
        Transform flagsTransform = transform.Find("Flags");

        foreach (Transform flagTransform in flagsTransform)
        {
            checkpoint checkpoint = flagTransform.GetComponent<checkpoint>();
            checkpoint.SetGameCondition(this);
        }
        SBNum = 0;
        canFinish = false;
    }

    public void PlayerGoThroughtCheckPoint(checkpoint cp)
    {
        lastCheckPoint = cp.gameObject.transform.position;
    }

    public void GameOver()
    {
        if (lastCheckPoint != null)
        {
            player.transform.position = lastCheckPoint;
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void Add()
    {
        SBNum++;

        if (SBNum == 2)
        {
            canFinish = true;
        }
    }
}
