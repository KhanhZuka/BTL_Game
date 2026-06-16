using UnityEngine;
using UnityEngine.UI;

public class PanelInstruct : MonoBehaviour
{
    [SerializeField] private Button btnX;
    [SerializeField] private Button btnLeft;
    [SerializeField] private Button btnRight;

    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelInstruct;

    [SerializeField] private GameObject[] instructionPages;

    private int currentPage = 0;

    void Start()
    {
        btnX.onClick.AddListener(OpenInstruction);
        btnLeft.onClick.AddListener(PreviousPage);
        btnRight.onClick.AddListener(NextPage);

        ShowPage(0);
    }

    void OpenInstruction()
    {
        AudioManager.Instance.PlaySFX();
        panelInstruct.SetActive(false);
        panelMain.SetActive(true);
    }

    void NextPage()
    {
        AudioManager.Instance.PlaySFX();
        if (currentPage < instructionPages.Length - 1)
        {
            currentPage++;
            ShowPage(currentPage);
        }
    }

    void PreviousPage()
    {
        AudioManager.Instance.PlaySFX();
        if (currentPage > 0)
        {
            currentPage--;
            ShowPage(currentPage);
        }
    }

    void ShowPage(int index)
    {
        for (int i = 0; i < instructionPages.Length; i++)
        {
            if (instructionPages[i] != null)
            {
                instructionPages[i].SetActive(i == index);
            }
        }

        btnLeft.interactable = currentPage > 0;
        btnRight.interactable = currentPage < instructionPages.Length - 1;
    }
}