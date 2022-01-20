using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T>
{
    IComparer<T> comparer;
    private List<T> list;
    public PriorityQueue(IComparer<T> comparer)
    {
        this.comparer = (comparer == null) ? Comparer<T>.Default : comparer;
        list = new List<T>();
    }
    /// <summary>
    /// 入队
    /// </summary>
    public void EnQueue(T key)
    {
        list.Add(key);
        UpAdjust();
    }
    public T DeQueue()
    {
        if (list.Count <= 0)
        {
            Debug.LogError("Queue is empty!");
            return default(T);
        }
        // 获取堆顶元素
        T head = list[0];
        // 让最后一个元素移动到堆顶
        int lastIndex = list.Count - 1;
        list[0] = list[lastIndex];
        list.RemoveAt(lastIndex);
        if (list.Count > 0) {
            DownAdjust();
        }
        return head;
    }
    /// <summary>
    /// 上浮调整
    /// </summary>
    private void UpAdjust()
    {
        int childIndex = list.Count - 1;
        int parentIndex = (childIndex - 1) / 2;
        // temp保存插入的叶子节点值，用于最后的赋值
        T temp = list[childIndex];
        while (childIndex > 0 && comparer.Compare(temp, list[parentIndex]) < 0)
        {
            list[childIndex] = list[parentIndex];
            childIndex = parentIndex;
            parentIndex = (parentIndex - 1) / 2;
        }
        list[childIndex] = temp;
    }
    /// <summary>
    /// 下沉调整
    /// </summary>
    public void DownAdjust()
    {
        // temp 保存父节点的值，用于最后的赋值
        int parentIndex = 0;
        T temp = list[parentIndex];
        int childIndex = 1;
        int size = list.Count;
        while (childIndex < size)
        {
            if (childIndex + 1 < size && comparer.Compare(list[childIndex + 1] , list[childIndex]) < 0)
            {
                childIndex++;
            }

            if (comparer.Compare(temp, list[childIndex]) < 0)
            {
                break;
            }
            list[parentIndex] = list[childIndex];
            parentIndex = childIndex;
            childIndex = 2 * childIndex + 1;
        }
        list[parentIndex] = temp;
    }

    public int GetCount(){
        return list.Count;
    }

    public List<T> GetQueue(){
        return list;
    }
}