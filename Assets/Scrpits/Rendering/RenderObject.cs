using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightRef
{
    public enum RenderObjectType
    {
        Mesh,
        PointLight,
        Camera,

    }

    public class RenderObject : MonoBehaviour
    {
        public RenderObjectType ROType { get; set; }
        public RenderDataFragment Data = new RenderDataFragment();
        private IRenderType _render;
        private RenderTypeInitData _initData;

        #region PointLightParam
        public Light SrcLight;
        public float LightPower;
        #endregion
        #region RelfectionRenderParam
        public MaterialScript SrcMatrial;
        #endregion
        #region CameraRenderType
        public Camera SrcCamera;
        #endregion

        public Vector3 Posistion { get; private set; }

        public bool TryGetRenderType(out Type type)
        {
            type = typeof(RenderObject);
            if (_render == null)
            {
                return false;
            }
            type = _render.GetType();
            return true;
        }
        public void TryInitialize(RenderObjectType roType)
        {
            ROType = roType;
            Posistion = transform.position;

            switch (ROType)
            {
                case RenderObjectType.Mesh:
                    _render = InitReflectionRenderType();
                    break;
                case RenderObjectType.PointLight:
                    _render = InitPointlightType();
                    break;
                case RenderObjectType.Camera:
                    _render = InitCameraRenderType();
                    break;
                default: throw new ArgumentException($"没有定于对{ROType}的渲染方法");

            }
        }

        public void InputSource(int x, int y, RenderData data)
        {
            _render.SourceInput(x, y, data, _initData);
        }

        public void Render(RenderObject cameraObj)
        {
            Vector2Int size = _render.InitRenderTypeData();
            _render.RenderInCamera(Data.GetNewOne(size.x, size.y), cameraObj);
            _render.Render(Data.GetNow());
        }

        private CameraRenderType InitCameraRenderType()
        {
            CameraRenderType type = new CameraRenderType();
            CameraRenderTypeInitData initData = new CameraRenderTypeInitData()
            {
                Camera = SrcCamera
            };
            initData.Forward = SrcCamera.transform.forward;
            initData.Posistion = SrcCamera.transform.position;
            initData.Near = SrcCamera.nearClipPlane;

            _initData = initData;
            type.CameraInit(initData);
            Vector2Int size = type.InitRenderTypeData();
            Data.GetNewOne(size.x, size.y);
            return type;
        }

        private PointLightRender InitPointlightType()
        {
            PointLightRender render = PointLightRender.Instance;
            PointLightRenderInitData initData = new PointLightRenderInitData()
            {
                LightColor = SrcLight.color,
                LightPoint = SrcLight.transform.position,
                LightPower = LightPower
            };
            _initData = initData;
            return render;
        }

        private ReflectionRenderType InitReflectionRenderType()
        {
            ReflectionRenderType render = ReflectionRenderType.Instance;
            ReflectionRenderTypeInitData initData = new ReflectionRenderTypeInitData()
            {
                LambertValue = SrcMatrial.LambertLerp,
            };

            _initData = initData;
            return render;
        }

        public void CameraRender()
        {
            _render.Render(Data.GetNow());
        }
    }
}
