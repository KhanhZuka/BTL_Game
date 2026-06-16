using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanelLose : MonoBehaviour
{
    public Button BtnPlayAgain;
    public Button BtnQuit;
    public GameObject[] ImgStars;
    private int soSao;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        soSao = 0;
        for (int i = 0; i < ImgStars.Length; i++)
        {
            ImgStars[i].gameObject.SetActive(false);
        }
        BtnPlayAgain.onClick.AddListener(PlayAgain);
        BtnQuit.onClick.AddListener(OpenUIScene);
        if (PlayerOne.instance.soXu == 6) soSao++;
        if (PlayerOne.instance.soQuaiDead >= 3) soSao++;
        for (int i = 0; i < soSao; i++)
        {
            ImgStars[i].gameObject.SetActive(true);
        }
        PlayerOne.instance.soXu = 0;
        PlayerOne.instance.soQuaiDead = 0;

        if (PlayerController.instance.soXu == 6) soSao++;
        if (PlayerController.instance.soQuaiDead >= 3) soSao++;
        for (int i = 0; i < soSao; i++)
        {
            ImgStars[i].gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayAgain()
    {
        Time.timeScale = 1f;
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
                break;

            case "Map4":
                SceneManager.LoadScene("Map4");
                break;
        }
    }

    void OpenUIScene()
    {
        AudioManager.Instance.PlaySFX();
        Time.timeScale = 1f;
        GameData.backToMapPanel = true;
        SceneManager.LoadScene("UIScene");
    }
}
