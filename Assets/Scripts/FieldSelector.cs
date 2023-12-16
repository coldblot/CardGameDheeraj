using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


namespace FormCreatorWebRequest
{
    public enum States { UserLogin, UserRegister };
    public enum FieldState { Name, Username, Password, ConfirmPassword };
    public enum Result { Failure,Success };
    public enum RequestType { POST, GET, DELETE, PUT };

    class FieldSelector
    {
        private readonly States state;
        private string userlocalExtension;

        private RequestType requesttype;
        /// <summary>
        /// Check if the web request results in success or failure
        /// </summary>
        public Result requestStatus;
        public string GetRoute
        {
            get
            {
                return userlocalExtension;
            }
        }

        
        private string setRequest;
        private readonly string url;
        
        private WWWForm form;

        private int port;

        private UserLogin userlogin;
        private UserRegister userRegister;

        private string[] fields;
        private string[] values;

        private bool autoFieldSet;
        public class UserLogin
        {
            public string username;
            public string password;
        }
        public class UserRegister
        {
            public string name;
            public string username;
            public string password;
            public string confirmpassword;
        }
        public FieldSelector(string url, States state, int port,string route)
        {
            autoFieldSet = true;
            this.state = state;
            this.url = url + $":{port}";
            this.port = port;
            this.userlocalExtension = route;
            InitializeFields();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="state"></param>
        /// <param name="port">
        /// If port not available or not working on local host then write -1 in port column
        /// </param>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <param name="route"></param>
        public FieldSelector(string url, States state, int port, string[] fields, string[] values,string route)
        {
            if (fields.Length != values.Length)
            {
                Debug.LogError("fields values are not equivalent to values!");
                return;
            }
            autoFieldSet = false;
            this.state = state;

            if (port > 0)
                this.url = url + $":{port}";
            else
                this.url = url;


            this.port = port;
            this.fields = fields;
            this.values = values;
            this.userlocalExtension = route;
            InitializeFields();
            ManualFieldSet();
        }
        private void InitializeFields()
        {
            form = new WWWForm();
            userRegister = new UserRegister();
            userlogin = new UserLogin();
            fetchData = new byte[0];
        }
        public void SetField(FieldState fieldState, string value)
        {
            if (!autoFieldSet)
                return;
            switch (fieldState)
            {
                case FieldState.Name:
                    form.AddField("name", value);
                    userRegister.name = value;
                    break;
                case FieldState.Username:
                    form.AddField("username", value);
                    userlogin.username = value;
                    userRegister.username = value;
                    break;
                case FieldState.Password:
                    form.AddField("password", value);
                    userlogin.password = value;
                    userRegister.password = value;
                    break;
                case FieldState.ConfirmPassword:
                    form.AddField("confirmpassword", value);
                    userRegister.confirmpassword = value;
                    break;
            }
        }
        private void ManualFieldSet()
        {
            for (int i = 0; i < fields.Length; i++)
            {
                form.AddField(fields[i].ToLower(), values[i].ToLower());
            }
        }
        private string data; 
        private UnityWebRequest WebRequest()
        {
            try
            {
                switch (requesttype)
                {
                    case RequestType.POST:
                        return UnityWebRequest.Post(url + $"/{userlocalExtension}", form);
                    case RequestType.GET:
                        return UnityWebRequest.Get(url + $"/{userlocalExtension}");
                    case RequestType.PUT:
                        return UnityWebRequest.Put(url + $"/{userlocalExtension}",data);
                    case RequestType.DELETE:
                        return UnityWebRequest.Delete(url + $"/{userlocalExtension}");
                }
            }
            catch
            {
                Debug.LogError("Field value not set or fields not initialize");
            }
            return null;
        }

        private byte[] fetchData;

        public byte[] FetchData()
        {
            if (fetchData.Length > 0)
            { return fetchData; }
            else
            { return null; }
        }
        public async Task SendWebRequest(RequestType type)
        {
            this.requesttype = type;
            if (autoFieldSet)
            {
                switch (state)
                {
                    case States.UserLogin:
                        if (userlogin.username == "" || userlogin.password == "" || userlogin.password == null || userlogin.password == null)
                        { Debug.LogError("Set all the fields!"); return; }
                        break;
                    case States.UserRegister:
                        if (userRegister.name == "" || userRegister.name == null || userRegister.username == "" || userRegister.username == null || userRegister.confirmpassword == "" || userRegister.confirmpassword == null || userRegister.password == "" || userRegister.password == null)
                        {
                            Debug.LogError("Set all the fields!");
                            return;
                        }
                        break;
                }
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] == "" || values[i] == null)
                    {
                        Debug.LogError("Values can't be null or empty");
                        return;
                    }
                }
            }

            var www = WebRequest();
            var task =  www.SendWebRequest();

            while (!task.isDone)
            {
                await Task.Delay(100);
            }
            if (www.result == UnityWebRequest.Result.Success)
            {
                requestStatus = Result.Success;
                fetchData = www.downloadHandler.data;
            }
            else
            {
                requestStatus = Result.Failure;
                Debug.LogError("Connection not working");
            }
            www.Dispose();
        }
        public void AddField(string fieldName, string value)
        {
            form.AddField(fieldName, value);
        }
    }
}


