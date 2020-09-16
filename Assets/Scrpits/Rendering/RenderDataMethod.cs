using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LightRef
{
    public class RenderDataMethod
    {
        public const string SOURCE_PATH = "K:/TempFile";
        public const float POWER_MIN = 0.001f;
        public static int INDEX_COUNT = -1;
        /// <summary>
        /// 处理图片，返回结果为文件链接
        /// </summary>
        /// <param name="data">渲染图片数据</param>
        /// <returns></returns>
        public static Queue<string> Excute(RenderData data, RenderObject camera)
        {
            Queue<string> paths = new Queue<string>();
            for (int x = 0; x < data.Width; ++x)
            {
                for (int y = 0; y < data.Height; ++y)
                {
                    Color color = data.RenderTex[x, y];
                    if (RenderHelper.ColorFliter(color))
                    {
                        if (color.a == RenderHelper.CAMERA_TRIANGLE_VALUE)
                        {
                            camera.InputSource(x, y, data);
                        }
                        else
                        {
                            int triangleIndex = data.TriangleIndexes[x, y];
                            try
                            {
                                Triangle triangle = StaticRenderData.Triangles[triangleIndex];
                                triangle.SourceRenderObject.InputSource(x, y, data);
                                triangle.SourceRenderObject.Render(camera);
                                string path = Save(triangle.SourceRenderObject.Data.GetNow());
                                paths.Enqueue(path);
                            }
                            catch (IndexOutOfRangeException err)
                            {
                                Debug.LogError($"找不到索引为：{triangleIndex}的三角形.");
                                throw err;
                            }
                        }
                    }
                }
            }
            return paths;
        }

        /// <summary>
        /// 深度搜索(递归)
        /// </summary>
        /// <param name="data">渲染图</param>
        /// <param name="camera"></param>
        public static void ExcuteDeeply(RenderData data, RenderObject camera)
        {
            for (int x = 0; x < data.Width; ++x)
            {
                for (int y = 0; y < data.Height; ++y)
                {
                    if (data.TriangleIndexes[x, y] == RenderHelper.CAMERA_TRIANGLE_VALUE)
                    {
                        camera.InputSource(x, y, data);
                        camera.CameraRender();
                    }
                    else
                    {
                        Color color = data.RenderTex[x, y];
                        if (RenderHelper.ColorFliter(color))
                        {
                            int triangleIndex = data.TriangleIndexes[x, y];
                            try
                            {
                                Triangle triangle = StaticRenderData.Triangles[triangleIndex];

                                triangle.SourceRenderObject.InputSource(x, y, data);
                                triangle.SourceRenderObject.Render(camera);

                                ExcuteDeeply(triangle.SourceRenderObject.Data.GetNow(), camera);
                                triangle.SourceRenderObject.Data.DisposeNow();
                            }
                            catch (IndexOutOfRangeException err)
                            {
                                Debug.LogError($"找不到索引为：{triangleIndex}的三角形.");
                                throw err;
                            }
                        }
                    }
                }
            }
        }

        public static RenderData OpenFile(string path)
        {
            RenderData data = RenderData.Apply();
            using (FileStream file = new FileStream(path, FileMode.Open))
            {
                byte[] bytes = new byte[file.Length];
                file.Read(bytes, 0, bytes.Length);
                data.FromBytes(bytes);
            }

            return data;
        }

        public static string Save(RenderData data)
        {
            string path = $"{SOURCE_PATH}/Temp{++INDEX_COUNT}.dat";
            using (FileStream file = new FileStream(path, FileMode.Create))
            {
                byte[] bytes = data.ToData();
                file.Write(bytes, 0, bytes.Length);
            }

            return path;
        }
    }
}
