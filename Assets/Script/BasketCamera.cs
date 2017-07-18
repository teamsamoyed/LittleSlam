using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketCamera : MonoBehaviour
{
    public GameObject Ball;
    public float YCut; //이거보다 공이 y축으로 높이 올라가면 따라 올름
    float StartY;
    public float MinX;
    public float MaxX;

	// Use this for initialization
	void Start ()
    {
        StartY = transform.position.y;
	}

    GameObject GetFollower()
    {
        if (Ball.activeSelf)
            return Ball;
        else
            return Ball.GetComponent<Ball>().Owner;
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        var newPosition = transform.position;
        var follower = GetFollower();

        newPosition.x = Mathf.Clamp(follower.transform.position.x, MinX, MaxX);

        if (follower.transform.position.y > YCut)
        {
            newPosition.y = StartY + follower.transform.position.y - YCut;
        }
        else
        {
            newPosition.y = StartY;
        }

        transform.position = newPosition;
	}
}
