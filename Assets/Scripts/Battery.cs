using System.Collections;
using UnityEngine;

public class Battery : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float chargeTime = 2.0f;

    [Header("Component References")]
    [SerializeField] private Transform chargeIndicator;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material chargingMaterial;
    [SerializeField] private Material chargedMaterial;

    private Renderer indicatorRenderer;
    private Vector3 indicatorStartScale;
    private Vector3 indicatorEndScale;

    // --- State Management ---
    private bool isPlayerNear = false;
    private bool isCharging = false;
    private bool isCharged = false;

    void Start()
    {
        if (chargeIndicator != null)
        {
            indicatorRenderer = chargeIndicator.GetComponent<Renderer>();
            if (indicatorRenderer != null)
            {
                indicatorRenderer.material = defaultMaterial;
            }

            // Store the final scale and set the initial scale (Y=0)
            indicatorEndScale = chargeIndicator.localScale;
            indicatorStartScale = new Vector3(indicatorEndScale.x, 0, indicatorEndScale.z);
            chargeIndicator.localScale = indicatorStartScale;
        }
    }

    void Update()
    {
        // Check for interaction input
        if (isPlayerNear && Input.GetKeyDown(interactionKey) && !isCharging && !isCharged)
        {
            StartCoroutine(ChargeBattery());
        }
    }

    // Core charging logic
    private IEnumerator ChargeBattery()
    {
        isCharging = true;
        indicatorRenderer.material = chargingMaterial;

        float elapsedTime = 0f;

        // Animate the scale over chargeTime
        while (elapsedTime < chargeTime)
        {
            chargeIndicator.localScale = Vector3.Lerp(
                indicatorStartScale,
                indicatorEndScale,
                elapsedTime / chargeTime
            );

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the scale is set to the final value
        chargeIndicator.localScale = indicatorEndScale;
        
        isCharging = false;
        isCharged = true;

        // Play the completion effect
        StartCoroutine(FlashEffect());
    }

    // Visual effect for completion
    private IEnumerator FlashEffect()
    {
        indicatorRenderer.material = chargedMaterial;
        yield return new WaitForSeconds(0.2f);
        indicatorRenderer.material = chargingMaterial;
        yield return new WaitForSeconds(0.15f);
        indicatorRenderer.material = chargedMaterial;
        yield return new WaitForSeconds(0.2f);
        indicatorRenderer.material = chargingMaterial;
        yield return new WaitForSeconds(0.15f);

        // Set final charged material
        indicatorRenderer.material = chargedMaterial;
    }

    // --- Trigger Detection ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}
