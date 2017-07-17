using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool IsPossessed;
    Rigidbody2D Body;
    public int Index;
    public float Hp;
    public float Speed;
    public float DashSpeed;

	// Use this for initialization
	void Start ()
    {
        Body = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (IsPossessed)
        {
            Control();
        }
        else
        {
            AutoMove();
        }
	}

    void Control()
    {
        var velocity = new Vector2(0.0f, 0.0f);

        if (Input.GetButton(Key.Up(Index)))
        {
            velocity.y = 1.0f;
        }

        if (Input.GetButton(Key.Down(Index)))
        {
            velocity.y = -1.0f;
        }

        if (Input.GetButton(Key.Left(Index)))
        {
            velocity.x = -1.0f;
        }

        if (Input.GetButton(Key.Right(Index)))
        {
            velocity.x = 1.0f;
        }

        if (velocity.x != 0.0f || velocity.y != 0.0f)
        {
            velocity.Normalize();
            velocity *= Speed;
        }

        Body.velocity = velocity;
    }

    void AutoMove()
    {
    }
}
