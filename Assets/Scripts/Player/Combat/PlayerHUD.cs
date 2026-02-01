using MoreMountains.Tools;
using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI acornsTMP;
    [SerializeField] MMProgressBar healthBar;

    PlayerContext ctx;

    public void InitHUD(PlayerConfig cfg, PlayerContext ctx)
    {
        this.ctx = ctx;

        acornsTMP.text = $"{cfg.Acorns}";
    }

    public void Update()
    {
        if (ctx == null)
        {
            Destroy(gameObject);
        }

        if (healthBar.BarTarget != ctx.currentHealth) healthBar.UpdateBar(ctx.currentHealth, 0, ctx.maxHealth);
    }
}
