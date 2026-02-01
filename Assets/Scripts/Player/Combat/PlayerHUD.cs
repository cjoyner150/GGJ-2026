using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI acornsTMP;
    [SerializeField] MMProgressBar healthBar;
    [SerializeField] Image headIMG;

    [SerializeField] Sprite bear;
    [SerializeField] Sprite bee;
    [SerializeField] Sprite butterfly;
    [SerializeField] Sprite crow;
    [SerializeField] Sprite man;
    [SerializeField] Sprite goddess;
    [SerializeField] Sprite rabbit;
    [SerializeField] Sprite snake;
    [SerializeField] Sprite turtle;

    [SerializeField] Image img1;
    [SerializeField] Image img2;

    PlayerContext ctx;

    public void InitHUD(PlayerConfig cfg, PlayerContext ctx)
    {
        this.ctx = ctx;

        acornsTMP.text = $"{cfg.Acorns}";

        switch (cfg.Mask.type)
        {
            case MaskObject.maskType.Bear:
                headIMG.sprite = bear;
                break;
            case MaskObject.maskType.Bee:
                headIMG.sprite = bee;
                break;
            case MaskObject.maskType.Butterfly:
                headIMG.sprite = butterfly;
                break;
            case MaskObject.maskType.Crow:
                headIMG.sprite = crow;
                break;
            case MaskObject.maskType.Man:
                headIMG.sprite = man;
                break;
            case MaskObject.maskType.Goddess:
                headIMG.sprite = goddess;
                break;
            case MaskObject.maskType.Rabbit:

                headIMG.sprite = rabbit;
                break;
            case MaskObject.maskType.Snake:
                headIMG.sprite = snake;
                break;
            case MaskObject.maskType.Turtle:
                headIMG.sprite = turtle;
                break;
        }

        headIMG.color = cfg.PlayerColor;

        if (cfg.Tarots.Count > 0)
        {
            img1.sprite = cfg.Tarots[0].UIAsset;

            if (cfg.Tarots.Count > 1)
            {
                img2.sprite = cfg.Tarots[1].UIAsset;
            }
            else Destroy(img2);
        }
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
