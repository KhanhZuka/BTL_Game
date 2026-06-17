using UnityEngine;
using UnityEngine.UI;

public class CoinUIManager : MonoBehaviour
{
    public static CoinUIManager Instance;

    public Image[] coinSlots; 

    int currentCoins = 0;

    void Awake()
    {
        Instance = this;

        // tắt hết ban đầu
        foreach (var img in coinSlots)
        {
            img.enabled = false;
        }
    }

    public void AddCoin()
    {
        if (currentCoins >= coinSlots.Length) return;

        coinSlots[currentCoins].enabled = true;
        currentCoins++;
    }
}
