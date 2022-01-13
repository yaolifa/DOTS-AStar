using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
public class Main : MonoBehaviour
{
    public GameObject role;
    public GameObject plane;
    public GameObject obstacle;
    public Transform obstacleRoot;
    public Transform cameraTransform;
    private List<GameObject> _obstacles;
    private List<GameObject> _roles;
    private List<ECSRole> _ECSRoles;
    private Map _map;
    private Entity _roleEntity;
    private EntityManager _entityManager;

    // Start is called before the first frame update
    void Start()
    {
        _obstacles = new List<GameObject>();
        _roles = new List<GameObject>();
        _ECSRoles = new List<ECSRole>();
        // RandomCreaterPlane();
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(
            World.DefaultGameObjectInjectionWorld, null);

        _roleEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(role, settings);
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<int>(EventName.AddRole, CreaterRole);
        EventManager.instance.AddListener<int>(EventName.InitMap, InitPlane);
        EventManager.instance.AddListener(EventName.ECSModel, ChangeToECS);
    }

    private void OnDisable()
    {
        EventManager.instance.RemoveListener<int>(EventName.AddRole, CreaterRole);
        EventManager.instance.RemoveListener<int>(EventName.InitMap, InitPlane);
        EventManager.instance.RemoveListener(EventName.ECSModel, ChangeToECS);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            RandomCreaterPlane();
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            CreaterRole(1);
        }
    }

    public void ClearRole()
    {
        if (Config.isECSModel)
        {
            for (int i = 0; i < _ECSRoles.Count; i++)
            {
                _entityManager.DestroyEntity(_ECSRoles[i].entity);
                Destroy(_ECSRoles[i]);
            }
            _ECSRoles.Clear();
            EventManager.instance.DispatchEvent<int>(EventName.UpdateRoleCount, _ECSRoles.Count);
        }
        else
        {
            for (int i = 0; i < _roles.Count; i++)
            {
                Destroy(_roles[i]);
            }
            _roles.Clear();
            EventManager.instance.DispatchEvent<int>(EventName.UpdateRoleCount, _roles.Count);
        }
    }

    public void CreaterRole(int count)
    {
        if (_map == null) return;

        if (Config.isECSModel)
        {
            for (int i = 0; i < count; i++)
            {
                Entity entity = _entityManager.Instantiate(_roleEntity);
                ECSRole eCSRole = new ECSRole();
                eCSRole.Convert(entity, _entityManager, null);
                eCSRole.Init(0, 0, 1);
                _ECSRoles.Add(eCSRole);
            }
            EventManager.instance.DispatchEvent<int>(EventName.UpdateRoleCount, _ECSRoles.Count);
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                GameObject obj = GameObject.Instantiate(role);
                int x, y;
                _map.GetCanPassPoint(out x, out y);
                Role roleScript = obj.AddComponent<Role>();
                roleScript.Init(x, y, _map);
                _roles.Add(obj);
            }
            EventManager.instance.DispatchEvent<int>(EventName.UpdateRoleCount, _roles.Count);
        }
    }

    public void ChangeToECS()
    {
        if (Config.isECSModel || _map == null) return;

        Config.isECSModel = true;
        for (int i = 0; i < _roles.Count; i++)
        {
            GameObject obj = _roles[i];
            Role roleScript = obj.GetComponent<Role>();
            ECSRole eCSRole = obj.AddComponent<ECSRole>();
            eCSRole.Init(roleScript.posx, roleScript.posy, roleScript.index, roleScript.path);
            _ECSRoles.Add(eCSRole);
        }
        _roles.Clear();
        EventManager.instance.DispatchEvent<int>(EventName.UpdateRoleCount, _ECSRoles.Count);
    }

    public void RandomCreaterPlane()
    {
        int count = Random.Range(10, 100) * 2 + 1;
        Debug.LogError(count);
        InitPlane(count);
    }

    public void InitPlane(int size)
    {
        ClearRole();
        cameraTransform.position = new Vector3(size / 2f, 250, size / 2f);
        for (int i = 0; i < _obstacles.Count; i++)
        {
            Destroy(_obstacles[i]);
        }
        _obstacles.Clear();

        plane.transform.localScale = new Vector3(size, 1, size);
        _map = new Map(size);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (_map.mapData[i, j] == 1)
                {
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
