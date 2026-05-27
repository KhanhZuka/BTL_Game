using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PanelMain : MonoBehaviour
{
   [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnInstruct;

    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelInstruct;
    [SerializeField] private GameObject panelMaps;

    void Start()
    {
        btnInstruct.onClick.AddListener(OpenInstruction);
        btnPlay.onClick.AddListener (OpenMaps);
        if (GameData.backToMapPanel)
        {
            panelMain.SetActive(false);
            panelMaps.SetActive(true);

            GameData.backToMapPanel = false;
        }
        else
        {
            panelMain.SetActive(true);
            panelMaps.SetActive(false);
        }
    }

    void OpenInstruction()
    {
        panelMain.SetActive(false);
        panelInstruct.SetActive(true);
    }

    void OpenMaps()
    {
        panelMain.SetActive(false);
        panelMaps.SetActive(true);
    }
}
