using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class PanelWin : MonoBehaviour
{
    public Button BtnPlayAgain;
    public Button BtnContinueMap;
    public GameObject[] ImgStars;
    public Text TxtCoins;
    private int soSao;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

            soSao = 1;


            for (int i = 0; i < ImgStars.Length; i++)
                ImgStars[i].SetActive(false);

            BtnPlayAgain.onClick.AddListener(PlayAgain);
            BtnContinueMap.onClick.AddListener(ContinueMap);

            int coins = GameData.lastCoins;
            int enemyDead = GameData.lastEnemyDead;
            Debug.Log("Cointtt: " + coins);
            TxtCoins.text = coins.ToString();
            if (coins == 6) soSao++;
            if (enemyDead >= 3) soSao++;

            GameData.lastEnemyDead = 0;
            GameData.lastCoins = 0;


            for (int i = 0; i < soSao && i < ImgStars.Length; i++)
                ImgStars[i].SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayAgain()
    {
        AudioManager.Instance.PlaySFX();
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
        AudioManager.Instance.PlaySFX();
        UIManager.Instance.OpenPanelMaps();
    }
}
