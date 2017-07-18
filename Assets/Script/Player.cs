using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool IsPossessed;
    Rigidbody Body;
    BoxCollider Collider;
    public int Index;
    public int Team;
    public float Hp;
    public float Speed;
    public float Jump;
    GameObject Ball;

	// Use this for initialization
	void Start ()
    {
        Body = GetComponent<Rigidbody>();
        Collider = GetComponent<BoxCollider>();
        Ball = GameObject.FindGameObjectWithTag(Tags.Ball);
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
        Move();

        if (Input.GetButtonDown(Key.Pass(Index)))
        {
        }
    }

    void Move()
    {
        var velocity = new Vector3(0.0f, 0.0f);

        if (Input.GetButton(Key.Up(Index)) &&
            CanMove(new Vector3( 0.0f,0.0f,1.0f)))
        {
            velocity.z = 1.0f;
        }

        if (Input.GetButton(Key.Down(Index)) &&
            CanMove(new Vector3(0.0f, 0.0f, -1.0f)))
        {
            velocity.z = -1.0f;
        }

        if (Input.GetButton(Key.Left(Index)) &&
            CanMove(new Vector3(-1.0f, 0.0f, 0.0f)))
        {
            velocity.x = -1.0f;
            GetComponent<SpriteRenderer>().flipX = false;
        }

        if (Input.GetButton(Key.Right(Index)) &&
            CanMove(new Vector3(1.0f, 0.0f, 0.0f)))
        {
            velocity.x = 1.0f;
            GetComponent<SpriteRenderer>().flipX = true;
        }

        if (velocity.x != 0.0f || velocity.y != 0.0f)
        {
            velocity.Normalize();
            velocity *= Speed;
        }

        if (transform.position.y <= -0.01f &&
            Input.GetButtonDown(Key.Jump(Index)))
        {
            velocity.y += Jump;
        }

        Body.velocity = velocity;
    }

    bool CanMove(Vector3 Direction)
    {
        Vector3 CheckPosition = transform.position;
        Vector3 size = Collider.size * 0.5f;
        CheckPosition.y += size.y;
        CheckPosition.x += Direction.x * size.x;
        CheckPosition.z += Direction.z * size.z;
        size.y = 0.0f;

        var overlapped = Physics.OverlapBox(CheckPosition, size);

        foreach (var o in overlapped)
        {
            if (o.gameObject == gameObject)
                continue;

            if (o.gameObject == Ball)
                continue;

            return false;
        }

        return true;
    }

    void AutoMove()
    {
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject != Ball)
        {
            return;
        }

        //ball 소유하기
        Ball.GetComponent<Ball>().Owner = gameObject;
        Ball.SetActive(false);
    }
}