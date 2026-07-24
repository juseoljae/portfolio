using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RawImageEnabler : MonoBehaviour
{
    public RawImage targetRawImage;
    public Camera renderingCamera;

    void OnEnable()
    {
        SngCanvasManager canvasMgr = transform.root.GetComponent<SngCanvasManager>();
        if (canvasMgr)
            renderingCamera = canvasMgr.uicamera;
            
        if (renderingCamera != null && targetRawImage.texture != null)
        {
            renderingCamera.targetTexture = (RenderTexture)targetRawImage.texture;
            renderingCamera.Render();
            renderingCamera.targetTexture = null;
        }
    }
}
