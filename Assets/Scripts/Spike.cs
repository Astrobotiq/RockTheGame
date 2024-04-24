using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spike : MonoBehaviour
{
    private GameObject God;
    private GameCondition condition;

    private void Awake()
    {
        God = GameObject.FindWithTag("God");
        condition = God.GetComponent<GameCondition>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            condition.GameOver();
        }
    }
}
