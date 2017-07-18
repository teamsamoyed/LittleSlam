using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
