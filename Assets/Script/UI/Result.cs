using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Result : MonoBehaviour
{
    public GameObject Player1Score;
    public GameObject Player2Score;

    public Sprite P1Win;
    public Sprite P2Win;
    public Sprite Draw;

	void Update ()
    {
        Player1Score.GetComponent<Text>().text = GameManager.Instance.Score[0].ToString();
        Player2Score.GetComponent<Text>().text = GameManager.Instance.Score[1].ToString();

        if (GameManager.Instance.Score[0] > GameManager.Instance.Score[1])
        {
            GetComponent<Image>().sprite = P1Win;
        }
        else if (GameManager.Instance.Score[0] == GameManager.Instance.Score[1])
        {
            GetComponent<Image>().sprite = Draw;
        }
        else
        {
            GetComponent<Image>().sprite = P2Win;
        }
    }
}
