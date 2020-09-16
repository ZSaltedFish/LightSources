using System.Collections.Generic;
using UnityEngine;

namespace LightRef
{
    public class RenderDataFragment
    {
        private RenderData _now;
        private Stack<RenderData> _stack;

        public RenderDataFragment()
        {
            _stack = new Stack<RenderData>();
        }

        public RenderData GetNewOne(int x, int y)
        {
            RenderData data = RenderData.Apply();
            if (_now != null)
            {
                _stack.Push(_now);
            }
            _now = data;
            _now.Init(x, y);
            return data;
        }

        public RenderData GetNow()
        {
            return _now;
        }

        public void DisposeNow()
        {
            if (_now == null)
            {
                return;
            }

            _now.Dispose();
            if (_stack.Count == 0)
            {
                _now = null;
            }
            else
            {
                _now = _stack.Pop();
            }
        }
    }
}
