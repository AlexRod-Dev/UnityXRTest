using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PlaceableUI", menuName = "Scriptable Objects/PlaceableUI", order = 1)]
public class PlaceableUI : ScriptableObject
{
    public string objectName;
    public Sprite icon;
    public GameObject prefab;

}
