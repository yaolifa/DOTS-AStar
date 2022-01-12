using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject role;
    public GameObject plane;
    public GameObject obstacle;
    public Transform obstacleRoot;
    public Transform cameraTransform;
    private List<GameObject> _obstacles;
    private List<GameObject> _roles;
    private Map _map;

    // Start is called before the first frame update
    void Start()
    {
        _obstacles = new List<GameObject>();
        _roles = new List<GameObject>();
        // RandomCreaterPlane();
    }

    private void OnEnable() {
        EventManager.instance.AddListener<int>(EventName.AddRole, CreaterRole);
        EventManager.instance.AddListener<int>(EventName.InitMap, InitPlane);
    }

    private void OnDisable() {
        EventManager.instance.RemoveListener<int>(EventName.AddRole, CreaterRole);
        EventManager.instance.RemoveListener<int>(EventName.InitMap, InitPlane);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A)){
            RandomCreaterPlane();
        }else if(Input.GetKeyDown(KeyCode.B)){
            CreaterRole(1);
        }
    }

    public void ClearRole(){
        for(int i = 0; i < _roles.Count; i++){
            Destroy(_roles[i]);
        }
        _roles.Clear();
    }

    public void CreaterRole(int count){
        if(_map == null) return;
        for(int i = 0; i < count; i++){
            GameObject obj = GameObject.Instantiate(role);
            int x, y;
            _map.GetCanPassPoint(out x, out y);
            Role roleScript = obj.AddComponent<Role>();
            roleScript.Init(x, y, _map);
            _roles.Add(obj);
        }
        EventManager.instance.DispatchEvent<int>(EventName.UpdateRoleCount, _roles.Count);
    }
    public void RandomCreaterPlane(){
        int count = Random.Range(10, 100) * 2 + 1;
        Debug.LogError(count);
        InitPlane(count);
    }

    public void InitPlane(int size){
        ClearRole();
        cameraTransform.position = new Vector3(size / 2f, 250, size / 2f);
        for(int i = 0; i < _obstacles.Count; i++){
            Destroy(_obstacles[i]);
        }
        _obstacles.Clear();

        plane.transform.localScale = new Vector3(size, 1, size);
        _map = new Map(size);
        for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
                if(_map.mapData[i, j] == 1){
                    GameObject obj = GameObject.Instantiate(obstacle, obstacleRoot);
                    obj.transform.position = new Vector3(i, 0, j);
                    _obstacles.Add(obj);
                }

			}
		}
        StaticBatchingUtility.Combine(obstacleRoot.gameObject);
        EventManager.instance.DispatchEvent<int>(EventName.UpdateMapSize, size);
    }
}
