using UnityEngine;

namespace LightRef
{
    public interface IRenderType
    {
        //void Init(RenderTypeInitData data);
        void Render(RenderData data);
        Vector2Int InitRenderTypeData();
        void SourceInput(int x, int y, RenderData data, RenderTypeInitData initData);
        void RenderInCamera(RenderData data, RenderObject camera);
    }
}
