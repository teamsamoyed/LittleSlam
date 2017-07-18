using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody Body;
    public GameObject Owner;

	// Use this for initialization
	void Start ()
    {
        Body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	}
}
