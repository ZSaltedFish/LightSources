using UnityEngine;

namespace LightRef
{
    public class CameraRenderTypeInitData : RenderTypeInitData
    {
        public Camera Camera;
        public Vector3 Forward;
        public Vector3 Posistion;
        public float Near;
        public int Width, Height;
    }
}
