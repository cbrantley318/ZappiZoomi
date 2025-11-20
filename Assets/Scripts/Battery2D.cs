using System.Collections;
using UnityEngine;

public class Battery2D : MonoBehaviour
{
    // --- Configuration ---
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float chargeTime = 2.0f;

    [Header("Visual Colors")]
    [SerializeField] private Color defaultColor = Color.gray;
    [SerializeField] private Color chargingColor = Color.yellow;
    [SerializeField] private Color chargedColor = Color.green;

    // --- References ---
    [Header("Component References")]
    [SerializeField] private Transform chargeIndicator;

    private SpriteRenderer batteryRenderer;
    private SpriteRenderer indicatorRenderer;
    
    // Scale vectors used for the smooth fill animation
    private Vector3 indicatorStartScale;
    private Vector3 indicatorEndScale;

    // --- State Management ---
    private bool isPlayerNear;
    private bool isCharging;
    private bool isCharged;

    void Start()
    {
        // Get renderers. Null-conditional operator is often used in human code, 
        // but here we check for null explicitly for clarity/safety.
        batteryRenderer = GetComponent<SpriteRenderer>();
        indicatorRenderer = chargeIndicator?.GetComponent<SpriteRenderer>();

        if (batteryRenderer != null)
        {
            batteryRenderer.color = defaultColor;
        }

        if (chargeIndicator != null)
        {
            // Set initial colors
            if (indicatorRenderer != null)
            {
                indicatorRenderer.color = defaultColor;
            }
            
            // Define scale vectors for Lerp animation: Start Y at 0, End at full scale
            indicatorEndScale = chargeIndicator.localScale;
            indicatorStartScale = indicatorEndScale;
            indicatorStartScale.y = 0;
            
            // Set initial visual state
            chargeIndicator.localScale = indicatorStartScale;
        }
    }

    void Update()
    {
        // Check if conditions for starting interaction are met
        if (isPlayerNear && Input.GetKeyDown(interactionKey) && !isCharging && !isCharged)
        {
            StartCoroutine(ChargeSequence());
        }
    }

    // Main coroutine handling the charging duration and visual fill
    private IEnumerator ChargeSequence()
    {
        isCharging = true;
        
        // Initial visual state (e.g., solid yellow)
        SetColors(chargingColor);

        float elapsedTime = 0f;

        // Scale animation using Vector3.Lerp for smooth fill over time
        while (elapsedTime < chargeTime)
        {
            chargeIndicator.localScale = Vector3.Lerp(
                indicatorStartScale,
                indicatorEndScale,
                elapsedTime / chargeTime
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the scale is exactly at the end point
        chargeIndicator.localScale = indicatorEndScale;
        isCharging = false;
        isCharged = true;

        // Trigger completion feedback
        StartCoroutine(FlashEffect());
    }

    // Provides visual confirmation (flash) that the battery is charged
    private IEnumerator FlashEffect()
    {
        // Flash 1 (Bright)
        SetColors(chargedColor);
        yield return new WaitForSeconds(0.2f); 

        // Dim
        SetColors(chargingColor);
        yield return new WaitForSeconds(0.15f);

        // Flash 2 (Bright)
        SetColors(chargedColor);
        yield return new WaitForSeconds(0.2f); 

        // Final Charged Color
        SetColors(chargedColor);
    }
    
    // Helper method to set both battery and indicator colors consistently
    private void SetColors(Color targetColor)
    {
        if (batteryRenderer != null) batteryRenderer.color = targetColor;
        if (indicatorRenderer != null) indicatorRenderer.color = targetColor;
    }

    // Check for Player entering the trigger zone
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    // Check for Player exiting the trigger zone
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}