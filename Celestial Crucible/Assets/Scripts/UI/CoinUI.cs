using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    public TextMeshProUGUI coinText;

    void Update()
    {
        coinText.text = "$" + GameManager.Instance.coinCount;
    }
}

