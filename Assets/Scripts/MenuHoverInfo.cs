using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuHoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tMeshObj;
    [TextArea] public string infoText;

    //Update the hover info text when the player hovers over a menu item
    public void OnPointerEnter(PointerEventData eventData)
    {
        var tMesh = tMeshObj.GetComponent<TextMeshProUGUI>();
        tMesh.text = infoText;
    }

    //Clear text when no longer hovered over
    public void OnPointerExit(PointerEventData eventData)
    {
        var tMesh = tMeshObj.GetComponent<TextMeshProUGUI>();
        tMesh.text = "";
    }
}
