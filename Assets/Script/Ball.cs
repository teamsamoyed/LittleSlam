using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    GameObject owner;
    int RecentTeam = 0;
    public Rigidbody Body;

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

            if (prevOwner == null ||
               (owner != null && prevOwner != null && owner.GetComponent<Player>().Team != prevOwner.GetComponent<Player>().Team))
            {
                PlayerManager.InitAutoMove();
            }
            else
            {
                prevOwner.GetComponent<Player>().ChangeToAutoMove();
                RecentTeam = prevOwner.GetComponent<Player>().Team;
            }

            if (owner != null && GameManager.Instance.Phase == GamePhase.OutlinePass)
            {
                GameManager.Instance.Phase = GamePhase.InGame;
            }
        }
    }

    public float XCut;
    public float ZCut;

    public int Score = 2;

    void Start()
    {
        Body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.Phase != GamePhase.InGame)
            return;

        bool isOut = false;
        var outlinePos = new Vector3();

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

        if (isOut)
        {
            GameManager.Instance.Phase = GamePhase.Wait;
            GameManager.Instance.OutlinePass((RecentTeam + 1) % 2, outlinePos);
        }
    }
}
