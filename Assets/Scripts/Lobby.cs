using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    public static Lobby instance = null;

    public Text playerCount;

    public Text userName;

    private void OnEnable()
    {
        GameManager.playerInfo += PlayerInfo;
    
    }
    private void OnDisable()
    {
        GameManager.playerInfo -= PlayerInfo;
        DataCommunicateServer.loginCompleteEvent -= GetLoginInformation;
    }

    private void GetLoginInformation(string loginUser)
    {
        userName.text = loginUser;
    }

   
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    private void PlayerInfo(Dictionary<int, PlayerManager> playerInfo)
    {
        playerCount.text=playerInfo.Count.ToString();
    }
}
