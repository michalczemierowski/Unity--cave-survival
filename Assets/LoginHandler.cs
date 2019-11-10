using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class LoginHandler : MonoBehaviour
{
    [Space] [Header("OBJECTS")] [Space]
    [SerializeField] private GameObject LoginCanvas;
    [SerializeField] private DatabaseConnection dbConnection;
    private MenuEventHandler menuEvent;

    [Space] [Header("LOGIN")] [Space]
    [SerializeField] private InputField lEmailField;
    [SerializeField] private InputField lPasswordField;

    [Space] [Header("REGISTER")] [Space]
    [SerializeField] private InputField rEmailField;
    [SerializeField] private InputField rPasswordField;
    [SerializeField] private InputField rUsernameField;


    void Start()
    {
        menuEvent = MenuEventHandler.Instance;
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            if (PlayerPrefs.HasKey(PlayerPrefKeys.playerID))
            {
                LoginWithCustomID();
                menuEvent.SetLoadingAnimActive(true);
            }
            else
                LoginCanvas.SetActive(true);
        }
    }

    #region Login
    public void Login()
    {
        //TODO: minimum password lenght
        string pass = lPasswordField.text;
        string email = lEmailField.text;

        if (email.Contains("@"))
            dbConnection.LoginEmail(email, pass);
        else
            dbConnection.LoginUsername(email, pass);

        menuEvent.SetLoadingAnimActive(true);
    }

    public void LoginSuccess(string playerID)
    {
        LoginCanvas.SetActive(false);
        dbConnection.GetStatistics();
        menuEvent.SetLoadingAnimActive(false);

        PlayerPrefs.SetString(PlayerPrefKeys.playerID, playerID);
        LinkCustomID(playerID);
    }

    public void LoginFailure(string error)
    {
        Debug.LogError("Login error: " + error);
        lPasswordField.text = null;

        menuEvent.SetLoadingAnimActive(false);
    }
    #endregion

    #region Register
    public void Register()
    {
        //TODO: minimum password & username lenght
        string pass = rPasswordField.text;
        string email = rEmailField.text;
        string username = rUsernameField.text;
        dbConnection.Register(email, pass, username);

        menuEvent.SetLoadingAnimActive(true);
    }

    public void RegisterSuccess(string playerID)
    {
        LoginCanvas.SetActive(false);
        menuEvent.SetLoadingAnimActive(false);

        PlayerPrefs.SetString(PlayerPrefKeys.playerID, playerID);
        LinkCustomID(playerID);
    }

    public void RegisterFailure(string error)
    {
        Debug.LogError("Register error: " + error);
        rPasswordField.text = null;

        menuEvent.SetLoadingAnimActive(false);
    }
    #endregion

    #region Login with custom ID
    private void LoginWithCustomID()
    {
        string ID = PlayerPrefs.GetString(PlayerPrefKeys.playerID);
        var loginRequest = new LoginWithCustomIDRequest { CustomId = ID, CreateAccount = false };
        PlayFabClientAPI.LoginWithCustomID(loginRequest, OnLoginWithIDSuccess, OnLoginWithIDFailure);
    }

    private void OnLoginWithIDSuccess(LoginResult result)
    {
        Debug.Log("Login with ID successfull");
        menuEvent.SetLoadingAnimActive(false);
        dbConnection.GetStatistics();
    }

    private void OnLoginWithIDFailure(PlayFabError error)
    {
        Debug.Log("Login with ID failure");
        menuEvent.SetLoadingAnimActive(false);
        LoginCanvas.SetActive(true);
    }
    #endregion

    #region Link custom ID
    private void LinkCustomID(string ID)
    {
        var linkRequest = new LinkCustomIDRequest { CustomId = ID, ForceLink = true };
        PlayFabClientAPI.LinkCustomID(linkRequest, OnLinkSuccess, OnLinkFailure);
    }

    private void OnLinkSuccess(LinkCustomIDResult result)
    {

    }

    private void OnLinkFailure(PlayFabError error)
    {

    }
    public void DeleteSession()
    {
        PlayerPrefs.DeleteKey(PlayerPrefKeys.playerID);
    }
    #endregion
}
