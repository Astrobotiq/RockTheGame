using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private float time;
    public bool isActive;

    public List<GameObject> first;
    public List<GameObject> second;
    

    

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        time = 0;
        isActive = true;
        foreach (GameObject node in first)
        {
            node.SetActive(true);

        }
        foreach (GameObject node in second)
        {
            node.SetActive(false);

        }

    }

    // Update is called once per frame
    void Update()
    {
        if (time >= 2.5)
        {
            isActive = !isActive;  
            time = 0;
        }
        if (isActive == false )
        {
            foreach ( GameObject node in first ) 
            {
                node.SetActive( false );
               
            }
            foreach (GameObject node in second)
            {
                node.SetActive(true);

            }
        }
        else 
        {
            foreach (GameObject node in first)
            {
                node.SetActive(true);

            }
            foreach (GameObject node in second)
            {
                node.SetActive(false);

            }
        }
        
        time += Time.deltaTime;

        

    }
}
