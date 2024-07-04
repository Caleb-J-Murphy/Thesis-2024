using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DocumentContent : MonoBehaviour
{
    [SerializeField] private TextAsset guide;
    [TextArea(10, 20)]
    [SerializeField] private string content;
    [Space]
    [SerializeField] private TMP_Text guideObject;

    private void Awake()
    {
        content = guide.text;
        SetupContent();

    }

    private void SetupContent()
    {
        guideObject.text = content;
    }
}
