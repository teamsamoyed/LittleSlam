using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    private void Update()
    {
        var text = GameManager.Instance.Score[0] + " : " +
            GameManager.Instance.Score[1];
        GetComponent<Text>().text = text;
    }
}
