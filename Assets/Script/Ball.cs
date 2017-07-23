using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody Body;
    GameObject owner;

    public GameObject Owner
    {
        get
        {
            return owner;
        }

        set
        {
            var prevOwner = owner;
            owner = value;
            if (prevOwner == null)
                PlayerManager.InitAutoMove();
            else
                prevOwner.GetComponent<Player>().ChangeToAutoMove();
        }
    }

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
