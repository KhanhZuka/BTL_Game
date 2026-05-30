using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class PanelWin : MonoBehaviour
{
    public Button BtnPlayAgain;
    public Button BtnContinueMap;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BtnPlayAgain.onClick.AddListener(PlayAgain);
        BtnContinueMap.onClick.AddListener(ContinueMap);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayAgain()
    {
        Time.timeScale = 1f;

        switch (GameData.lastMap)
        {
            case "Map1":
                SceneManager.LoadScene("Map1");
                break;

            case "Map2":
                SceneManager.LoadScene("Map2");
                break;

            case "Map3":
                SceneManager.LoadScene("Map3");
                break ;

            case "Map4":
                SceneManager.LoadScene("Map4");
                break;
        }
    }

    void ContinueMap()
    {
        UIManager.Instance.OpenPanelMaps();
    }
}
