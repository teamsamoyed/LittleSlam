using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goalpost : MonoBehaviour
{
    public int Team;
    public Vector3 GoalPos;

    public List<AudioClip> CheerSounds;

    AudioSource Source;

    void Start()
    {
        Source = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.Phase != GamePhase.InGame)
            return;

        var ball = other.GetComponent<Ball>();

        if (ball == null)
            return;

        var cheerIndex = Random.Range(0, CheerSounds.Count);
        var cheer = CheerSounds[cheerIndex];
        Source.PlayOneShot(cheer);

        GameManager.Instance.Score[(Team + 1) % 2] += ball.Score;
        GameManager.Instance.Phase = GamePhase.Wait;

        GameManager.Instance.OutlinePass(Team, GoalPos);
    }
}
