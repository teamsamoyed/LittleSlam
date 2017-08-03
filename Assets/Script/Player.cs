using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerState
{
    Move,
    Shoot,
    Block
}

enum AutoMoveState
{
    Stay,
    Move
}

public class Player : MonoBehaviour
{
    public bool IsPossessed;
    Rigidbody Body;
    BoxCollider Collider;
    public int Team;
    public int Index;
    public float Hp;
    public float Speed;
    public float Jump;
    public float DunkJump;
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
    public float StealRange;

    public float ShootX;
    public float ShootY;
    public float FrontShootX;
    public float FrontShootY;
    public float BackShootX;
    public float BackShootY;

    public float ThreePointDistance;

    float ShootHoldTime;
    bool WaitShootRelease;
    bool IsShootMotionEnded;
    bool IsAction;
    bool IsBlockMotionEnded;

    PlayerState State = PlayerState.Move;
    AutoMoveState AutoState = AutoMoveState.Stay;
    float StayTime = 0.0f;
    Vector3 AutoGoal;

    Vector3 PassStart;
    Vector3 PassVelocity;
    GameObject PassGoal;

    public float XCut;
    public float ZCut;

    public bool IsBlock;

    GameObject Indicator;

	// Use this for initialization
	void Start ()
    {
        Body = GetComponent<Rigidbody>();
        Collider = GetComponent<BoxCollider>();
        Ani = GetComponent<Animator>();
        Ball = GameObject.FindGameObjectWithTag(Tags.Ball);
        Floor = GameObject.FindGameObjectWithTag(Tags.Floor);
        InitAutoGoal();
        AutoState = AutoMoveState.Move;
        Indicator = transform.Find("Indicator").gameObject;
	}

    void ReadyShoot()
    {
        Body.AddForce(0.0f, Jump, 0.0f);
        IsShootMotionEnded = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        Indicator.SetActive(IsPossessed);

        Ani.SetBool("Landing", IsLanded);
        Ani.SetBool("OwnBall", Ball.GetComponent<Ball>().Owner == gameObject);

        if (IsBlock && Body.velocity.y < 0.0f)
        {
            Ani.SetTrigger("BlockEnd");
        }

        if (BlockInput)
        {
            BlockTime -= Time.deltaTime;
            if (BlockTime <= 0.0f)
            {
                BlockInput = false;
            }
        }

        switch (GameManager.Instance.Phase)
        {
            case GamePhase.BallGetting:
                break;
            case GamePhase.FreeDraw:
                break;
            case GamePhase.GameEnd:
                break;
            case GamePhase.InGame:
                InGameUpdate();
                break;
            case GamePhase.OutlinePass:
                OutlinePassUpdate();
                break;
            case GamePhase.Wait:
                break; // wait에서는 암 것도 안함(상태 바뀌길 기다림)
        }
	}

    #region InGame

