using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DefaultMats : MonoBehaviour
{
    [Header("Materials")]
    public Material defaultWalls;
    public Material defaultFloors,
    fallbackMat,
    transparentMat;

    [Header("Physics Materials")]
    public PhysicMaterial defaultPhysWalls;
    public PhysicMaterial defaultPhysFloors;

    [Header("Miscellaneous")]
    public bool xRayEnabled;
    public bool xRayChanging;
    private List<MeshRenderer> holeRenderers;
    
    //Fetches all the MeshRenderers of the hole itself, the props and the miscellaneous objects
    public void GetHoleRenderers()
    {
        holeRenderers = new List<MeshRenderer>();

        holeRenderers.Add(GameObject.FindGameObjectWithTag("HoleWalls").GetComponent<MeshRenderer>());
        holeRenderers.Add(GameObject.FindGameObjectWithTag("HoleFloor").GetComponent<MeshRenderer>());

        var propRends= GameObject.Find("PropPool").GetComponentsInChildren<MeshRenderer>();
        var miscRends = GameObject.Find("MiscPool").GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer m in propRends) holeRenderers.Add(m);
        foreach (MeshRenderer m in miscRends) holeRenderers.Add(m);
    }

    //Switches the materials of our MeshRenderers
    public TweenCallback UpdateMats(Material newMat, Material[] rendererMats, MeshRenderer renderer, int matIndex)
    {
        rendererMats[matIndex] = newMat;
        renderer.materials = rendererMats;
        return null;
    }

    //Fades our original materials to their previous color
    public TweenCallback FadeToColor(Material newMat, Color prevColor)
    {
        if (xRayEnabled)
        {
            Sequence fadeToColorSeq = DOTween.Sequence();
            fadeToColorSeq.PrependInterval(.2f).
                Append(newMat.DOColor(prevColor, 0.2f));
        }
        else if (!xRayEnabled)
        {
            Sequence fadeToColorSeq = DOTween.Sequence();
            fadeToColorSeq.PrependInterval(.2f).
                Append(newMat.DOColor(prevColor, 0.2f)).
                Append(newMat.DOFade(0.65f, .15f));
        }
        return null;
    }

    //Fades the objects to white
    public IEnumerator RunFadeAnim(Material newMat, Material origMat, Material[] rendererMats, MeshRenderer renderer, int matIndex)
    {
        Color prevColor = newMat.color;

        Sequence fadeToWhiteSeq = DOTween.Sequence();
        fadeToWhiteSeq.
            Append(newMat.DOColor(Color.grey, .2f)).
            Append(origMat.DOColor(Color.grey, .2f)).
            AppendCallback(UpdateMats(newMat, rendererMats, renderer, matIndex)).
            AppendCallback(FadeToColor(newMat, prevColor));
            
        yield return null;
    }

    public IEnumerator ToggleXRay()
    {
        xRayChanging = true;
        if (xRayEnabled)
        {
            foreach (MeshRenderer renderer in holeRenderers)
            {
                var mats = renderer.materials;

                for (int i = 0; i < mats.Length; i++)
                {
                    Material newMat = new Material(defaultFloors);
                    newMat.color = mats[i].color;
                    StartCoroutine(RunFadeAnim(newMat, mats[i], mats, renderer, i));
                }
            }
            yield return new WaitForSeconds(1.5f);
            xRayEnabled = false;
            xRayChanging = false;
        }
        else if (!xRayEnabled)
        {
            foreach (MeshRenderer renderer in holeRenderers)
            {
                var mats = renderer.materials;
                for(int i = 0; i < mats.Length; i++)
                {
                    Material newMat = new Material(transparentMat);
                    newMat.color = new Color(mats[i].color.r, mats[i].color.g, mats[i].color.b, 0.65f);
                    StartCoroutine(RunFadeAnim(newMat, mats[i], mats, renderer, i));
                }
            }
            yield return new WaitForSeconds(1.5f);
            xRayEnabled = true;
            xRayChanging = false;
        }
    }
}
