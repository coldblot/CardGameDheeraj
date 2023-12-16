using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public InputField usernameField;


    public Button loginButton;
    public Button registerButton;


    public GameObject loginForm;
    public GameObject afterLoginForm;


    public GameObject loginMessage;

    private void OnEnable()
    {
        DataCommunicateServer.loginCompleteEvent += OnLoginComplete;
        DataCommunicateServer.logoutCompleteEvent += OnLogoutComplete;
    }

    private void OnDisable()
    {
        DataCommunicateServer.loginCompleteEvent -= OnLoginComplete;
        DataCommunicateServer.logoutCompleteEvent -= OnLogoutComplete;
    }

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
        loginMessage.SetActive(false);
    }

    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
    }

    private void OnLoginComplete(string username)
    {
        Invoke("DelayForMessage", 2f);
        loginMessage.SetActive(true);  
    }
    private void DelayForMessage()
    {
        loginForm.SetActive(false);
        afterLoginForm.SetActive(true);
    }
    private void OnLogoutComplete()
    {
        loginForm.SetActive(true);
        afterLoginForm.SetActive(false);
        loginMessage.SetActive(false);
    }
}
