using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private Text leaderBoardTitle;
    [SerializeField] private Text[] leaderBoardUsername = new Text[10];
    [SerializeField] private Text[] leaderboardPoints = new Text[10];

    [SerializeField] private string defaultUsername = "----------";
    [SerializeField] private string defaultPoints = "--";

    private MenuEventHandler menuEvent;

    private string[] leaderboards =
{
        "enemies_killed",
        "games",
        "levels",
        "Points"
    };

    private string[] leaderboardTitles =
    {
        "Enemies Killed",
        "Games Played",
        "Levels",
        "Points"
    };

    public void Fill(GetLeaderboardResult result)
    {
        var leaderboard = result.Leaderboard;
        int n = leaderBoardUsername.Length;
        if (leaderboard.Count < leaderBoardUsername.Length)
            n = leaderboard.Count;

        for (int i = 0; i < leaderBoardUsername.Length; i++)
        {
            if (i < n)
            {
                StartCoroutine(fillText(leaderBoardUsername[i], leaderboard[i].DisplayName));
                StartCoroutine(fillText(leaderboardPoints[i], leaderboard[i].StatValue.ToString()));
            }
            else
            {
                leaderBoardUsername[i].text = defaultUsername;
                leaderboardPoints[i].text = defaultPoints;
            }
        }

        menuEvent.SetLoadingAnimActive(false);
    }

    public void SetTitle(string title)
    {
        StartCoroutine(fillText(leaderBoardTitle, title));
    }

    void Start()
    {
        if (leaderboardPoints.Length != leaderBoardUsername.Length)
            Debug.LogError("Error: leaderBoardUsername and leaderboardPoints should have same lenght!");

        menuEvent = MenuEventHandler.Instance;
    }

    public void GetLeaderBoard(StatisticsType statisticType)
    {
        PlayFabClientAPI.GetLeaderboard(
            new GetLeaderboardRequest
            {
                MaxResultsCount = leaderBoardUsername.Length,
                StatisticName = leaderboards[(int)statisticType]
            },
            Fill,
            error => Debug.LogError(error.GenerateErrorReport())
        );
        Clear();
        SetTitle(leaderboardTitles[(int)statisticType]);
    }

    private IEnumerator fillText(Text text, string value)
    {
        text.text = null;
        string temp = null;
        foreach(char c in value.ToCharArray())
        {
            yield return new WaitForSeconds(5 * Time.deltaTime);
            temp += c;
            text.text = temp;
        }
    }

    private void Clear()
    {
        StopAllCoroutines();
        for (int i = 0; i < leaderBoardUsername.Length; i++)
        {
            leaderBoardUsername[i].text = defaultUsername;
            leaderboardPoints[i].text = defaultPoints;
        }
    }
}

public enum StatisticsType
{
    enemiesKilled = 0,
    games = 1,
    levels = 2,
    points = 3,
    start = -1
}
