using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody Body;
    public GameObject Owner;
    public float XCut;
    public float ZCut;

	// Use this for initialization
	void Start ()
    {
        Body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (transform.position.x <= -XCut ||
            transform.position.x >= XCut ||
            transform.position.z <= -ZCut ||
            transform.position.z >= ZCut)
        {
            GameManager.Instance.Phase = GamePhase.OutlinePass;
        }
	}
}
