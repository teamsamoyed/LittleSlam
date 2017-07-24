using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goalpost : MonoBehaviour
{
    public int Team;
    public Vector3 GoalPos;

    void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.Phase != GamePhase.InGame)
            return;

        GameManager.Instance.Score[(Team + 1) % 2] += 2;
        GameManager.Instance.Phase = GamePhase.Wait;

        StartCoroutine(GameManager.Instance.ToOutlinePass(Team, GoalPos));
    }
}
