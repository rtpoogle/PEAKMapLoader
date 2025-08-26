using Photon.Pun;
using UnityEngine;

public class PEAKCustomSegment : MonoBehaviour
{
    public enum _ReferenceOperation
    {
        Before,
        After,
        Replace
    }

    public string Name = string.Empty;

    [Header("Placement Options")]
    public MapSegment ReferenceBiome = MapSegment.Tropics;

    [Tooltip("Before Beach = ThisMap->Beach->Tropics\nReplace Beach = ThisMap->Tropics\nAfter Beach = Beach->ThisMap->Tropics")]
    public _ReferenceOperation Placement = _ReferenceOperation.Replace;

    [Tooltip("The start anchor point [see example project]")]
    public GameObject StartAnchor;

    [Tooltip("The end anchor point [see example project]")]
    public GameObject EndAnchor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
