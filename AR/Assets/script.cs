using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class script : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0f,10f,0f));
        transform.Translate(Vector3.forward);
    }

    
}
