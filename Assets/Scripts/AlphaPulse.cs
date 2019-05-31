using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class AlphaPulse : MonoBehaviour
{
    public TextMeshProUGUI tMesh;
    public Tween tween;

    //Fade the text's alpha in and out whenever the GameObject is enabled.
    private void OnEnable()
    {
        tMesh = this.GetComponent<TextMeshProUGUI>();

        tween = tMesh.DOColor(new Color(tMesh.color.r, tMesh.color.g, tMesh.color.b, 1f), 2f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuart);
    }
}
