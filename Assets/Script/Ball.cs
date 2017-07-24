using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody Body;
    GameObject owner;
    int RecentTeam = 0;
    Vector3 prevPos;

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
            {
                PlayerManager.InitAutoMove();
            }
            else
            {
                prevOwner.GetComponent<Player>().ChangeToAutoMove();
                RecentTeam = prevOwner.GetComponent<Player>().Team;
            }
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
        if (GameManager.Instance.Phase != GamePhase.InGame)
            return;

        bool isOut = false;
        var outlinePos = new Vector3();

        if (prevPos.x >= -XCut && prevPos.x <= XCut &&
            prevPos.z >= -ZCut && prevPos.z <= ZCut)
        {
            if (transform.position.x <= -XCut)
            {
                isOut = true;
                outlinePos.x = -XCut - 0.1f;
                outlinePos.z = transform.position.z;
            }
            else if (transform.position.x >= XCut)
            {
                isOut = true;
                outlinePos.x = XCut + 0.1f;
                outlinePos.z = transform.position.z;
            }
            else if (transform.position.z <= -ZCut)
            {
                isOut = true;
                outlinePos.x = transform.position.x;
                outlinePos.z = -ZCut - 0.1f;
            }
            else if (transform.position.z >= ZCut)
            {
                isOut = true;
                outlinePos.x = transform.position.z;
                outlinePos.z = ZCut + 0.1f;
            }
        }

        if (isOut)
        {
            GameManager.Instance.Phase = GamePhase.Wait;
            StartCoroutine(GameManager.Instance.ToOutlinePass((RecentTeam + 1) % 2, outlinePos));
        }

        prevPos = transform.position;
    }
}
