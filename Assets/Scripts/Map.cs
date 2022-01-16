using UnityEngine;
using System;
using System.Collections.Generic;

public class Map
{
    private AStart _aStart;
    public int[,] mapData;
    private int _size;
    public int size{
        get{
            return _size;
        }
    }
    private System.Random _random = new System.Random();
    public Map(int size)
    {
        _aStart = new AStart();
        this._size = size;
        mapData = new int[size, size];
        for (int i = 0; i < size; i++)
        {
            mapData[0, i] = 1;
            mapData[size - 1, i] = 1;
            mapData[i, 0] = 1;
            mapData[i, size - 1] = 1;
        }
        GenMaze(1, 1, size - 2, size - 2);
    }

    private void OpenAdoor(int x1, int y1, int x2, int y2)
    {
        int pos;
        if (x1 == x2)
        {
            pos = y1 + _random.Next((y2 - y1) / 2 + 1) * 2;//在奇数位置开门
            mapData[x1, pos] = 0;
        }
        else if (y1 == y2)
        {
            pos = x1 + _random.Next((x2 - x1) / 2 + 1) * 2;
            mapData[pos, y1] = 0;
        }
        else
        {
            Debug.LogError("开门错误");
        }
    }

    private void GenMaze(int x, int y, int height, int width)
    {
        int xPos, yPos;

        if (height <= 2 || width <= 2)
            return;

        //横着画线，在偶数位置画线
        xPos = x + _random.Next(height / 2) * 2 + 1;
        for (int i = y; i < y + width; i++)
        {
            mapData[xPos, i] = 1;
        }

        //竖着画一条线，在偶数位置画线
        yPos = y + _random.Next(width / 2) * 2 + 1;
        for (int i = x; i < x + height; i++)
        {
            mapData[i, yPos] = 1;
        }

        // //随机开三扇门，左侧墙壁为1，逆时针旋转
        int isClosed = _random.Next(4) + 1;
        switch (isClosed)
        {
            case 1:
                OpenAdoor(xPos + 1, yPos, x + height - 1, yPos);// 2
                OpenAdoor(xPos, yPos + 1, xPos, y + width - 1);// 3
                OpenAdoor(x, yPos, xPos - 1, yPos);// 4
                break;
            case 2:
                OpenAdoor(xPos, yPos + 1, xPos, y + width - 1);// 3
                OpenAdoor(x, yPos, xPos - 1, yPos);// 4
                OpenAdoor(xPos, y, xPos, yPos - 1);// 1
                break;
            case 3:
                OpenAdoor(x, yPos, xPos - 1, yPos);// 4
                OpenAdoor(xPos, y, xPos, yPos - 1);// 1
                OpenAdoor(xPos + 1, yPos, x + height - 1, yPos);// 2
                break;
            case 4:
                OpenAdoor(xPos, y, xPos, yPos - 1);// 1
                OpenAdoor(xPos + 1, yPos, x + height - 1, yPos);// 2
                OpenAdoor(xPos, yPos + 1, xPos, y + width - 1);// 3
                break;
            default:
                break;
        }

        // 左上角
        GenMaze(x, y, xPos - x, yPos - y);
        // 右上角
        GenMaze(x, yPos + 1, xPos - x, width - yPos + y - 1);
        // 左下角
        GenMaze(xPos + 1, y, height - xPos + x - 1, yPos - y);
        // 右下角
        GenMaze(xPos + 1, yPos + 1, height - xPos + x - 1, width - yPos + y - 1);
    }

	public bool CanPass(int x, int y){
		return mapData[x, y] == 0;
	}

    public void GetCanPassPoint(out int x, out int y){
        do{
			x = UnityEngine.Random.Range(0, _size);
			y = UnityEngine.Random.Range(0, _size);
		}while(!CanPass(x, y));
    }

    public List<int[]> GetPath(int startX, int startY){
		int endX, endY;
		do{
			GetCanPassPoint(out endX, out endY);
		}while(endX == startX && endY == startY);
        if(Config.fastModel){
            return _aStart.GetPathFast(startX, startY, endX, endY, mapData);
        }
		return _aStart.GetPath(startX, startY, endX, endY, mapData);
	}
}