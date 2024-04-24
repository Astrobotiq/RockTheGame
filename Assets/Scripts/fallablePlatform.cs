using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fallablePlatform : MonoBehaviour
{
    public new BoxCollider2D collider;
    public PolygonCollider2D trigger;
    public Rigidbody2D body;

    private GameObject God;
    private GameCondition condition;

    private void Awake()
    {
        God = GameObject.FindWithTag("God");
        condition = God.GetComponent<GameCondition>();
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player") 
        {
            StartCoroutine(startFalling());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "destroyable")
        {
            Destroy(collision.gameObject);
            Destroy(this.gameObject);
        }
        if(collision.gameObject.tag == "Player")
        {
            condition.GameOver();
        }
    }

    IEnumerator startFalling()
    {
        body.bodyType = RigidbodyType2D.Dynamic;
        yield return new WaitForSeconds(2);
        transform.rotation = Quaternion.Euler(transform.rotation.x,transform.rotation.y,transform.rotation.z+180);
    }
}
