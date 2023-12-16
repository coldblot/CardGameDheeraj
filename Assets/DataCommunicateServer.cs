using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using FormCreatorWebRequest;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft;
using Newtonsoft.Json;

public class DataCommunicateServer : MonoBehaviour
{
    FieldSelector selector;

    [Header("Register Fields")]
    public InputField name;
    public InputField username;
    public InputField email;
    public InputField password;
    public InputField confirmpassword;

    public Button register;

    [Header("Login Fields")]
    public InputField usernamelogin;
    public InputField passwordlogin;

    public Button login;

    public Text loginErrorMessage;
    public GameObject logoutButton;
    //public GameObject loginForm;
    public List<string> error;

    public delegate void LoginComplete(string loginUser);
    public static event LoginComplete loginCompleteEvent;

    public delegate void LogoutComplete();
    public static event LogoutComplete logoutCompleteEvent;


    private void OnEnable()
    {
        loginCompleteEvent += LoginCompleteFunction;
        logoutCompleteEvent += LogoutCompleteFunction;
    }
    private void OnDisable()
    {
        loginCompleteEvent -= LoginCompleteFunction;
        logoutCompleteEvent -= LogoutCompleteFunction;
    }
    private List<string> storedErrors = new List<string>
    {
        "Name field can't be empty!",
        "Username field can't be empty!",
        "Password field can't be empty!",
        "Email field can't be empty!",
        "ConfirmPassword field can't be empty!",
        "Password not match!",
        "Please make strong password atleast 8 characters",
        "Please enter valid email!"
    };
    public string serverError;

    private void Awake()
    {
        loginErrorMessage.text = serverError;      
    }
    public async void Register()
    {
        error.Clear();
        serverMessage = String.Empty;
        if (name.text == "" || name.text == null)
        { error.Add("Name field can't be empty!"); }
        if (username.text == "" || username.text == null)
        { error.Add("Username field can't be empty!"); }
        if (password.text == "" || password.text == null)
        { error.Add("Password field can't be empty!"); }
        if (email.text == "" || email.text == null)
        { error.Add("Email field can't be empty!"); }
        if (confirmpassword.text == "" || confirmpassword.text == null)
        { error.Add("ConfirmPassword field can't be empty!");}

        
        if (password.text != confirmpassword.text)
        {
            error.Add("Password not match!");
        }
        if(password.text.Length<8)
        {
                error.Add("Please make strong password atleast 8 characters");
        }
        if(!Regex.IsMatch(email.text, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
        {
                error.Add("Please enter valid email!");
        }
        if (error.Count > 0)
        {
            return; 
        }
        error.Clear();

        selector = new FieldSelector(onlineUrl, States.UserRegister, -1, new string[] { "name", "email","username","password","confirmpassword" }, new string[] { name.text, email.text, username.text,password.text,confirmpassword.text },"user/register");

        await selector.SendWebRequest(RequestType.POST);

        var fectchData = selector.FetchData();


        var encodeData = Encoding.UTF7.GetString(fectchData);
        Dictionary<string,object>data = JsonConvert.DeserializeObject<Dictionary<string,object>>(encodeData);

        if(data.ContainsKey("error"))
        {
            if(data.ContainsKey("field"))
            {
                loginErrorMessage.text = $"{data["field"]} is already present!Try newone";
            }
            else
            {
                loginErrorMessage.text = $"{data["error"]}";
            }
            data.Clear();
            return;
        }

        if (selector.requestStatus==Result.Success)
        {
            Debug.Log("Registered the data");
        }
        else
        {
            Debug.Log("Unable to register");
        }
    }
    public string serverMessage;
    public async void Login()
    {
        error.Clear();
        if (usernamelogin.text == "" || usernamelogin.text == null)
        { error.Add("Username field can't be empty!"); return; }
        if (passwordlogin.text == "" || passwordlogin.text == null)
        { error.Add("Password field can't be empty!"); return; }

        if (error.Count > 0)
            return;
        error.Clear();

        selector = new FieldSelector(onlineUrl, States.UserLogin, -1, new string[] {"username", "password"}, new string[] { usernamelogin.text, passwordlogin.text },"user/login");

        await selector.SendWebRequest(RequestType.POST);

        var fetch = selector.FetchData();

        var encodeData = Encoding.UTF7.GetString(fetch);

        Dictionary<string, object> deserializeData = JsonConvert.DeserializeObject<Dictionary<string, object>>(encodeData);

        if (deserializeData.ContainsKey("error"))
        {
            loginErrorMessage.text = (string)deserializeData["error"];
            return;
        }

        var token = (string)deserializeData["token"];

        PlayerPrefs.SetString("token", token);

        if (selector.requestStatus == Result.Success)
        {
            StartCoroutine(StartVerifyToken());
        }
        else
        {
            Debug.LogError("Login Failed!");
        }
        error.Clear();
    }

    private void CheckUserVerification()
    {
        StartCoroutine(StartVerifyToken());
    }
    public void ConnectToServer()
    {
        Client.instance.ConnectToServer();
    }
    public void Logout()
    {
        logoutCompleteEvent();
    }
    private void LogoutCompleteFunction()
    {
        PlayerPrefs.SetString("token", "");
        Client.instance.TCPUDPDisconnect();
        GameManager.playerInfo(GameManager.players);
    }
    string onlineUrl = "https://unityauthenticationnodejsserver.onrender.com";
    string localUrl = "http://localhost";
    int onlinePort = 10000;
    int localPort = 23001;

    private IEnumerator StartVerifyToken()
    {
       
        var www = UnityWebRequest.Get($"{onlineUrl}/user/verify?token={PlayerPrefs.GetString("token")}");

        yield return www.SendWebRequest();

        var fetch = www.downloadHandler.data;

        var encodeData = Encoding.UTF7.GetString(fetch);

        Dictionary<string, object> deserializeData = JsonConvert.DeserializeObject<Dictionary<string, object>>(encodeData);

        if(deserializeData.ContainsKey("error"))
        {
            loginErrorMessage.text = (string)deserializeData["error"];
            if(deserializeData.ContainsKey("type"))
            {
                loginErrorMessage.text = loginErrorMessage.text + ":"+(string)deserializeData["type"];
            }
            yield break;
        }

      
        if (www.result==UnityWebRequest.Result.Success)
        {
            SuccessfullLogin(deserializeData);
            loginErrorMessage.text = string.Empty;
        }
        else
        {
            loginErrorMessage.text = "Unable to contact server!";
        }
    }
    public static string nameOfUser;

    private void SuccessfullLogin(Dictionary<string,object> data)
    {
        nameOfUser = (string)data["name"];
        loginCompleteEvent(nameOfUser);
    }

    private void LoginCompleteFunction(string username)
    {
        ConnectToServer();
    }

}

