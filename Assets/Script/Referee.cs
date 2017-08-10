using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Referee : MonoBehaviour
{
    public float ThrowX;
    public float ThrowY;
    public float ThrowForce;
    GameObject Ball;

    public AudioClip ThrowSound;

    AudioSource Source;

    bool moveStart;

	// Use this for initialization
	void Start () {
        Source = GetComponent<AudioSource>();

        Ball = GameObject.FindGameObjectWithTag(Tags.Ball);

        Ball.SetActive(false);
        Ball.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

        StartCoroutine(StartAnimation());
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.z < 2.5)
        {
            if (moveStart)
            {
                transform.Translate(0.0f, 0.0f, Time.deltaTime);
            }
        }
        else
        {
            GetComponent<Animator>().SetTrigger("MoveEnd");
        }
	}

    IEnumerator StartAnimation()
    {
        yield return new WaitForSeconds(1.0f);

        GetComponent<Animator>().SetTrigger("Throw");
    }

    void ThrowBall()
    {
        Source.PlayOneShot(ThrowSound);
        StartCoroutine(BallGettingStart());
        GameManager.Instance.Phase = GamePhase.BallGetting;

        var startPosition = transform.position;
        startPosition.x += ThrowX;
        startPosition.y += ThrowY;

        Ball.SetActive(true);
        Ball.transform.position = startPosition;
        Ball.GetComponent<Rigidbody>().velocity = new Vector3(0.0f, ThrowForce, 0.0f);
    }

    void AnimationEnd()
    {
        moveStart = true;
    }

    IEnumerator BallGettingStart()
    {
        yield return new WaitForSeconds(1.0f);

        GameManager.Instance.Phase = GamePhase.BallGetting;
    }
}

