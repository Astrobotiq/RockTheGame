
using UnityEngine;


public class grapler : MonoBehaviour
{

    public Color enter;
    public Color exit;
    public GameObject Node;
    
    
    void Start()
    {
        Node = null;
    }

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Node"))
        {
            Node = collision.gameObject;
            Node.GetComponent<Renderer>().material.color = enter;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Node.GetComponent<Renderer>().material.color = exit;
        Node = null;
    }
}
