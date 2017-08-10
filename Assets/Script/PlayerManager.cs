using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    GameObject Ball;

    void Start()
    {
        Ball = GameObject.FindGameObjectWithTag(Tags.Ball);
    }

    // Update is called once per frame
    void Update ()
    {
        TabPlayer(0);
        TabPlayer(1);	
	}

    void TabPlayer(int Idx)
    {
        if (GameManager.Instance.Phase != GamePhase.InGame)
            return;

        if (!Input.GetButtonDown(Key.TabPlayer(Idx)))
            return;

        var owner = Ball.GetComponent<Ball>().Owner;
        if (owner != null &&
            owner.GetComponent<Player>().Team == Idx)
            return;

        //조작할 플레이어 변경하기
        var players = GameObject.FindGameObjectsWithTag(Tags.Player);

        GameObject nextPlayer = null;
        var minDistance = -99999.9f;

        foreach (var player in players)
        {
            if (player.GetComponent<Player>().Team != Idx)
                continue;

            if (player.GetComponent<Player>().IsPossessed)
            {
                player.GetComponent<Player>().IsPossessed = false;
                player.GetComponent<Player>().ChangeToAutoMove();
                continue;
            }

            float distance = player.transform.position.x - Ball.transform.position.x;

            if (player.GetComponent<Player>().Team == 0)
            {
                distance = -distance;
            }

            if (Mathf.Abs(distance) < Mathf.Abs(minDistance) ||
                (minDistance < 0 && distance > 0))
            {
                if (distance < 0 && minDistance > 0)
                    continue;

                minDistance = distance;
                nextPlayer = player;
            }
        }

        if (nextPlayer != null)
        {
            nextPlayer.GetComponent<Player>().IsPossessed = true;
        }
    }

    public static void InitAutoMove()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags.Player);

        foreach (var player in players)
        {
            player.GetComponent<Player>().ChangeToAutoMove();
        }
    }

    public static GameObject GetPlayer(int Team, int Index)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags.Player);

        foreach (var player in players)
        {
            var playerComponent = player.GetComponent<Player>();

            if (playerComponent.Team == Team && playerComponent.Index == Index)
                return player;
        }

        return null;
    }
}
