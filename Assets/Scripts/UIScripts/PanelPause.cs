using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PanelPause : MonoBehaviour
{
    [SerializeField] private Button btnResume;
    [SerializeField] private Button btnPlayAgain;
    [SerializeField] private Button btnQuit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btnQuit.onClick.AddListener(OpenUIScene);
        btnResume.onClick.AddListener(ResumePlay);
        btnPlayAgain.onClick.AddListener(PlayAgain);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OpenUIScene()
    {
        AudioManager.Instance.PlaySFX();
        Time.timeScale = 1f;
        GameData.backToMapPanel = true;
        SceneManager.LoadScene("UIScene");
    }
    
    void ResumePlay()
    {
        AudioManager.Instance.PlaySFX();
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    void PlayAgain()
    {
        AudioManager.Instance.PlaySFX();
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }
}
