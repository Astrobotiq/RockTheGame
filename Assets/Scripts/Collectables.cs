using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectables : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator animator;
    private bool isCollected;
    private GameObject God;
    private GameCondition condition;

    
    private void Awake()
    {
        isCollected = false;
        God = GameObject.FindWithTag("God");
        condition = God.GetComponent<GameCondition>();
    }

    private void Update()
    {
        if (isCollected) 
        {
            StartCoroutine(destroyCollectable());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            animator.SetBool("isCollected", true);
            isCollected = true;
            condition.Add();
        }
    }


    IEnumerator destroyCollectable()
    {
        yield return new WaitForSeconds(1);
        Destroy(this.gameObject);
    }
}
