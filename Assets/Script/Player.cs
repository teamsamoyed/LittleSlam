using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerState
{
    Move,
    Shoot,
}

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
    public float PassTime;
    GameObject Ball;
    GameObject Floor;
    Animator Ani;
    bool IsLanded = true;
    bool BlockInput = false;
    float BlockTime = 0.0f;
    //보는 방향에서 이 각도 안 쪽일 때만 그 선수 겨냥해서 패스
    //그 외의 경우 그냥 해당 방향 직선으로 쏴버린다
    public float MaxPassDegree;
    public float DefaultPassSpeed;
    public float MaxPassSpeed;
    public float ShootMaxHoldTime;

    public float ShootX;
    public float ShootY;

    float ShootHoldTime;
    bool WaitShootRelease;
    bool IsShootMotionEnded;

    PlayerState State = PlayerState.Move;

	// Use this for initialization
	void Start ()
    {
        Body = GetComponent<Rigidbody>();
        Collider = GetComponent<BoxCollider>();
        Ani = GetComponent<Animator>();
        Ball = GameObject.FindGameObjectWithTag(Tags.Ball);
        Floor = GameObject.FindGameObjectWithTag(Tags.Floor);
	}

    void ReadyShoot()
    {
        Body.AddForce(0.0f, Jump, 0.0f);
        IsShootMotionEnded = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        Ani.SetBool("Landing", IsLanded);
        Ani.SetBool("OwnBall", Ball.GetComponent<Ball>().Owner == gameObject);

        if (BlockInput)
        {
            BlockTime -= Time.deltaTime;
            if (BlockTime <= 0.0f)
            {
                BlockInput = false;
            }
            else
            {
                return;
            }
        }

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
        switch (State)
        {
            case PlayerState.Move:
                MoveControl();
                break;
            case PlayerState.Shoot:
                ShootControl();
                break;
        }
    }

    void MoveControl()
    {
        Move();

        if (Ball.GetComponent<Ball>().Owner == gameObject)
        {
            if (Input.GetButtonDown(Key.Pass(Index)))
            {
                Pass();
            }

            if (Input.GetButtonDown(Key.Shoot(Index)))
            {
                State = PlayerState.Shoot;
                Ani.ResetTrigger("Shoot");
                Ani.ResetTrigger("ShootEnd");
                Ani.SetTrigger("Shoot");
                WaitShootRelease = true;
                IsShootMotionEnded = false;
                ShootHoldTime = 0.0f;
            }
        }
    }

    void ShootImpulse()
    {
        var dir = new Vector3(1.0f, 1.0f);
        dir.Normalize();

        var startPos = transform.position;

        if (GetComponent<SpriteRenderer>().flipX)
            startPos.x -= ShootX;
        else
            startPos.x += ShootX;

        startPos.y += ShootY;

        dir *= 3.0f;
        Ball.SetActive(true);
        Ball.transform.position = startPos;
        Ball.GetComponent<Ball>().Owner = null;
        Ball.GetComponent<Rigidbody>().velocity = dir;
    }

    void ShootControl()
    {
        if (Input.GetButtonUp(Key.Shoot(Index)))
        {
            WaitShootRelease = false;
        }

        ShootHoldTime += Time.deltaTime;

        if (ShootHoldTime > ShootMaxHoldTime)
            WaitShootRelease = false;

        if (IsShootMotionEnded && !WaitShootRelease)
        {
            //공 발사
            Ani.SetTrigger("ShootEnd");

            if (IsLanded)
            {
                State = PlayerState.Move;
            }
        }
    }

    void Move()
    {
        if (!IsLanded)
            return;

        var velocity = GetInputDirection();

        if (velocity.sqrMagnitude > 0.0f)
        {
            if (velocity.x < 0.0f)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
            else if(velocity.x > 0.0f)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }
        }

        if (velocity != Vector3.zero && transform.position.y <= -0.01f)
            Ani.SetBool("Running", true);
        else
            Ani.SetBool("Running", false);

        if (CanMove(velocity))
        {
            transform.Translate(Time.deltaTime * velocity * Speed);
        }
    }

    Vector3 GetInputDirection()
    {
        var velocity = new Vector3(0.0f, 0.0f, 0.0f);

        if (Input.GetButton(Key.Up(Index)))
        {
            velocity.z = 1.0f;
        }

        if (Input.GetButton(Key.Down(Index)))
        {
            velocity.z = -1.0f;
        }

        if (Input.GetButton(Key.Left(Index)))
        {
            velocity.x = -1.0f;
        }

        if (Input.GetButton(Key.Right(Index)))
        {
            velocity.x = 1.0f;
        }

        if(velocity.magnitude > 0.0f)
            velocity.Normalize();

        return velocity;
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
        if (collision.gameObject == Floor)
        {
            IsLanded = true;
        }

        if (collision.gameObject != Ball)
        {
            return;
        }

        //ball 소유하기
        Ball.GetComponent<Ball>().Owner = gameObject;
        Ball.SetActive(false);

        if (!IsPossessed)
        {
            var players = GameObject.FindGameObjectsWithTag(Tags.Player);

            foreach (var player in players)
            {
                if (player.GetComponent<Player>().Team != Team)
                {
                    continue;
                }

                player.GetComponent<Player>().IsPossessed = false;
            }

            IsPossessed = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == Floor)
            IsLanded = false;
    }

    void Pass()
    {
        Ball.SetActive(true);

        //아군은 pass time동안은 못 움직임
        var players = GameObject.FindGameObjectsWithTag(Tags.Player);

        foreach (var player in players)
        {
            if (player.GetComponent<Player>().Team != Team)
                continue;

            player.GetComponent<Player>().BlockInput = true;
            player.GetComponent<Player>().BlockTime = PassTime;
        }

        var goal = GetPassHandler();
        Vector3 start;
        Vector3 end;

        if (goal != null)
        {
            start = transform.position;
            end = goal.transform.position;
            var dir = end - start;
            dir.Normalize();
            start += dir * 0.15f;
            start.y += 0.12f;
            end.y -= 0.24f;

            IsPossessed = false;
            goal.GetComponent<Player>().IsPossessed = true;
        }
        else
        {
            var dir = GetPassDirection();

            //이 방향으로 기본 속도로 쏜다 그냥 그게 끝
            start = transform.position;
            end = start + dir * DefaultPassSpeed;
            dir.Normalize();
            start += dir * 0.15f;
            start.y += 0.12f;
            end.y -= 0.24f;
        }

        var velocity = (end - start) / PassTime - Physics.gravity * PassTime;

        if (velocity.magnitude > MaxPassSpeed)
        {
            velocity *= MaxPassSpeed / velocity.magnitude;
        }

        Ball.transform.position = start;
        Ball.GetComponent<Rigidbody>().velocity = velocity;
        Ball.GetComponent<Ball>().Owner = null;
    }

    Vector3 GetPassDirection()
    {
        var dir = GetInputDirection();

        if (dir.sqrMagnitude == 0.0f)
        {
            if (GetComponent<SpriteRenderer>().flipX)
            {
                dir.x = -1.0f;
            }
            else
            {
                dir.x = 1.0f;
            }
        }

        return dir;
    }

    GameObject GetPassHandler()
    {
        var dir = GetPassDirection();
        var dir2d = new Vector2(dir.x, dir.z);

        var players = GameObject.FindGameObjectsWithTag(Tags.Player);

        GameObject target = null;
        float targetDistance = 10000.0f;

        foreach (var player in players)
        {
            if (player == gameObject ||
                player.GetComponent<Player>().Team != Team)
            {
                continue;
            }

            float playerZ = player.transform.position.z - transform.position.z;
            float playerX = player.transform.position.x - transform.position.x;
            var pdir = new Vector2(playerX, playerZ);
            float angle = Vector2.Angle(dir2d, pdir);

            if (Mathf.Abs(angle) >= MaxPassDegree)
                continue;

            float distance = new Vector2(playerX, playerZ).magnitude;
            if (targetDistance > distance)
            {
                target = player;
                targetDistance = distance;
            }
        }

        return target;
    }
}