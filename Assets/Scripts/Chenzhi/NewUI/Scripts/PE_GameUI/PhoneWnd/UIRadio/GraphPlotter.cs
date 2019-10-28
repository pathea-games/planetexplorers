using System;
using UnityEngine;

/// <summary>
/// lz-2016.12.02 采集音频的频谱数据，多种方式绘制，输出像素数据
/// </summary>
public class GraphPlotter
{
    public enum GraphShapeType
    {
        TopAndBottom,
        Top,
        Grid
    }

    public Color BackgroundColor = new Color(0, 0, 0, 0);
    public Color32 TopColor;
    public Color32 BottomColor;

    public int TextureWidth = 16;
    public int TextureHeight = 16;
    public bool NormalizeGraph=true;


    #region public methods
    /// <summary>
    ///     |
    ///    |||
    /// |||||||||
    ///   |||||
    ///     |
    /// </summary>
    public void PlotGraph(float[] data, float[] plotData,Color32[] colors)
    {
        int size = data.Length;
        float min = 0;
        float max = 0;


        float index;
        int i;

        float offset = Mathf.Max(1, size / (float)TextureWidth);

        for (index = 0, i = 0; i < size; index += offset, i = (int)index)
        {
            plotData[i] = data[i];
        }

        //lz-2016.12.02 使用标准图
        if (NormalizeGraph)
        {
            //lz-2016.12.02 找出最大最小值
            for (index = 0, i = 0; i < size; index += offset, i = (int)index)
            {
                float val = plotData[i];
                if (val > 0f)
                {
                    if (val > max)
                    {
                        max = val;
                    }
                }
                else
                {
                    val = -val;
                    if (val > min)
                    {
                        min = val;
                    }
                }
            }
            //lz-2016.12.02 数据处理为-1到1
            for (index = 0, i = 0; i < size; index += offset, i = (int)index)
            {
                float val = plotData[i];
                if (val > 0f)
                {
                    if (max != 0f)
                    {
                        val /= max;
                    }
                }
                else
                {
                    if (min != 0f)
                    {
                        val /= min;
                    }
                }

                plotData[i] = val;
            }
        }

        int textureSizeHalf = TextureHeight / 2;
        int textureSizeHalfMinusOne = textureSizeHalf - 1;

        int x;
        for (x = 0, index = 0, i = 0; i < size && x < TextureWidth; index += offset, i = (int)index, ++x)
        {
            float val = plotData[i];
            int v = (int)(val * textureSizeHalf);
            int y = textureSizeHalf;
            while (y >= 0)
            {
                float t = y / (float)textureSizeHalf;
                if (v > 0 &&
                    y <= v)
                {
                    SetColors(colors, x, textureSizeHalf + y, Color.Lerp(BottomColor, TopColor, t));
                    SetColors(colors, x, textureSizeHalfMinusOne - y, BackgroundColor);
                }
                else if (v < 0 &&
                    y <= -v)
                {
                    SetColors(colors, x, textureSizeHalf + y, BackgroundColor);
                    SetColors(colors, x, textureSizeHalfMinusOne - y, Color.Lerp(BottomColor,TopColor, t));
                }
                else
                {
                    SetColors(colors, x, textureSizeHalfMinusOne + y, BackgroundColor);
                    SetColors(colors, x, textureSizeHalf - y, BackgroundColor);
                }
                --y;
            }
        }
    }

    /// <summary>
    ///    ||
    ///   ||||
    /// |||||||||
    /// </summary>
    public void PlotGraph2(float[] data, float[] plotData,Color32[] colors)
    {
        float index;
        int i;
        int size = data.Length;

        float offset = size / (float)TextureWidth;

        for (index = 0, i = 0; i < size; index += offset, i = (int)index)
        {
            plotData[i] = Mathf.Abs(data[i]);
        }

        if (NormalizeGraph)
        {
            float max = 0;
            for (index = 0, i = 0; i < size; index += offset, i = (int)index)
            {
                float val = plotData[i];
                if (val > max)
                {
                    max = val;
                }
            }

            for (index = 0, i = 0; i < size; index += offset, i = (int)index)
            {
                float val = plotData[i];
                if (max != 0f)
                {
                    val /= max;
                }

                plotData[i] = val;
            }
        }

        int x;
        for (x = 0, index = 0, i = 0; i < size && x < TextureWidth; index += offset, i = (int)index, ++x)
        {
            float val = plotData[i];
            int v = (int)(val * TextureHeight);
            int y = TextureHeight - 1;
            while (y >= 0)
            {
                float t = y / (float)TextureHeight;
                if (v > 0 &&
                    y <= v)
                {
                    SetColors(colors, x, y, Color32.Lerp(BottomColor, TopColor, t));
                }
                else
                {
                    SetColors(colors, x, y, BackgroundColor);
                }
                --y;
            }
        }
    }

