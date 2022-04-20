using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandColor : MonoBehaviour
{
    Pen pen;
    public Material mat;
    private Color outlineColor;
    // Start is called before the first frame update
    void Start()
    {
        pen = FindObjectOfType<Pen>();
        mat = GetComponent<Renderer>().material;
        mat.EnableKeyword("_RIMCOLOR");
    }

    // Update is called once per frame
    void Update()
    {
        SetOutline();
    }

    private void SetOutline()
    {
        outlineColor = pen.GetColor();
        mat.SetColor("_RIMCOLOR", outlineColor);
    }
}
