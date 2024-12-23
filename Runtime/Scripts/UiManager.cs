using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    public GameObject Board;
    public bool ison;

    public void PlayAgain()
    {
        SceneManager.LoadScene(0);
    }

    public void ToggleBoard()
    {
        ison = !ison;
        Board.SetActive(ison);
    }
}
