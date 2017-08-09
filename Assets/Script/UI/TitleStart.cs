using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleStart : MonoBehaviour
{
    float time = 0.0f;

    public float Speed;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        time += Time.deltaTime * Speed;

        while (time > 1.0f)
            time -= 1.0f;

        var alpha = Mathf.Abs(1 - 2 * time);

        GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, alpha);
	}
}
