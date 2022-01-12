using System;
using System.Collections.Generic;
class PathDataComp : IComparer<PathData>
{
    public int Compare(PathData x, PathData y)
    {
        if(x.F == y.F){
            return 0;
        }else{
            return x.F > y.F ? 1 : -1;
        }
    }
}

class PathData
{
    public int x, y;//顶点的X,Y坐标
    public PathData parent;//父节点
    public int F, G, H; //F = G +H
    public PathData(int x, int y, PathData parent)
    {
        this.x = x;
        this.y = y;
        this.parent = parent;
    }
}

class AStart
{
    private int[,] direction = new int[4, 2] {
        {1, 0},
        {-1, 0},
        {0, 1},
        {0, -1},
        };
    public int GetH(PathData reachableV, PathData goalV)
    {
        int disX = reachableV.x - goalV.x;
        int disY = reachableV.y - goalV.y;
        return (Math.Abs(disX) + Math.Abs(disY)) * 10;
    }

    public int GetG(PathData p)
    {
        if (p.parent == null) return 0;

        return p.parent.G + 10;
    }

    //从openList中查找F值最小的顶点
    public PathData GetMinFFromOpenList(List<PathData> openList)
    {
        int min = openList[0].F;
        PathData pathDataOfMinF = openList[0];
        for (int i = 1; i < openList.Count; ++i)
        {
            if (openList[i].F < min)
            {
                min = openList[i].F;
                pathDataOfMinF = openList[i];
            }
        }
        return pathDataOfMinF;
    }

    public bool IsInList(int x, int y, List<PathData> list)
    {
        foreach (PathData v in list)
        {
            if (v.x == x && v.y == y)
            {
                return true;
            }
        }
        return false;
    }


    public PathData GetPathDataFromList(int x, int y, List<PathData> list)
    {
        foreach (PathData v in list)
        {
            if (v.x == x && v.y == y)
            {
                return v;
            }
        }
        return null;
    }

    public List<int[]> GetPathFast(int startX, int startY, int targetX, int targetY, int[,] mapData, bool canPenetrate = false)
    {
        PathData startData = new PathData(startX, startY, null);
        PathData endData = new PathData(targetX, targetY, null);
        return GetPathFast(startData, endData, mapData, canPenetrate);
    }

    public PathData GetPathDataFromQueue(int x, int y, PriorityQueue<PathData> queue){
        return GetPathDataFromList(x, y, queue.GetQueue());
    }

    public List<int[]> GetPathFast(PathData startData, PathData endData, int[,] mapData, bool canPenetrate)
    {
        PriorityQueue<PathData> openList = new PriorityQueue<PathData>(new PathDataComp());
        // List<PathData> openList = new List<PathData>();
        HashSet<int> closeList = new HashSet<int>();

        //初始化closeList
        //closeList.Add(startData);

        //初始化openList,将起点添加进去
        openList.EnQueue(startData);
        int width = mapData.GetLength(0);
        int hieght = mapData.GetLength(1);

        while (openList.GetCount() > 0)
        {//当openList不为空时


            //遍历openList,查找F值最小的顶点
            PathData minVertex = openList.DeQueue();

            if (minVertex.x == endData.x && minVertex.y == endData.y)
            {
                endData.parent = minVertex.parent;
                break;
            }

            //将F值最小的节点移入close表中，并将其作为当前要处理的节点
            closeList.Add(minVertex.x + minVertex.y * hieght);

            //对当前要处理节点的可到达节点进行检查,
            for (int i = 0; i < 4; ++i)
            {
                int nextX = minVertex.x + direction[i, 0];
                int nextY = minVertex.y + direction[i, 1];

                //下一个顶点没有越界
                if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < hieght)
                {
                    int key = nextX + nextY * hieght;
                    if ((canPenetrate || mapData[nextX, nextY] == 0) && !closeList.Contains(key))
                    {   //不是障碍物并且不在close列表中
                        //判断是否在openList中
                        PathData vex = GetPathDataFromQueue(nextX, nextY, openList);
                        if (vex == null)
                        {
                            //不在openList中，则加入，并将当前处理节点作为它的父节点，并计算其F,G,H
                            vex = new PathData(nextX, nextY, minVertex);
                            vex.G = GetG(vex);
                            vex.H = GetH(vex, endData);
                            vex.F = vex.G + vex.H;
                            openList.EnQueue(vex);
                        }
                        else
                        {
                            int newG = 0;
                            newG = minVertex.G + 10;

                            if (newG < vex.G)
                            {
                                vex.parent = minVertex;
                                vex.G = newG;
                                vex.F = vex.G + vex.H;
                            }
                        }
                    }
                }
            }
        }

        List<int[]> way = new List<int[]>();
        PathData v = endData;
        while (v.parent != null)
        {
            int[] pos = new int[2];
            pos[0] = v.x;
            pos[1] = v.y;
            way.Add(pos);
            v = v.parent;
        }

        way.Reverse();
        return way;
    }

    public List<int[]> GetPath(int startX, int startY, int targetX, int targetY, int[,] mapData, bool canPenetrate = false)
    {
        PathData startData = new PathData(startX, startY, null);
        PathData endData = new PathData(targetX, targetY, null);
        return GetPath(startData, endData, mapData, canPenetrate);
    }

    public List<int[]> GetPath(PathData startData, PathData endData, int[,] mapData, bool canPenetrate)
    {
        List<PathData> openList = new List<PathData>();
        List<PathData> closeList = new List<PathData>();

        //初始化closeList
        //closeList.Add(startData);

        //初始化openList,将起点添加进去
        openList.Add(startData);

        while (openList.Count > 0)
        {//当openList不为空时


            //遍历openList,查找F值最小的顶点
            PathData minVertex = GetMinFFromOpenList(openList);

            if (minVertex.x == endData.x && minVertex.y == endData.y)
            {
                endData.parent = minVertex.parent;
                break;
            }

            //将F值最小的节点移入close表中，并将其作为当前要处理的节点
            openList.Remove(minVertex);
            closeList.Add(minVertex);

            //对当前要处理节点的可到达节点进行检查,
            for (int i = 0; i < 4; ++i)
            {
                int nextX = minVertex.x + direction[i, 0];
                int nextY = minVertex.y + direction[i, 1];

                //下一个顶点没有越界
                if (nextX >= 0 && nextX < mapData.GetLength(0) && nextY >= 0 && nextY < mapData.GetLength(1))
                {
                    if ((canPenetrate || mapData[nextX, nextY] == 0) && !IsInList(nextX, nextY, closeList))
                    {   //不是障碍物并且不在close列表中
                        //判断是否在openList中
                        PathData vex = GetPathDataFromList(nextX, nextY, openList);
                        if (vex == null)
                        {
                            //不在openList中，则加入，并将当前处理节点作为它的父节点，并计算其F,G,H
                            vex = new PathData(nextX, nextY, minVertex);
                            vex.G = GetG(vex);
                            vex.H = GetH(vex, endData);
                            vex.F = vex.G + vex.H;
                            openList.Add(vex);
                        }
                        else
                        {
                            int newG = 0;
                            newG = minVertex.G + 10;

                            if (newG < vex.G)
                            {
                                vex.parent = minVertex;
                                vex.G = newG;
                                vex.F = vex.G + vex.H;
                            }
                        }
                    }
                }
            }
        }

        List<int[]> way = new List<int[]>();

        PathData v = endData;
        while (v.parent != null)
        {
            int[] pos = new int[2];
            pos[0] = v.x;
            pos[1] = v.y;
            way.Insert(0, pos);
            v = v.parent;
        }
        return way;
    }
}