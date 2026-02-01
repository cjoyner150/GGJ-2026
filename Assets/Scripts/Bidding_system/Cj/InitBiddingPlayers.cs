using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class InitBiddingPlayers : MonoBehaviour
{
    [SerializeField] GameObject biddingPlayerPrefab;
    [SerializeField] HorizontalLayoutGroup hlg;

    void Initialize()
    {
        var configs = PlayerConfigManager.Instance.GetPlayerConfigs();

        for (int i = 0; i<configs.Count; i++)
        {
            var player = Instantiate(biddingPlayerPrefab, hlg.transform);
            configs[i].Input.uiInputModule = player.GetComponentInChildren<InputSystemUIInputModule>();

            var inp = player.GetComponent<PlayerBiddingInput>();
            inp.Initialize(configs[i]);

        }

    }
}
