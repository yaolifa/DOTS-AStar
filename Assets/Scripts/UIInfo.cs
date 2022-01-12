using UnityEngine;
using UnityEngine.UI;

public class UIInfo: MonoBehaviour{
    public int roleCount = 10;
    public int mapSize = 99;
    public Text textMapSize;
    public Text textRoleCount;
    public Button buttonCreatRole;
    public Button buttonCreatMap;
    public Toggle toggleFast;

    private void Start() {
        buttonCreatRole.onClick.AddListener(OnClickCreatRole);
        buttonCreatMap.onClick.AddListener(OnClickCreatMap);
        toggleFast.isOn = Config.fastModel;
        toggleFast.onValueChanged.AddListener(OnToggleChange);
    }

    private void OnEnable(){
        EventManager.instance.AddListener<int>(EventName.UpdateMapSize, UpdateMapSize);
        EventManager.instance.AddListener<int>(EventName.UpdateRoleCount, UpdateRoleCount);
    }

    private void OnDisable() {
        EventManager.instance.RemoveListener<int>(EventName.UpdateMapSize, UpdateMapSize);
        EventManager.instance.RemoveListener<int>(EventName.UpdateRoleCount, UpdateRoleCount);
    }

    private void UpdateMapSize(int size){
        textMapSize.text = size.ToString();
    }

    private void UpdateRoleCount(int count){
        textRoleCount.text = count.ToString();
    }

    private void OnClickCreatRole(){
        EventManager.instance.DispatchEvent<int>(EventName.AddRole, roleCount);
    }

    private void OnClickCreatMap(){
        EventManager.instance.DispatchEvent<int>(EventName.InitMap, mapSize);
    }

    private void OnToggleChange(bool off){
        Config.fastModel = off;
    }
}