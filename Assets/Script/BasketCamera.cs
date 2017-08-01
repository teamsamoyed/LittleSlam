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

        if (newPosition.x < follower.transform.position.x - 0.3f)
            newPosition.x = follower.transform.position.x - 0.3f;

        if (newPosition.x > follower.transform.position.x + 0.3f)
            newPosition.x = follower.transform.position.x + 0.3f;

        newPosition.x = Mathf.Clamp(newPosition.x, MinX, MaxX);
        var fy = follower.transform.position.y;

        if (!Ball.activeSelf)
            fy += 0.2f;

        if (fy > YCut)
        {
            newPosition.y = StartY + fy - YCut;
        }
        else
        {
            newPosition.y = StartY;
        }

        var delta = newPosition - transform.position;

        /* 카메라 툭툭 튀는 거 방지 - 나중에 손 볼것
        if (delta.magnitude > 0.01f)
            delta *= 0.01f / delta.magnitude;
            */

        transform.position += delta;
	}
}
