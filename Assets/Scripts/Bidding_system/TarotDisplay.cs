using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TarotDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public Image cardBack;
    public Image cardFront;
    public TMP_Text cardNameText;
    public TMP_Text cardDescriptionText;
    
    [Header("Animation")]
    public bool startFlipped = false;
    public float flipDuration = 0.5f;
    
    private Coroutine flipCoroutine;
    
    void Start()
    {
        if (startFlipped)
        {
            ShowFront();
        }
        else
        {
            ShowBack();
        }
    }
    
    public void SetTarot(TarotCard tarot)
    {
        cardNameText.text = tarot.name;
        cardDescriptionText.text = tarot.description;
    }
    
    public void FlipCard()
    {
        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);
        
        flipCoroutine = StartCoroutine(FlipAnimation());
    }
    
    System.Collections.IEnumerator FlipAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        
        // Shrink
        while (elapsed < flipDuration / 2f)
        {
            float scale = Mathf.Lerp(1f, 0.1f, elapsed / (flipDuration / 2f));
            transform.localScale = startScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Flip sides
        if (cardBack.gameObject.activeSelf)
            ShowFront();
        else
            ShowBack();
        
        // Grow back
        elapsed = 0f;
        while (elapsed < flipDuration / 2f)
        {
            float scale = Mathf.Lerp(0.1f, 1f, elapsed / (flipDuration / 2f));
            transform.localScale = startScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = startScale;
    }
    
    void ShowFront()
    {
        cardBack.gameObject.SetActive(false);
        cardFront.gameObject.SetActive(true);
    }
    
    void ShowBack()
    {
        cardBack.gameObject.SetActive(true);
        cardFront.gameObject.SetActive(false);
    }
}