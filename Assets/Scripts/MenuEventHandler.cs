using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuEventHandler : MonoBehaviour
{
    public static MenuEventHandler Instance;

    [SerializeField] private Color white, gray;
    [SerializeField] private GameObject loadingAnim;
    [SerializeField] private GameObject loginFormObject, openLoginFormButton;
    [SerializeField] private GameObject registerFormObject, openRegisterFormButton;
    [SerializeField] private GameObject leaderboardObject;
    [SerializeField] private LeaderboardManager leaderboardManager;
    private Canvas canvas;

    private StatisticsType lastLeaderboard = StatisticsType.start;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        if(canvas != null)
            canvas.worldCamera = Camera.main;
    }
    public void StartGame()
    {
        if (isLoadingAnimActive())
            return;
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void Menu()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void SetLoadingAnimActive(bool active)
    {
        loadingAnim.SetActive(active);
    }

    public bool isLoadingAnimActive()
    {
        return loadingAnim.activeSelf;
    }

    public void OpenLeaderboard(int statisticsTypeInt)
    {
        if (!isLoadingAnimActive())
        {
            StatisticsType statisticsType = (StatisticsType)statisticsTypeInt;
            if (lastLeaderboard == StatisticsType.start || lastLeaderboard != statisticsType)
            {
                leaderboardObject.SetActive(true);
                lastLeaderboard = statisticsType;
                leaderboardManager.GetLeaderBoard(statisticsType);
                SetLoadingAnimActive(true);
            }
            else
                leaderboardObject.SetActive(!leaderboardObject.activeSelf);
        }
    }

    public void OpenLoginForm()
    {
        if (registerFormObject.activeSelf)
            OpenRegisterForm();
        bool isActive = !loginFormObject.activeSelf;
        loginFormObject.SetActive(isActive);

        openLoginFormButton.GetComponent<Image>().color = isActive ? gray : white;
        openLoginFormButton.GetComponentInChildren<Text>().color = isActive ? white : gray;
    }

    public void OpenRegisterForm()
    {
        if (loginFormObject.activeSelf)
            OpenLoginForm();
        bool isActive = !registerFormObject.activeSelf;
        registerFormObject.SetActive(isActive);

        openRegisterFormButton.GetComponent<Image>().color = isActive ? gray : white;
        openRegisterFormButton.GetComponentInChildren<Text>().color = isActive ? white : gray;
    }
}