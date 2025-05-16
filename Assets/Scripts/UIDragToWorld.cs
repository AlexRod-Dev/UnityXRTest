using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragToWorld : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject objectToPlace;
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    

    private GameObject previewObject;
    private Camera mainCamera;
    private MaterialPropertyBlock propBlock;
    private Renderer previewRenderer;

    private Vector3 fixedScale;
    private float fixedY;

    private bool canPlace = false;

    void Start()
    {
        mainCamera = Camera.main;
        propBlock = new MaterialPropertyBlock();

        invalidColor.a = 0.02f;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (objectToPlace == null) return;

        previewObject = Instantiate(objectToPlace);
        previewObject.transform.SetParent(null);

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

            previewObject.transform.position = pos;
            previewObject.transform.localScale = fixedScale; // enforce constant scale

            canPlace = hit.collider.CompareTag("Ground") && !IsOverlapping(pos);

            SetPreviewColor(canPlace ? validColor : invalidColor);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canPlace)
        {
            GameObject placedObject = Instantiate(objectToPlace, previewObject.transform.position, Quaternion.identity);
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