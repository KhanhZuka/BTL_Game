using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanelMaps : MonoBehaviour
{
    [SerializeField] private Button btnComeBack;
    [SerializeField] private Button btnMan1;
    [SerializeField] private Button btnMan2;
    [SerializeField] private Button btnMan3;
    [SerializeField] private Button btnMan4;

    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelMap;

    void Start()
    {
        btnComeBack.onClick.AddListener(OpenMainScreen);

        btnMan1.onClick.AddListener(OpenMapOne);
        btnMan2.onClick.AddListener(OpenMapTwo);
        btnMan3.onClick.AddListener(OpenMapThree);
        btnMan4.onClick.AddListener(OpenMapFour);
    }

    void OpenMapOne()
    {
        AudioManager.Instance.PlaySFX();
        SceneManager.LoadScene("Map1");
    }

    void OpenMapTwo()
    {
        AudioManager.Instance.PlaySFX();
        SceneManager.LoadScene("Map2");
    }

    void OpenMapThree()
    {
        AudioManager.Instance.PlaySFX();
        SceneManager.LoadScene("Map3");
    }

    void OpenMapFour()
    {
        AudioManager.Instance.PlaySFX();
        SceneManager.LoadScene("Map4");
    }

    void OpenMainScreen()
    {
        AudioManager.Instance.PlaySFX();
        panelMap.SetActive(false);
        panelMain.SetActive(true);
    }
}