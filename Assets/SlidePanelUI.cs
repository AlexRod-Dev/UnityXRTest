using UnityEngine;

public class SlidePanelUI : MonoBehaviour
{

    [SerializeField] private float slideSpeed = 5f;
    [SerializeField] private float visibleY = 55f;
    [SerializeField] private float hiddenY = -200f;


    private bool bIsVisible;
    private RectTransform rectTransform;
    
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bIsVisible = false;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, hiddenY);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            bIsVisible = !bIsVisible;
        }
        float targetY = bIsVisible ? visibleY : hiddenY;
        Vector2 targetPos = new Vector2(0, targetY);
        
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPos,
            Time.deltaTime * slideSpeed);

    }
}
