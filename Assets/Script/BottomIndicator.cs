using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomIndicator : MonoBehaviour
{	
	// Update is called once per frame
	void Update ()
    {
        var pos = transform.position;
        pos.y = -0.032f;

        transform.position = pos;
		
	}
}
