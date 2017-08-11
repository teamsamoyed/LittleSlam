using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerState
{
    Move,
    Shoot,
    Dunk,
    Layup,
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
    public float DunkDistance;
    public float LayupDistance;
    // goalpoint 기준 상대 좌표
    public Vector3 DunkFrontPos;
    public Vector3 DunkBackPos;
    public Vector3 DunkLeftPos;
    public Vector3 DunkRightPos;
    public float PassTime;
    public float BlockJump;
    public float BlockPower;
    public float BlockHead;
    public float PasslineRange;

    GameObject Ball;
    GameObject Floor;
    Animator Ani;
    bool IsLanded = true;
    bool BlockInput = false;
    float BlockTime = 0.0f;
    //보는 방향에서 이 각도 안 쪽일 때만 그 선수 겨냥해서 패스
    //그 외의 경우 그냥 해당 방향 직선으로 쏴버린다
    public float MaxPassDegree;
    public float MinPassSpeed;
    public float ShootMaxHoldTime;
    public float StealRange;
    public float BlockRange;

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

    public AudioClip BounceSound;
    public AudioClip ShootSound;

    GameObject Indicator;

    AudioSource Source;

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
        Source = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {

        if (transform.position.y > 0.2f)
        {
            if (Ani.GetBool("Front"))
                GetComponent<SpriteRenderer>().sortingOrder = 0;
            else if (Ani.GetBool("Back"))
                GetComponent<SpriteRenderer>().sortingOrder = 2;
        }
        else
        {
            GetComponent<SpriteRenderer>().sortingOrder = 1;
        }

        Indicator.SetActive(IsPossessed);

        Ani.SetBool("Landing", IsLanded);
        Ani.SetBool("OwnBall", Ball.GetComponent<Ball>().Owner == gameObject);

        if (IsBlock && transform.position.y > 0.2f && Body.velocity.y < 0.0f)
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
                BallGettingUpdate();
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

    void Bounce()
    {
        Source.PlayOneShot(BounceSound);
    }

    #region BallGetting

    void BallGettingUpdate()
    {
        if (!IsPossessed)
            return;

        switch (State)
        {
            case PlayerState.Move:
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
                break;
            case PlayerState.Block:
                BlockControl();
                BallGettingBlockUpdate();
                break;
        }
    }

    void BallGettingBlockUpdate()
    {
        if (Ball.transform.position.y  < transform.position.y + BlockHead)
        {
            Ball.GetComponent<Ball>().TouchCount++;
            //아군 쪽으로 패스
            //인덱스 1 or 2로 보낸다.
            Source.PlayOneShot(BounceSound);
            Ani.SetTrigger("BlockEnd");
            GameManager.Instance.Phase = GamePhase.InGame;

            int goalIndex = Random.Range(1, 3);

            GameObject goal = null;

            var players = GameObject.FindGameObjectsWithTag(Tags.Player);

            foreach (var player in players)
            {
                player.GetComponent<Player>().BlockInput = true;
                player.GetComponent<Player>().BlockTime = 0.5f;

                if (player.GetComponent<Player>().Team == Team &&
                    player.GetComponent<Player>().Index == goalIndex)
                {
                    goal = player;
                }
            }

            var dir = goal.transform.position - Ball.transform.position;
            dir.y += BlockHead;
            dir.Normalize();
            dir *= 2;

            Ball.GetComponent<Rigidbody>().velocity = dir;

            IsPossessed = false;
            goal.GetComponent<Player>().IsPossessed = true;
        }
    }

    #endregion

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
            case PlayerState.Dunk:
                DunkControl();
                break;
            case PlayerState.Layup:
                LayupControl();
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
            var goal = Goalpost.GetEnemy(Team);
            var goalPos = goal.transform.position;
            goalPos.y = 0;
            var startPos = transform.position;
            startPos.y = 0;
            var distance = Vector3.Distance(goalPos, startPos);

            if (distance < DunkDistance)
            {
                DunkStart(goal);
            }
            else if (distance < LayupDistance)
            {
                LayupStart(goal);
            }
            else
            {
                ShootStart(goal);
            }
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
        Body.AddForce(0.0f, BlockJump, 0.0f);

        if (GameManager.Instance.Phase == GamePhase.InGame)
        {
            var dir = Ball.transform.position - transform.position;
            dir.y = 0.0f;

            if (dir.magnitude > 0.5f)
            {
                dir.Normalize();
                dir *= 0.5f;
            }

            Body.velocity = new Vector3(dir.x, Body.velocity.y, dir.z);
        }
    }

    void BlockEnd()
    {
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
            IsBlock = false;
            IsBlockMotionEnded = false;
        }

        if (GameManager.Instance.Phase == GamePhase.InGame && IsBlock)
        {
            if (Ball.GetComponent<Ball>().Owner == null &&
                Ball.GetComponent<Ball>().TouchCount == 0)
            {
                var distance = Ball.transform.position - transform.position;

                Debug.Log(distance.magnitude);
                if (distance.magnitude < BlockRange)
                {
                    Source.PlayOneShot(BounceSound);
                    //공을 날아오는 반대편으로 쳐낸다
                    var vel = Ball.GetComponent<Rigidbody>().velocity;
                    var inverseVel = new Vector3(-vel.x, 0.0f, -vel.z);

                    if (GetComponent<SpriteRenderer>().flipX)
                    {
                        inverseVel.x -= BlockPower;
                    }
                    else
                    {
                        inverseVel.x += BlockPower;
                    }

                    Ball.GetComponent<Rigidbody>().velocity = inverseVel;
                    IsBlock = false;
                }
            }
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
        Vector3 nowPos = transform.position;

        if (nowPos.x <= -XCut || nowPos.x >= XCut ||
            nowPos.z <= -ZCut || nowPos.z >= ZCut)
            return true; // 이미 필드 바깥으로 나가있는 경우 다시 안으로 들어올 수 있게 해주기 위함

        Vector3 CheckPosition = transform.position;
        Vector3 size = Collider.size * 0.5f;
        CheckPosition.y += 0.35f;
        CheckPosition.x += Direction.x * size.x;
        CheckPosition.z += Direction.z * size.z;

        if (!IsAutoMove)
        {
            if (CheckPosition.x <= -XCut || CheckPosition.x >= XCut ||
                CheckPosition.z <= -ZCut || CheckPosition.z >= ZCut)
                return false;
        }

        size.y = 0.3f;
        /*
        var overlapped = Physics.OverlapBox(CheckPosition, size);

        foreach (var o in overlapped)
        {
            if (o.gameObject == gameObject)
                continue;

            if (o.gameObject == Ball)
                continue;

            var player = o.gameObject.GetComponent<Player>();

            if (!IsAutoMove || (player != null && player.IsPossessed))
                return false;
        }*/

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

        if (goal == null)
            return;

        PassGoal = goal;

        Vector3 end = new Vector3();

        if (goal != null)
        {
            PassStart = transform.position;
            end = goal.transform.position;
            var dir = end - PassStart;
            dir.Normalize();
            PassStart += dir * 0.15f;
            PassStart.y += 0.16f;
            end.y += 0.16f;
        }

        Ani.SetBool("Front", false);
        Ani.SetBool("Back", false);

        PassVelocity = (end - PassStart) / PassTime - Physics.gravity * PassTime * 0.5f;

        float time = PassTime;

        if (PassVelocity.magnitude < MinPassSpeed)
        {
            var rate = MinPassSpeed / PassVelocity.magnitude;

            var fastTime = PassTime / rate;
            time = fastTime;

            PassVelocity = (end - PassStart) / fastTime - Physics.gravity * fastTime * 0.5f;
        }

        else if (PassVelocity.x > 0.0f)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }

        Ani.SetTrigger("Pass");

        IsAction = true;
        //아군은 pass time동안은 못 움직임
        var players = GameObject.FindGameObjectsWithTag(Tags.Player);

        foreach (var player in players)
        {
            if (player.GetComponent<Player>().Team != Team)
                continue;

            player.GetComponent<Player>().BlockInput = true;
            player.GetComponent<Player>().BlockTime = time + 0.26f;
        }

    }

    void PassEnd()
    {
        Source.PlayOneShot(ShootSound);
        IsAction = false;
        Ball.SetActive(true);

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

    void DunkControl()
    {
        //최고점 찍음 - 이 때 내려온다
        if (WaitShootRelease && IsShootMotionEnded &&
            transform.position.y > 0.2f &&
            Body.velocity.y < 0.0f)
        {
            Ani.SetTrigger("ShootEnd");
            WaitShootRelease = false;

            if (Ball.GetComponent<Ball>().Owner != gameObject)
                return;

            //공도 여기서부터 등장
            var goal = Goalpost.GetEnemy(Team);
            
            Ball.SetActive(true);
            var ballPos = goal.transform.position;
            ballPos.y += 0.05f;

            if (Team == 0)
            {
                ballPos.x -= 0.04f;
            }
            else
            {
                ballPos.x += 0.04f;
            }

            Ball.transform.position = ballPos;
            Ball.GetComponent<Ball>().Owner = null;
            Ball.GetComponent<Ball>().Score = 2;
            Ball.GetComponent<Rigidbody>().velocity = new Vector3(0.0f, -0.1f, 0.0f);
        }

        if (!WaitShootRelease && IsShootMotionEnded && IsLanded)
        {
            Ani.ResetTrigger("Dunk");
            State = PlayerState.Move;
        }
    }

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
                Ani.ResetTrigger("Shoot");
                Ani.ResetTrigger("ShootEnd");
                State = PlayerState.Move;
            }
        }
    }

    void LayupControl()
    {
        if (IsShootMotionEnded && WaitShootRelease)
        {
            WaitShootRelease = false;
            ShootImpulse();
        }

        if (!WaitShootRelease && IsShootMotionEnded)
        {
            if (IsLanded)
            {
                Ani.ResetTrigger("Layup");
                State = PlayerState.Move;
            }
        }
    }

    void ShootStart(GameObject goal)
    {
        var startPos = transform.position;

        State = PlayerState.Shoot;
        WaitShootRelease = true;
        IsShootMotionEnded = false;
        ShootHoldTime = 0.0f;

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

    void DunkStart(GameObject goal)
    {
        Ani.ResetTrigger("Dunk");

        State = PlayerState.Dunk;
        WaitShootRelease = true;
        IsShootMotionEnded = false;

        Ani.SetBool("Front", false);
        Ani.SetBool("Back", false);
        GetComponent<SpriteRenderer>().flipX = false;

        float dx = goal.transform.position.x - transform.position.x;
        float dz = goal.transform.position.z - transform.position.z;

        if (Mathf.Abs(dx) > Mathf.Abs(dz))
        {
            if (dx < 0)
                GetComponent<SpriteRenderer>().flipX = true;
            else
                GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            if (dz < 0)
                Ani.SetBool("Front", true);
            else
                Ani.SetBool("Back", true);

            if(Team == 1)
                GetComponent<SpriteRenderer>().flipX = true;
        }

        Ani.SetTrigger("Dunk");
    }

    void LayupStart(GameObject goal)
    {
        State = PlayerState.Layup;
        WaitShootRelease = true;
        IsShootMotionEnded = false;

        Ani.SetBool("Front", false);
        Ani.SetBool("Back", false);
        GetComponent<SpriteRenderer>().flipX = false;

        float dx = goal.transform.position.x - transform.position.x;
        float dz = goal.transform.position.z - transform.position.z;

        if (dx < 0)
            GetComponent<SpriteRenderer>().flipX = true;
        else
            GetComponent<SpriteRenderer>().flipX = false;

        Body.velocity = new Vector3(dx, 0.0f, dz);
        Body.AddForce(0.0f, Jump, 0.0f);

        Ani.SetTrigger("Layup");
    }

    void LayupRelease()
    {
        IsShootMotionEnded = true;
    }

    void ReadyShoot()
    {
        Body.AddForce(0.0f, Jump, 0.0f);
        IsShootMotionEnded = true;
    }

    void ReadyDunk()
    {
        //Body.AddForce(0.0f, DunkJump, 0.0f);
        var goal = Goalpost.GetEnemy(Team);
        var goalPos = goal.transform.position;
        Vector3 vel = new Vector3();

        if (Ani.GetBool("Front"))
        {
            goalPos += DunkFrontPos;
        }
        else if (Ani.GetBool("Back"))
        {
            goalPos += DunkBackPos;
        }
        else
        {
            if (Team == 0)
            {
                goalPos += DunkLeftPos;
            }
            else
            {
                goalPos += DunkRightPos;
            }
        }

        goalPos -= goal.transform.localPosition;

        var start = transform.position;
        var dir = goalPos - start;

        vel.y = Mathf.Sqrt(-2 * Physics.gravity.y * dir.y);

        var time = -vel.y / Physics.gravity.y;

        vel.x = dir.x / time;
        vel.z = dir.z / time;

        Body.velocity = vel;
        IsShootMotionEnded = true;
    }

    void ShootImpulse()
    {
        if (Ball.GetComponent<Ball>().Owner != gameObject)
            return;

        Source.PlayOneShot(ShootSound);
        
        GameObject goal = Goalpost.GetEnemy(Team);

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

        var d = Random.Range(1.0f - dist * 0.01f, 1.0f + dist * 0.01f);
        d = Mathf.Clamp(d, 0.5f, 1.5f);
        var yvel = Body.velocity.y * d;

        yvel = Mathf.Clamp(yvel, -0.2f, 0.2f);
        yvel = Mathf.Abs(yvel);

        if (State != PlayerState.Layup)
        {
            endPos.x += Random.Range(-yvel, yvel);
            endPos.y += Random.Range(-yvel, yvel);
            endPos.z += Random.Range(-yvel, yvel);
        }

        var dir = (endPos - startPos) / ShootTime - 0.5f * Physics.gravity * ShootTime;
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
        if (!IsLanded)
        {
            return;
        }

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
        if(Team == OwnTeam)
            IsPossessed = false;

        if (Team == OwnTeam && Index == 0)
        {
            Ani.SetBool("Running", false);
            Ball.SetActive(false);
            Ball.GetComponent<Ball>().Owner = gameObject;
            transform.position = Pos;

            Ani.SetBool("Front", false);
            Ani.SetBool("Back", false);

            if (Pos.z >= Ball.GetComponent<Ball>().ZCut)
            {
                Ani.SetBool("Front", true);

                GetComponent<SpriteRenderer>().flipX = false;
            }
            else if (Pos.z <= -Ball.GetComponent<Ball>().ZCut)
            {
                Ani.SetBool("Back", true);

                GetComponent<SpriteRenderer>().flipX = false;
            }
            else if (Pos.x < 0.0f)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
        }
        else 
        {
            if (Index == 1 && Team == OwnTeam)
            {
                IsPossessed = true;
            }

            var box = GetComponent<BoxCollider>();
            Collider[] overlapped;
 
            do
            {
                ToPasslineRandomPos(Pos);
                var center = box.center + transform.position;
                overlapped = Physics.OverlapBox(center, box.size);
            } while (overlapped.Length > 2);
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

        if (GameManager.Instance.Phase == GamePhase.InGame || GameManager.Instance.Phase == GamePhase.OutlinePass)
        {
            if ((State == PlayerState.Move && !IsAction) ||
                Ball.GetComponent<Ball>().TouchCount >= 1)
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
                Source.PlayOneShot(BounceSound);
                //공을 날아오는 반대편으로 쳐낸다
                var vel = Ball.GetComponent<Rigidbody>().velocity;
                var inverseVel = new Vector3(-vel.x, 0.0f, -vel.z);

                if (GetComponent<SpriteRenderer>().flipX)
                {
                    inverseVel.x -= BlockPower;
                }
                else
                {
                    inverseVel.x += BlockPower;
                }

                Ball.GetComponent<Rigidbody>().velocity = inverseVel;
                IsBlock = false;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == Floor)
            IsLanded = false;
    }
    #endregion

    void ToPasslineRandomPos(Vector3 Pos)
    {
        float minX, maxX;
        float minZ, maxZ;

        //적절히 근처 랜덤한 위치로 보내기
        if (Pos.z >= Ball.GetComponent<Ball>().ZCut)
        {
            minX = transform.position.x - PasslineRange * 0.5f;
            maxX = transform.position.x + PasslineRange * 0.5f;

            minZ = Ball.GetComponent<Ball>().ZCut - PasslineRange;
            maxZ = Ball.GetComponent<Ball>().ZCut - 0.3f;
        }
        else if (Pos.z <= -Ball.GetComponent<Ball>().ZCut)
        {
            minX = transform.position.x - PasslineRange * 0.5f;
            maxX = transform.position.x + PasslineRange * 0.5f;

            minZ = -Ball.GetComponent<Ball>().ZCut + 0.3f;
            maxZ = -Ball.GetComponent<Ball>().ZCut + PasslineRange;
        }
        else if (Pos.x < 0.0f)
        {
            minX = -Ball.GetComponent<Ball>().XCut + 0.3f;
            maxX = -Ball.GetComponent<Ball>().XCut + PasslineRange;

            minZ = transform.position.z - PasslineRange;
            maxZ = transform.position.z + PasslineRange;
        }
        else
        {
            minX = Ball.GetComponent<Ball>().XCut - PasslineRange;
            maxX = Ball.GetComponent<Ball>().XCut - 0.3f;

            minZ = transform.position.z - PasslineRange;
            maxZ = transform.position.z + PasslineRange;
        }

        minX = Mathf.Max(minX, -Ball.GetComponent<Ball>().XCut);
        maxX = Mathf.Min(maxX, Ball.GetComponent<Ball>().XCut);
        minZ = Mathf.Max(minZ, -Ball.GetComponent<Ball>().ZCut);
        maxZ = Mathf.Min(maxZ, Ball.GetComponent<Ball>().ZCut);

        Vector3 newPosition = transform.position;
        newPosition.x = Random.Range(minX, maxX);
        newPosition.z = Random.Range(minZ, maxZ);

        transform.position = newPosition;
    }
}