using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goalpost : MonoBehaviour
{
    public int Team;

    void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.Phase != GamePhase.InGame)
            return;

        GameManager.Instance.Phase = GamePhase.OutlinePass;
        //if (other.tag == Tags.Ball)
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
