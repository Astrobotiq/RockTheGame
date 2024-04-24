using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : MonoBehaviour
{
    public BoxCollider2D trigger;

    private GameObject God;
    private GameCondition condition;

    private void Awake()
    {
        God = GameObject.FindWithTag("God");
        condition = God.GetComponent<GameCondition>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "player" && condition.canFinish)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }

}
