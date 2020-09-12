﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;
    
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<Player>().OnDead += OnGameOver;
    }

    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, Color.black, 1, gameOverUI,true));
    }

    IEnumerator Fade(Color from, Color to, float time, GameObject UiObject, bool isFadeOut)
    {
        float speed = 1 / time;
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
        UiObject.SetActive(isFadeOut);
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene("MainGame");
    }
}
