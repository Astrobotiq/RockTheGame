using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trytry : MonoBehaviour
{
    public SpringJoint2D sj;
    public float boing = 2f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (sj.distance > 10)
        {
            sj.distance -= Time.deltaTime*boing;
        }
    }
}
