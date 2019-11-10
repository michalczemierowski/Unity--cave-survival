using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseConnection : MonoBehaviour
{
    [SerializeField] private LoginHandler loginHandler;
    private StatsManager statsManager;

    protected string email, password, username;
    public void Start()
    {
        statsManager = StatsManager.Instance;
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            PlayFabSettings.staticSettings.TitleId = "78D05";
        }
    }

    public void LoginEmail(string email, string pass)
    {
        this.email = email;
        this.password = pass;
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            var request = new LoginWithEmailAddressRequest { Email = email, Password = password };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
        }
    }

    public void LoginUsername(string username, string pass)
    {
        this.username = username;
        this.password = pass;
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            var request = new LoginWithPlayFabRequest { Username = username, Password = password };
            PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
        }
    }

    public void UpdateDisplayName(string username)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = username
        }, result => {
            Debug.Log("The player's display name is now: " + result.DisplayName);
        }, error => Debug.LogError(error.GenerateErrorReport()));
    }

    private void OnLoginSuccess(LoginResult result)
    {
        loginHandler.LoginSuccess(result.PlayFabId);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        loginHandler.LoginFailure(error.GenerateErrorReport());
    }

    public void Register(string email, string pass, string username)
    {
        this.email = email;
        this.password = pass;
        this.username = username;
        var registerRequest = new RegisterPlayFabUserRequest { Email = email, Password = password, Username = username, DisplayName = username };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        loginHandler.RegisterSuccess(result.PlayFabId);
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        loginHandler.RegisterFailure(error.GenerateErrorReport());
    }

    void OnDisplayName(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log(result.DisplayName + " is your new display name");
    }

    public void SubmitScore(int value)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
            new StatisticUpdate {
                StatisticName = "Points",
                Value = value,
            }
        }
        }, result => OnStatisticsUpdated(result), FailureCallback);
    }

    private void OnStatisticsUpdated(UpdatePlayerStatisticsResult updateResult)
    {
        Debug.Log("Successfully submitted high score");
    }
    public void GetStatistics()
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            OnGetStatistics,
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void OnGetStatistics(GetPlayerStatisticsResult result)
    {
        statsManager.SetStatistics(result.Statistics);
        Debug.Log("Received the following Statistics:");
        foreach (var eachStat in result.Statistics)
            Debug.Log("Statistic (" + eachStat.StatisticName + "): " + eachStat.Value);
    }

    private void FailureCallback(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your API call. Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }
}
