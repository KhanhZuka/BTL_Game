using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Image[] slotIcons;

    public GameObject panelMain;
    public GameObject panelMaps;
    public GameObject panelInstruct;
    public GameObject panelLose;
    public GameObject panelWin;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameData.backToMapPanel)
        {
            OpenPanelMaps();
            GameData.backToMapPanel = false;
        }
        else
        {
            OpenPanelMain();
        }
    }

    public void OpenPanelMaps()
    {
        panelMain.SetActive(false);
        panelInstruct.SetActive(false);
        panelLose.SetActive(false);
        panelWin.SetActive(false);

        panelMaps.SetActive(true);
    }

    public void OpenPanelMain()
    {
        panelMaps.SetActive(false);
        panelInstruct.SetActive(false);
        panelLose.SetActive(false);
        panelWin.SetActive(false);

        panelMain.SetActive(true);
    }

    public void UpdateInventory(List<ItemData> items)
    {
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (i < items.Count)
            {
                slotIcons[i].sprite = items[i].icon;
                slotIcons[i].enabled = true;
            }
            else
            {
                slotIcons[i].enabled = false;
            }
        }
    }
}