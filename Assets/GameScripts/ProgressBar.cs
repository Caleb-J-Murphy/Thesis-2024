using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    public void SetProgressPercent(float progress) {
        transform.localScale = new Vector3(progress, 1f, 1f);
    }
}
