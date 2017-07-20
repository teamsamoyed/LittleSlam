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
    GameEnd // 게임 종료 결과
}

public class GameManager : MonoBehaviour
{
    GamePhase phase = GamePhase.InGame;

    public int[] Score = new int[2];
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

            if (phase == GamePhase.OutlinePass)
            {
                //TODO : 일단 씬 재시작
                StartCoroutine(Restart());
            }
        }
    }

    public static GameManager Instance;

    GameObject Ball;

	// Use this for initialization
	void Awake ()
    {
        Instance = this;
	}
	
	// Update is called once per frame
	void Update ()
    {
	}

    IEnumerator Restart()
    {
        yield return new WaitForSeconds(2.0f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
