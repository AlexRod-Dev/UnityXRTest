using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragToWorld : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    // === Configuration ===
    [Header("Object to Place")]
    [SerializeField] private GameObject objectToPlace;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

    [Header("Visual Effects")]
    [SerializeField] private Material beamMaterial;

    [Header("UI Drag Feedback")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private GameObject dragGhostPrefab;

    // === Runtime State ===
    private GameObject previewObject;
    private Renderer previewRenderer;
    private MaterialPropertyBlock propBlock;
    private float fixedY;
    private Vector3 fixedScale;
    private bool bCanPlace;

    // === Object Rotation ===
    private float currentRotationY = 0f;
    private float rotationStep = 15f;

    // === Snapping ===
    private bool bSnapToGrid = false;
    private float snapSize = 1f;

    // === UI Ghost Follow ===
    private GameObject uiGhost;
    private RectTransform uiGhostRect;
    private float ghostMinLifetime = 0.5f;
    private float ghostSpawnTime;
    private Coroutine followCoroutine;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        propBlock = new MaterialPropertyBlock();
        
    }

    private void Update()
    {
        if (!previewObject) return;
        
        // Rotate with Q/E keys
        float rotationSpeed = rotationStep * Time.deltaTime * 10f;

        if (Input.GetKey(KeyCode.Q))
        {
            currentRotationY -= rotationSpeed;
            ApplyRotation();
        }
        else if (Input.GetKey(KeyCode.E))
        {
            currentRotationY += rotationSpeed;
            ApplyRotation();
        }
        
        // Toggle snap-to-grid with X
        if (Input.GetKeyDown(KeyCode.X))
        {
            bSnapToGrid = !bSnapToGrid;
            Debug.Log("Snap to Grid: " + (bSnapToGrid ? "ON" : "OFF"));
        }
        
    }

    private void ApplyRotation()
    {
        previewObject.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (objectToPlace == null) return;

        // === Create 3D Preview Object ===
        previewObject = Instantiate(objectToPlace);
        
        fixedScale = objectToPlace.transform.localScale;
        previewObject.transform.localScale = fixedScale;

        previewRenderer = previewObject.GetComponentInChildren<Renderer>();
        if (previewRenderer != null)
        {
            // Get the vertical extent (half-height) of the object
            fixedY = previewRenderer.bounds.extents.y;
        }
        else
        {
            fixedY = 0.5f; // Fallback if no renderer
        }

        SetPreviewColor(validColor);
        
        if (previewObject.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;
        
        // === Create UI Ghost Feedback ===
        uiGhost = Instantiate(dragGhostPrefab, canvas.transform);
        uiGhostRect = uiGhost.GetComponent<RectTransform>();
        ghostSpawnTime = Time.time;

        // Centered anchors and pivot
        uiGhostRect.anchorMin = uiGhostRect.anchorMax = uiGhostRect.pivot = new Vector2(0.5f, 0.5f);
        uiGhostRect.position = eventData.pressPosition;
        
        // Transparent and non-blocking UI
        CanvasGroup cg = uiGhost.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
        cg.alpha = 1f;

        // Start smooth ghost follow
        followCoroutine = StartCoroutine(SmoothFollowGhost());
    }
    
    private IEnumerator SmoothFollowGhost()
    {
        float elapsed = 0f;
        float duration = transitionDuration; // Total lifetime (you can adjust or base on distance)
        Vector3 initialScale = Vector3.one * 0.5f;
        Vector3 peakScale = Vector3.one * 1.5f;

        CanvasGroup cg = uiGhost.GetComponent<CanvasGroup>();
        if (cg == null) cg = uiGhost.AddComponent<CanvasGroup>();

        while (uiGhostRect != null && previewObject != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Follow the 3D object
            Vector3 worldTarget = previewObject.transform.position;
            Vector3 screenTarget = RectTransformUtility.WorldToScreenPoint(mainCamera, worldTarget);
            uiGhostRect.position = Vector3.Lerp(uiGhostRect.position, screenTarget, Time.deltaTime * 10f);

            // Scale up for first half, then down
            if (t < 0.5f)
            {
                float growT = t / 0.5f;
                uiGhostRect.localScale = Vector3.Lerp(initialScale, peakScale, growT);
            }
            else
            {
                float shrinkT = (t - 0.5f) / 0.5f;
                uiGhostRect.localScale = Vector3.Lerp(peakScale, Vector3.zero, shrinkT);
            }

            // Fade out in second half
            cg.alpha = t < 0.5f ? 1f : 1f - ((t - 0.5f) / 0.5f);

            yield return null;
        }

        // Ensure minimum visibility time
        float minTimeLeft = ghostMinLifetime - elapsed;
        if (minTimeLeft > 0f)
            yield return new WaitForSeconds(minTimeLeft);

        if (uiGhost) Destroy(uiGhost);
        followCoroutine = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (previewObject == null) return;

        // Raycast from mouse into the world
        Ray ray = mainCamera.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 pos = hit.point;
            pos.y = fixedY; // always keep it grounded

            // Snap to grid if enabled
            if (bSnapToGrid)
            {
                pos.x = Mathf.Round(pos.x / snapSize) * snapSize;
                pos.z = Mathf.Round(pos.z / snapSize) * snapSize;
            }

            // Move and validate position
            previewObject.transform.position = pos;
            previewObject.transform.localScale = fixedScale; // enforce constant scale

            bCanPlace = hit.collider.CompareTag("Ground") && !IsOverlapping(pos);

            SetPreviewColor(bCanPlace ? validColor : invalidColor);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    if (bCanPlace)
    { 
        StartCoroutine(PlayPlacementBeamEffect(previewObject.transform.position, previewObject.transform.rotation)); 
    }
    
    // Cleanup
    if (previewObject) 
        Destroy(previewObject);
    
    if (uiGhost) 
        Destroy(uiGhost);
    
    if (followCoroutine != null) 
        StopCoroutine(followCoroutine);
    }

    private bool IsOverlapping(Vector3 position)
    {
        Vector3 halfExtents = previewRenderer ? previewRenderer.bounds.extents : Vector3.one * 0.5f;

        // Temporarily disable preview colliders for check
        Collider[] previewColliders = previewObject.GetComponentsInChildren<Collider>();
        foreach (var col in previewColliders)
            col.enabled = false;
        
        Collider[] colliders = Physics.OverlapBox(position, halfExtents, Quaternion.identity);

        foreach (var col in colliders)
        {
            if (!col.CompareTag("Ground") && !col.transform.IsChildOf(previewObject.transform))
                return true;
        }

        return false;
    }

    private void SetPreviewColor(Color color)
    {
        if (previewRenderer == null) return;

        previewRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_BaseColor", color);
        previewRenderer.SetPropertyBlock(propBlock);
    }
    
    
    IEnumerator PlayPlacementBeamEffect(Vector3 position, Quaternion rotation)
    {
        // === Play Beam Effect ===
        GameObject beam = Instantiate(objectToPlace, position, rotation);
    
        
        foreach (var renderer in beam.GetComponentsInChildren<Renderer>())
        {
            Material mat = new Material(beamMaterial); // Instance it!
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.cyan * 5f); // Brighter emissive glow
            renderer.material = mat;
        }

        Vector3 startScale = beam.transform.localScale;
        Vector3 targetScale = startScale;
        startScale.y = 0.001f;
        beam.transform.localScale = startScale;

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 currentScale = Vector3.Lerp(startScale, targetScale, t);
            beam.transform.localScale = currentScale;

            elapsed += Time.deltaTime;
            yield return null;
        }

        beam.transform.localScale = targetScale;

        Destroy(beam);

        // === Final Placement ===
        GameObject placed = Instantiate(objectToPlace, position, rotation);
        placed.transform.localScale = targetScale;
        placed.transform.SetParent(GameObject.FindGameObjectWithTag("Ground")?.transform);
        currentRotationY = 0f; //Reset rotation
    }
    
}