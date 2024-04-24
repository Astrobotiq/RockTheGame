using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class whale : MonoBehaviour
{

    public BoxCollider2D damagePad;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Karakter hasar aldý");
            var opposite =collision.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(opposite*100*Time.deltaTime,ForceMode2D.Impulse);
        }
    }

    private void Update()
    {
        
    }

}
