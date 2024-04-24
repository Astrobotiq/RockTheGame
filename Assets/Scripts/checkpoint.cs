using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkpoint : MonoBehaviour
{
    public Animator animator;
    private GameCondition condition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            animator.SetBool("isEntered", true);
            condition.PlayerGoThroughtCheckPoint(this);
            
            
        }
    }

    public void SetGameCondition(GameCondition condition)
    {
        this.condition = condition;
    }

}
