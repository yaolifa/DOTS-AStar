using System;
using System.Collections.Generic;
/// <summary>
/// 事件管理类，目前最高只支持两个参数事件
/// </summary>
public class EventManager : Singleton<EventManager>
{
    private Dictionary<EventName, Delegate> _eventDic = new Dictionary<EventName, Delegate>();

    #region 添加事件
    private void _onAddListener(EventName name, Delegate action){
        if (!_eventDic.ContainsKey(name))
        {
            _eventDic.Add(name, null);
        }
        Delegate d = _eventDic[name];
        if (d != null && d.GetType() != action.GetType())
        {
            throw new Exception(string.Format("尝试为事件{0}添加不同类型的委托", name));
        }
    }

    public void AddListener(EventName name, Action action){
        _onAddListener(name, action);
        _eventDic[name] = (Action)_eventDic[name] + action;
    }

    public void AddListener<T>(EventName name, Action<T> action){
        _onAddListener(name, action);
        _eventDic[name] = (Action<T>)_eventDic[name] + action;
    }

    public void AddListener<T1, T2>(EventName name, Action<T1, T2> action){
        _onAddListener(name, action);
        _eventDic[name] = (Action<T1, T2>)_eventDic[name] + action;
    }
    #endregion

    #region 移除事件
    private void _onRemoveListener(EventName name, Delegate action)
    {
        if (_eventDic.ContainsKey(name))
        {
            Delegate d = _eventDic[name];
            if (d == null)
            {
                throw new Exception(string.Format("移除监听错误：事件{0}没有对应的委托", name));
            }
            else if (d.GetType() != action.GetType())
            {
                throw new Exception(string.Format("移除监听错误：尝试为事件{0}移除不同类型的委托", name));
            }
        }
        else
        {
            throw new Exception(string.Format("移除监听错误：没有事件{0}", name));
        }
    }

    private void _onRemoveListenerEnd(EventName name){
        if(_eventDic[name] == null){
            _eventDic.Remove(name);
        }
    }
    public void RemoveListener(EventName name, Action action){
        _onRemoveListener(name, action);
        _eventDic[name] = (Action)_eventDic[name] - action;
        _onRemoveListenerEnd(name);
    }

    public void RemoveListener<T>(EventName name, Action<T> action){
        _onRemoveListener(name, action);
        _eventDic[name] = (Action<T>)_eventDic[name] - action;
        _onRemoveListenerEnd(name);
    }

    public void RemoveListener<T1, T2>(EventName name, Action<T1, T2> action){
        _onRemoveListener(name, action);
        _eventDic[name] = (Action<T1, T2>)_eventDic[name] - action;
        _onRemoveListenerEnd(name);
    }
    #endregion

    #region 派发事件
    public void DispatchEvent(EventName name)
    {
        Delegate d;
        if (_eventDic.TryGetValue(name, out d))
        {
            Action callBack = d as Action;
            if (callBack != null)
            {
                callBack();
            }
            else
            {
                throw new Exception(string.Format("事件{0}参数错误", name));
            }
        }
    }

    public void DispatchEvent<T>(EventName name, T arg)
    {
        Delegate d;
        if (_eventDic.TryGetValue(name, out d))
        {
            Action<T> callBack = d as Action<T>;
            if (callBack != null)
            {
                callBack(arg);
            }
            else
            {
                throw new Exception(string.Format("事件{0}参数错误", name));
            }
        }
    }

    public void DispatchEvent<T1, T2>(EventName name, T1 arg1, T2 arg2)
    {
        Delegate d;
        if (_eventDic.TryGetValue(name, out d))
        {
            Action<T1, T2> callBack = d as Action<T1, T2>;
            if (callBack != null)
            {
                callBack(arg1, arg2);
            }
            else
            {
                throw new Exception(string.Format("事件{0}参数错误", name));
            }
        }
    }
    #endregion
}