    void InGameUpdate()
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
        switch (State)
        {
            case PlayerState.Move:
                MoveControl();
                break;
            case PlayerState.Shoot:
                ShootControl();
                break;
            case PlayerState.Block:
                BlockControl();
                break;
        }
    }

    #region Move
    void MoveControl()
    {
        Move();

        if (Ball.GetComponent<Ball>().Owner == gameObject)
        {
            OwnerControl();
        }
        else
        {
            NonOwnerControl();
        }
    }

    void OwnerControl()
    {
        Ball.transform.position = transform.position;

        if (Input.GetButtonDown(Key.Pass(Team)))
        {
            Pass();
        }

        if (Input.GetButtonDown(Key.Shoot(Team)))
        {
            State = PlayerState.Shoot;
            Ani.ResetTrigger("Shoot");
            Ani.ResetTrigger("ShootEnd");
            WaitShootRelease = true;
            IsShootMotionEnded = false;
            ShootHoldTime = 0.0f;

            var goalposts = GameObject.FindGameObjectsWithTag(Tags.Goalpost);

            GameObject goal = null;

            foreach (var goalpost in goalposts)
            {
                if (goalpost.GetComponent<Goalpost>().Team != Team)
                {
                    goal = goalpost;
                    break;
                }
            }

            var startPos = transform.position;
            var dir = goal.transform.position - startPos;

            if (Mathf.Abs(dir.x) * 4 < Mathf.Abs(dir.z))
            {
                if (dir.z < 0.0f)
                {
                    Ani.SetBool("Front", true);
                    Ani.SetBool("Back", false);
                }
                else if (dir.z > 0.0f)
                {
                    Ani.SetBool("Front", false);
                    Ani.SetBool("Back", true);
                }

                GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                Ani.SetBool("Front", false);
                Ani.SetBool("Back", false);

                if (goal.transform.position.x < startPos.x)
                    GetComponent<SpriteRenderer>().flipX = true;
                else
                    GetComponent<SpriteRenderer>().flipX = false;
            }

            Ani.SetTrigger("Shoot");
        }
    }

    void StealCheck()
    {
        if (Ball.GetComponent<Ball>().Owner == null)
            return;

        var distance = Vector3.Distance(transform.position,
            Ball.transform.position);

        if (distance < StealRange)
        {
            Ball.GetComponent<Ball>().Owner = gameObject;
        }
    }

    void StealEnd()
    {
        Ani.ResetTrigger("Steal");
        IsAction = false;
    }

    void NonOwnerControl()
    {
        if (IsAction)
            return;

        var owner = Ball.GetComponent<Ball>().Owner;

        if (owner != null && owner.GetComponent<Player>().Team == Team)
            return;

        if (Input.GetButtonDown(Key.Steal(Team)))
        {
            Ani.SetTrigger("Steal");
            IsAction = true;

            if (Ball.transform.position.x > transform.position.x)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
        }

        if (Input.GetButtonDown(Key.Block(Team)))
        {
            Ani.SetTrigger("Block");
            State = PlayerState.Block;
            IsAction = true;

            if (Ball.transform.position.x > transform.position.x)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
        }
    }

    void BlockRelease()
    {
        IsBlock = true;
        Body.AddForce(0.0f, Jump, 0.0f);
    }

    void BlockEnd()
    {
        IsBlock = false;
        IsBlockMotionEnded = true;
    }

    void BlockControl()
    {
        if (IsBlockMotionEnded && IsLanded)
        {
            State = PlayerState.Move;

            Ani.ResetTrigger("Block");
            Ani.ResetTrigger("BlockEnd");
            IsAction = false;
            IsBlockMotionEnded = false;
        }
    }

    void Move()
    {
        if (!IsLanded)
            return;

        if (IsAction)
            return;

        var velocity = GetInputDirection();

        MoveTo(velocity, false);
    }

    Vector3 GetInputDirection()
    {
        var velocity = new Vector3(0.0f, 0.0f, 0.0f);

        if (Input.GetButton(Key.Up(Team)))
        {
            velocity.z = 1.0f;
        }

        if (Input.GetButton(Key.Down(Team)))
        {
            velocity.z = -1.0f;
        }

        if (Input.GetButton(Key.Left(Team)))
        {
            velocity.x = -1.0f;
        }

        if (Input.GetButton(Key.Right(Team)))
        {
            velocity.x = 1.0f;
        }

        if (velocity.magnitude > 0.0f)
            velocity.Normalize();

        return velocity;
    }

    bool CanMove(Vector3 Direction, bool IsAutoMove = false)
    {
        if (IsAutoMove)
            return true;

        Vector3 nowPos = transform.position;

        if (nowPos.x <= -XCut || nowPos.x >= XCut ||
            nowPos.z <= -ZCut || nowPos.z >= ZCut)
            return true; // 이미 필드 바깥으로 나가있는 경우 다시 안으로 들어올 수 있게 해주기 위함

        Vector3 CheckPosition = transform.position;
        Vector3 size = Collider.size * 0.5f;
        CheckPosition.y += size.y;
        CheckPosition.x += Direction.x * size.x;
        CheckPosition.z += Direction.z * size.z;

        if (!IsAutoMove)
        {
            if (CheckPosition.x <= -XCut || CheckPosition.x >= XCut ||
                CheckPosition.z <= -ZCut || CheckPosition.z >= ZCut)
                return false;
        }

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

    void MoveTo(Vector3 direction, bool IsAutoMove)
    {
        if (direction == Vector3.zero || BlockInput)
        {
            Ani.SetBool("Running", false);
            return;
        }

        Ani.SetBool("Running", true);

        if (direction.x < 0.0f)
        {
            Ani.SetBool("Back", false);
            Ani.SetBool("Front", false);
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else if (direction.x > 0.0f)
        {
            Ani.SetBool("Back", false);
            Ani.SetBool("Front", false);
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (direction.z > 0.0f)
        {
            Ani.SetBool("Back", true);
            Ani.SetBool("Front", false);
        }
        else if (direction.z < 0.0f)
        {
            Ani.SetBool("Back", false);
            Ani.SetBool("Front", true);
        }

        if (!CanMove(direction, IsAutoMove))
            return;

        transform.Translate(direction * Time.deltaTime * Speed);
    }
    #endregion

    #region Pass
    void Pass(GameObject goal = null)
    {
        if(goal == null)
            goal = GetPassHandler();

        PassGoal = goal;

        Vector3 end;

        if (goal != null)
        {
            PassStart = transform.position;
            end = goal.transform.position;
            var dir = end - PassStart;
            dir.Normalize();
            PassStart += dir * 0.15f;
            PassStart.y += 0.12f;
            end.y += 0.12f;
        }
        else
        {
            var dir = GetPassDirection();

            //이 방향으로 기본 속도로 쏜다 그냥 그게 끝
            PassStart = transform.position;
            end = PassStart + dir * DefaultPassSpeed;
            dir.Normalize();
            PassStart += dir * 0.15f;
            PassStart.y += 0.12f;
            end.y += 0.12f;
        }

        Ani.SetBool("Front", false);
        Ani.SetBool("Back", false);

        PassVelocity = (end - PassStart) / PassTime - Physics.gravity * PassTime * 0.5f;

        if (PassVelocity.x < 0.0f)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else if (PassVelocity.x > 0.0f)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }

        Ani.SetTrigger("Pass");

        if (PassVelocity.magnitude > MaxPassSpeed)
        {
            PassVelocity *= MaxPassSpeed / PassVelocity.magnitude;
        }

        IsAction = true;
    }

    void PassEnd()
    {
        IsAction = false;
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

        Ani.ResetTrigger("Pass");
        Ball.transform.position = PassStart;
        Ball.GetComponent<Rigidbody>().velocity = PassVelocity;
        Ball.GetComponent<Ball>().Owner = null;

        if (PassGoal != null)
        {
            IsPossessed = false;
            PassGoal.GetComponent<Player>().IsPossessed = true;
        }
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
    #endregion

    #region Shoot

    void ShootControl()
    {
        if (Input.GetButtonUp(Key.Shoot(Team)))
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

    void ShootImpulse()
    {
        var goalposts = GameObject.FindGameObjectsWithTag(Tags.Goalpost);

        GameObject goal = null;

        foreach (var goalpost in goalposts)
        {
            if (goalpost.GetComponent<Goalpost>().Team != Team)
            {
                goal = goalpost;
                break;
            }
        }

        var startPos = transform.position;

        if (Ani.GetBool("Front"))
        {
            startPos.x += FrontShootX;
            startPos.y += FrontShootY;
        }
        else if (Ani.GetBool("Back"))
        {
            startPos.x += BackShootX;
            startPos.y += BackShootY;
        }
        else
        {
            if (GetComponent<SpriteRenderer>().flipX)
                startPos.x -= ShootX;
            else
                startPos.x += ShootX;
        }

        startPos.y += ShootY;

        var endPos = goal.transform.position;
        endPos.x -= 0.001f;
        var ShootTime = 0.8f;
        var dist = Mathf.Min((endPos - startPos).magnitude, 3.0f);

        ShootTime += dist * 0.33333f;

        var dir = (endPos - startPos) / ShootTime - 0.5f * Physics.gravity * ShootTime;

        var yvel = Body.velocity.y;

        dir *= 1.0f - Random.Range(0.0f, Mathf.Abs(yvel) * 0.1f);

        Ball.SetActive(true);
        Ball.transform.position = startPos;
        Ball.GetComponent<Ball>().Owner = null;

        var xzPos = new Vector2(transform.position.x, transform.position.z);
        var xzGoal = new Vector2(goal.transform.position.x, goal.transform.position.z);

        if (Vector2.Distance(xzPos, xzGoal) < ThreePointDistance)
            Ball.GetComponent<Ball>().Score = 2;
        else
            Ball.GetComponent<Ball>().Score = 3;

        Ball.GetComponent<Rigidbody>().velocity = dir;
    }

    #endregion

    #region AutoMove

    void AutoMove()
    {
        if (Ball.GetComponent<Ball>().Owner == null)
        {
            MoveTo(Vector3.zero, true);
            return;
        }

        if (AutoState == AutoMoveState.Stay)
        {
            MoveTo(Vector3.zero, true);
            StayTime -= Time.deltaTime;

            if (StayTime <= 0.0f)
            {
                AutoState = AutoMoveState.Move;
                InitAutoGoal();
            }

            return;
        }

        var now = transform.position;

        if (Vector3.Distance(now, AutoGoal) < 0.1f)
        {
            AutoState = AutoMoveState.Stay;
            StayTime = Random.Range(3.5f, 5.0f);
            return;
        }

        var dir = AutoGoal - now;
        dir.Normalize();

        MoveTo(dir, true);
    }

    void InitAutoGoal()
    {
        if (Ball.GetComponent<Ball>().Owner == null)
            return;

        if (Ball.GetComponent<Ball>().Owner.GetComponent<Player>().Team == Team)
        {
            //우리 팀이 갖고 있는 경우 - 상대 골대 쪽 특정 지점으로 이동함
            AutoGoal.z = Random.Range(-ZCut, ZCut);
            if (Team == 1)
            {
                AutoGoal.x = Random.Range(-0.5f * XCut, -XCut);
            }
            else
            {
                AutoGoal.x = Random.Range(0.5f * XCut, XCut);
            }
        }
        else
        {
            //적팀이 갖고 있는 경우 - possessed 되어 있는 애들 제외 나머지 애들끼리 서로 마크하게 만들기
            AutoGoal.z = Random.Range(-ZCut, ZCut);
            if (Team == 0)
            {
                AutoGoal.x = Random.Range(-0.5f * XCut, -XCut);
            }
            else
            {
                AutoGoal.x = Random.Range(0.5f * XCut, XCut);
            }
        }
    }

    public void ChangeToAutoMove()
    {
        InitAutoGoal();
        AutoState = AutoMoveState.Move;
    }

    #endregion

    #region Steal
    #endregion

    #endregion

    #region OutlinePass
    void OutlinePassUpdate()
    {
        var owner = Ball.GetComponent<Ball>().Owner;

        if (owner == null)
            return;

        if (owner == gameObject)
        {
            if (Input.GetButtonDown(Key.Pass(Team)))
            {
                Pass(PlayerManager.GetPlayer(Team, 1));
            }
            return;
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

    public void ToOutlinePos(int OwnTeam, Vector3 Pos)
    {
        if (OwnTeam != Team)
            return;

        IsPossessed = false;

        if (Index == 0)
        {
            //패스 대상
            Ball.SetActive(false);
            Ball.GetComponent<Ball>().Owner = gameObject;
            transform.position = Pos;
        }
        else if (Index == 1)
        {
            IsPossessed = true;
        }
    }
    #endregion

    #region Collision
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

        if (State == PlayerState.Move && !IsAction)
        {
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
        else if (State == PlayerState.Block && IsBlock)
        {
            //공을 날아오는 반대편으로 쳐낸다
            var vel = Ball.GetComponent<Rigidbody>().velocity;
            var inverseVel = new Vector3(-vel.x, -Mathf.Abs(vel.y), -vel.z);

            Ball.GetComponent<Rigidbody>().velocity = inverseVel;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == Floor)
            IsLanded = false;
    }
    #endregion
}