    /// <summary>
    ///     ■ ■
    ///   ■ ■ ■ ■ 
    /// ■ ■ ■ ■ ■ ■
    /// </summary>
    public void PlotGraph3(float[] data, float[] plotData, int xGridCount, int yGridCount, int gridBorderX, int gridBorderY, Color32[] colors)
    {
        int size = data.Length;

        plotData = new float[xGridCount];

        int blockSize = size / xGridCount;
        float blockTotal = 0f;
        float temp = 0f;
        float max = 0;

        //lz-2016.12.01 求出每一块的平均数,和所有块的最大数
        for (int i = 0; i < xGridCount; i++)
        {
            blockTotal = 0;
            //lz-2016.12.01 数据分块
            for (int j = i * blockSize; j < (i + 1) * blockSize; ++j)
            {
                blockTotal += Mathf.Abs(data[j]);
            }
            temp = blockTotal / blockSize;
            plotData[i] = temp;
            if (NormalizeGraph)
            {
                if (temp > max) max = temp;
            }
        }
        if (NormalizeGraph)
        {
            //lz-2016.12.01 求出每一块的最大数,和所有块的最大数
            //for (int i = 0; i < xGridCount; i++)
            //{
            //    tempCount = 0;
            //    //lz-2016.12.01 数据分块
            //    for (int j = i * blockSize; j < (i + 1) * blockSize; ++j)
            //    {
            //        temp= data[j];
            //        if (temp > tempCount)
            //            tempCount = temp;
            //    }
            //    plotData[i] = tempCount;
            //    if (tempCount > max) max = tempCount;
            //}

            //lz-2016.12.01 0-1处理
            for (int i = 0; i < xGridCount; ++i)
            {
                float val = plotData[i];
                if (max != 0f)
                {
                    val /= max;
                }

                plotData[i] = val;
            }
        }

        //lz-2016.12.01 映射到颜色数组
        int girdWidth = (TextureWidth-gridBorderX * (xGridCount - 1))/ xGridCount;
        int gridHeight = (TextureHeight- gridBorderY * (yGridCount - 1))/ yGridCount;

        int ColX = 0;
        int ColY = 0;
        int dataIndex = 0;
        bool m_HasXGrid = false;
        bool m_HasYGrid = false;
        float curVal = 0f;
        int allGridHeight = yGridCount * gridHeight;
        int curGridWidth = 0;
        int curGridHeight = 0;
        int curPoint = 0;

        for (int gridX = 0; gridX < xGridCount * 2 - 1; gridX++)
        {
            m_HasXGrid = (gridX % 2 == 0);
            curGridWidth = m_HasXGrid ? girdWidth : gridBorderX;
            int topPoint = 0;
            if (m_HasXGrid)
            {
                curVal = plotData[dataIndex];
                topPoint = (int)(curVal * allGridHeight);
                curPoint = allGridHeight;
                dataIndex++;
            }

            ColY = TextureHeight - 1;
            for (int gridY = 0; gridY < yGridCount * 2 - 1; gridY++)
            {
                m_HasYGrid = (gridY % 2 == 0);
                curGridHeight = m_HasYGrid ? gridHeight : gridBorderY;
                if (m_HasYGrid && m_HasXGrid)
                { 
                    //lz-2016.12.01 有数据的块
                    for (int y = 0; y < curGridHeight; y++)
                    {
                        for (int x = 0; x < curGridWidth; x++)
                        {
                            float t = curPoint / (float)allGridHeight;
                            if (topPoint > 0 && curPoint <= topPoint)
                                SetColors(colors, ColX + x, ColY, Color32.Lerp(BottomColor, TopColor, t));
                            else
                                SetColors(colors, ColX + x, ColY, BackgroundColor);
                        }
                        ColY--;
                        curPoint--;
                    }
                }
                else
                {
                    //lz-2016.12.01 空白块
                    for (int y = 0; y < curGridHeight; y++)
                    {
                        for (int x = 0; x < curGridWidth; x++)
                        {
                            SetColors(colors, ColX + x, ColY, BackgroundColor);
                        }
                        ColY--;
                    }
                }
            }
            while (ColY >= 0)
            {
                //lz-2016.12.01 清除不能整除的y部分
                for (int x = 0; x < curGridWidth; x++)
                {
                    SetColors(colors, ColX + x, ColY, BackgroundColor);
                }
                ColY--;
            }
            ColX += curGridWidth;
        }

        //lz-2016.12.01 清除不能整除的x部分
        if (ColX< TextureWidth)
        {
            for (ColY = TextureHeight-1; ColY >=0; --ColY)
            {
                for (int x = ColX; x < TextureWidth; ++x)
                {
                    SetColors(colors, x, ColY, BackgroundColor);
                }
            }
           
        }
    }

    #endregion

    #region private methods
    void SetColors(Color32[] colors, int x, int y, Color32 color)
    {
        if (null == colors)
        {
            return;
        }

        int index = x + y * TextureWidth;
        if (index >= 0 &&
            (index < TextureHeight * TextureWidth))
        {
            colors[index] = color;
        }
    }
    #endregion
}