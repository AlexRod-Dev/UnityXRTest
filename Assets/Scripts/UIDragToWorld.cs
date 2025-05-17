using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragToWorld : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private GameObject objectToPlace;
   
    
    [SerializeField]
    private Color validColor = Color.green;
    
    [SerializeField]
    private Color invalidColor = Color.red;
    

    private GameObject previewObject;
    private Camera mainCamera;
    private Renderer previewRenderer;
    private MaterialPropertyBlock propBlock;
    
    private Vector3 fixedScale;
    private float fixedY;
    private bool bCanPlace;

    private float currentRotationY = 0f;
    private float rotationStep = 15f;

    private bool bSnapToGrid;
    private float snapSize;

    void Start()
    {
        bCanPlace = false;
        bSnapToGrid = false;
        snapSize = 1f;
        
        mainCamera = Camera.main;
        propBlock = new MaterialPropertyBlock();
        
    }

    private void Update()
    {
        if (!previewObject) return;
        
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

        previewObject = Instantiate(objectToPlace);
       

        fixedScale = objectToPlace.transform.localScale;
        previewObject.transform.localScale = fixedScale;

        previewRenderer = previewObject.GetComponentInChildren<Renderer>();
        if (previewRenderer != null)
        {
            // Fix Y position to keep it always grounded
            fixedY = previewRenderer.bounds.size.y / 2f;
        }
        else
        {
            fixedY = 0.5f;
        }

        SetPreviewColor(validColor);

        if (previewObject.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (previewObject == null) return;

        Ray ray = mainCamera.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 pos = hit.point;
            pos.y = fixedY; // always keep it grounded

            if (bSnapToGrid)
            {
                pos.x = Mathf.Round(pos.x / snapSize) * snapSize;
                pos.z = Mathf.Round(pos.z / snapSize) * snapSize;
            }

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
            GameObject placedObject = Instantiate(objectToPlace, previewObject.transform.position, previewObject.transform.rotation);
            placedObject.transform.localScale = fixedScale;
            placedObject.transform.SetParent(GameObject.FindGameObjectWithTag("Ground")?.transform);
        }

        if (previewObject != null)
            Destroy(previewObject);
    }

    private bool IsOverlapping(Vector3 position)
    {
        Vector3 halfExtents = (previewRenderer != null) ? previewRenderer.bounds.extents : Vector3.one * 0.5f;

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
}