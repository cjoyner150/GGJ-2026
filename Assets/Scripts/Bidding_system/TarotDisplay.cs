using UnityEngine;
using TMPro;

public class TarotDisplay : MonoBehaviour
{
    [Header("Model References")]
    public Transform modelHolder; // Where to spawn the model
    private GameObject currentModel; // Currently spawned model
    
    [Header("UI References (Optional)")]
    public TMP_Text nameText; // Card name
    public TMP_Text effectText; // Card effect
    public Canvas worldCanvas; // World space canvas
    
    [Header("Rare Card Effects")]
    public ParticleSystem rareGlow; // Rare card effect
    public Material rareMaterial; // Material override for rare
    public Light rareLight; // Optional: Point light for rare
    public Color rareGlowColor = new Color(0.5f, 0.2f, 1f); // Purple
    
    [Header("Animation")]
    public bool rotateModel = true;
    public float rotationSpeed = 20f;
    public bool floatAnimation = true;
    public float floatHeight = 0.1f;
    public float floatSpeed = 1f;
    
    private bool isRare = false;
    private Vector3 startPosition;
    private float floatTimer = 0f;
    
    void Start()
    {
        if (modelHolder != null)
        {
            startPosition = modelHolder.localPosition;
        }
        else
        {
            startPosition = transform.localPosition;
        }
    }
    
    void Update()
    {
        if (currentModel == null) return;
        
        // Rotate the card
        if (rotateModel)
        {
            currentModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        // Float animation
        if (floatAnimation)
        {
            floatTimer += Time.deltaTime * floatSpeed;
            float yOffset = Mathf.Sin(floatTimer) * floatHeight;
            
            Transform targetTransform = modelHolder != null ? modelHolder : transform;
            Vector3 newPos = startPosition;
            newPos.y += yOffset;
            targetTransform.localPosition = newPos;
        }
    }
    
    public void SetTarot(TarotCard tarot)
    {
        isRare = tarot.isRare;
        
        // Clear old model
        if (currentModel != null)
        {
            Destroy(currentModel);
        }
        
        // Spawn new 3D model
        if (tarot.modelPrefab != null)
        {
            Transform spawnParent = modelHolder != null ? modelHolder : transform;
            currentModel = Instantiate(tarot.modelPrefab, spawnParent.position, spawnParent.rotation, spawnParent);
            
            // Apply rare effects if needed
            if (isRare)
            {
                ApplyRareEffects();
            }
        }
        
        // Set name text
        if (nameText != null)
        {
            string displayName = isRare ? $"★ {tarot.cardName} ★" : tarot.cardName;
            nameText.text = displayName;
            
            if (isRare)
            {
                nameText.color = rareGlowColor;
                nameText.fontStyle = FontStyles.Bold;
            }
            else
            {
                nameText.color = Color.white;
                nameText.fontStyle = FontStyles.Normal;
            }
        }
        
        // Set effect text
        if (effectText != null)
        {
            effectText.text = tarot.effect;
        }
    }
    
    private void ApplyRareEffects()
    {
        // Apply rare material
        if (rareMaterial != null && currentModel != null)
        {
            Renderer[] renderers = currentModel.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                Material[] mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = rareMaterial;
                }
                renderer.materials = mats;
                
                // Enable emission
                if (renderer.material.HasProperty("_EmissionColor"))
                {
                    renderer.material.EnableKeyword("_EMISSION");
                    renderer.material.SetColor("_EmissionColor", rareGlowColor * 3f);
                }
            }
        }
        
        // Start particles
        if (rareGlow != null)
        {
            rareGlow.Play();
            
            var main = rareGlow.main;
            main.startColor = rareGlowColor;
        }
        
        // Enable light
        if (rareLight != null)
        {
            rareLight.enabled = true;
            rareLight.color = rareGlowColor;
        }
    }
    
    void OnDestroy()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
        }
    }
}