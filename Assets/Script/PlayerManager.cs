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
        if (!Input.GetButtonDown(Key.TabPlayer(Idx)))
            return;

        //조작할 플레이어 변경하기
        var players = GameObject.FindGameObjectsWithTag(Tags.Player);

        GameObject nextPlayer = null;
        var minDistance = 999999.9f;

        foreach (var player in players)
        {
            if (player.GetComponent<Player>().Team != Idx)
                continue;

            if (player.GetComponent<Player>().IsPossessed)
            {
                player.GetComponent<Player>().IsPossessed = false;
                continue;
            }

            var distance = Vector3.Distance(player.transform.position, Ball.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nextPlayer = player;
            }
        }

        if (nextPlayer != null)
        {
            nextPlayer.GetComponent<Player>().IsPossessed = true;
        }
    }
}
