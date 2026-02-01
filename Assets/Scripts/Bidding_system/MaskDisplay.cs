using UnityEngine;
using TMPro;

public class MaskDisplay : MonoBehaviour
{
    [Header("Model References")]
    public Transform modelHolder; // Where to spawn the model
    private GameObject currentModel; // Currently spawned model
    
    [Header("UI References (Optional)")]
    public TMP_Text nameText; // Optional: 3D text for name
    public Canvas worldCanvas; // Optional: World space canvas
    
    [Header("Blessed Mask Effects")]
    public ParticleSystem blessedParticles; // Blessed effect
    public Material blessedMaterial; // Material override for blessed
    public Light blessedLight; // Optional: Point light for blessed
    public Color blessedGlowColor = Color.yellow;
    
    [Header("Animation")]
    public bool rotateModel = true;
    public float rotationSpeed = 30f;
    
    private bool isBlessed = false;
    private Material[] originalMaterials;
    
    void Update()
    {
        // Optional: Rotate the model slowly
        if (rotateModel && currentModel != null)
        {
            currentModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    public void SetMask(Mask mask, bool blessed = false)
    {
        isBlessed = blessed;
        
        // Clear old model
        if (currentModel != null)
        {
            Destroy(currentModel);
        }
        
        // Spawn new 3D model
        if (mask.modelPrefab != null)
        {
            Transform spawnParent = modelHolder != null ? modelHolder : transform;
            currentModel = Instantiate(mask.modelPrefab, spawnParent.position, spawnParent.rotation, spawnParent);
            
            // Store original materials
            Renderer[] renderers = currentModel.GetComponentsInChildren<Renderer>();
            originalMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].material;
            }
            
            // Apply blessed effects
            if (isBlessed)
            {
                ApplyBlessedEffects();
            }
        }
        
        // Set name text
        if (nameText != null)
        {
            string displayName = isBlessed ? $"★ BLESSED {mask.maskName} ★" : mask.maskName;
            nameText.text = displayName;
            
            if (isBlessed)
            {
                nameText.color = blessedGlowColor;
                nameText.fontStyle = FontStyles.Bold;
            }
            else
            {
                nameText.color = Color.white;
                nameText.fontStyle = FontStyles.Normal;
            }
        }
    }
    
    private void ApplyBlessedEffects()
    {
        // Apply golden material
        if (blessedMaterial != null && currentModel != null)
        {
            Renderer[] renderers = currentModel.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                // Create material instance to avoid modifying the original
                Material[] mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = blessedMaterial;
                }
                renderer.materials = mats;
                
                // Enable emission if material supports it
                if (renderer.material.HasProperty("_EmissionColor"))
                {
                    renderer.material.EnableKeyword("_EMISSION");
                    renderer.material.SetColor("_EmissionColor", blessedGlowColor * 2f);
                }
            }
        }
        
        // Start particles
        if (blessedParticles != null)
        {
            blessedParticles.Play();
            
            // Tint particles to blessed color
            var main = blessedParticles.main;
            main.startColor = blessedGlowColor;
        }
        
        // Enable light
        if (blessedLight != null)
        {
            blessedLight.enabled = true;
            blessedLight.color = blessedGlowColor;
        }
    }
    
    public void SetMask(Mask mask)
    {
        SetMask(mask, false);
    }
    
    void OnDestroy()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
        }
    }
}