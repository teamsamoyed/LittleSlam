using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GamePhase
{
    BallGetting, // 처음에 양쪽 점프 뛰어서 공 잡기
    InGame, //인 게임 진행
    OutlinePass, //골 넣고 나서 or 라인 밖으로 공 나가서 아군한테 패스
    FreeDraw, //자유투
    GameEnd, // 게임 종료 결과
    Wait //잠깐 대기
}

public class GameManager : MonoBehaviour
{
    public GamePhase phase = GamePhase.InGame;

    public int[] Score = new int[2];
    public float TotalGameTime;
    float RemainGameTime;
    public GamePhase Phase
    {
        get
        {
            return phase;
        }

        set
        {
            if (phase == value)
                return;

            phase = value;
        }
    }

    public static GameManager Instance;

    GameObject Ball;

	// Use this for initialization
	void Awake ()
    {
        Instance = this;
	}

    void Start()
    {
        RemainGameTime = TotalGameTime;
    }
	
	// Update is called once per frame
	void Update ()
    {
        RemainGameTime -= Time.deltaTime;

        if (RemainGameTime <= 0.0f)
        {
            Phase = GamePhase.GameEnd;
        }
	}

    IEnumerator Restart()
    {
        yield return new WaitForSeconds(2.0f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public IEnumerator ToOutlinePass(int OwnTeam, Vector3 Pos)
    {
        yield return new WaitForSeconds(2.0f);

        //모든 플레이어들 Outline Pass 기준으로 동작하게 해준다
        var players = GameObject.FindGameObjectsWithTag(Tags.Player);

        foreach (var player in players)
        {
            player.GetComponent<Player>().ToOutlinePos(OwnTeam, Pos);
        }

        Phase = GamePhase.OutlinePass;
    }
}
