using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class Main : SingletonBehaviour<Main>
{
    public GameObject role;
    public GameObject plane;
    public GameObject obstacle;
    public Transform obstacleRoot;
    public Camera mainCamera;
    private List<GameObject> _obstacles;
    private List<GameObject> _roles;
    private List<RoleEntitiesOwner> _roleEntities;
    private Map _map;
    private Entity _roleEntity;
    private EntityManager _entityManager;
    public Map map{
        get{
            return _map;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _obstacles = new List<GameObject>();
        _roles = new List<GameObject>();
        _roleEntities = new List<RoleEntitiesOwner>();
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
            for (int i = 0; i < _roleEntities.Count; i++)
            {
                DynamicBuffer<Child> child = _entityManager.GetBuffer<Child>(_roleEntities[i].entity);
                _entityManager.DestroyEntity(child[0].Value);
                _entityManager.DestroyEntity(_roleEntities[i].entity);
            }
            _roleEntities.Clear();
            EventManager.instance.DispatchEvent<int>(EventName.UpdateRoleCount, _roleEntities.Count);
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
                RoleEntitiesOwner roleEntitiesOwner = new RoleEntitiesOwner();
                int x, y;
                _map.GetCanPassPoint(out x, out y);
                roleEntitiesOwner.Init(x, y, 1);
                roleEntitiesOwner.Convert(entity, _entityManager, null);
                _roleEntities.Add(roleEntitiesOwner);
            }
            EventManager.instance.DispatchEvent<int>(EventName.UpdateRoleCount, _roleEntities.Count);
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

        int size = _map.size;
        Entity mapEntity = _entityManager.CreateEntity();
        _entityManager.AddComponent<CompMapFlag>(mapEntity);
        DynamicBuffer<CompMap> compMaps = _entityManager.AddBuffer<CompMap>(mapEntity);


        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                compMaps.Add(new CompMap{
                    data = _map.mapData[i, j]
                });
            }
        }

        for (int i = 0; i < _roles.Count; i++)
        {
            GameObject obj = _roles[i];
            Role roleScript = obj.GetComponent<Role>();
            ECSRole eCSRole = obj.AddComponent<ECSRole>();
            RoleEntitiesOwner roleEntitiesOwner = new RoleEntitiesOwner();
            roleEntitiesOwner.Init(roleScript.posx, roleScript.posy, roleScript.index, roleScript.path);
            eCSRole.roleEntitiesOwner = roleEntitiesOwner;
            _roleEntities.Add(roleEntitiesOwner);
        }
        _roles.Clear();
        EventManager.instance.DispatchEvent<int>(EventName.UpdateRoleCount, _roleEntities.Count);
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
        mainCamera.transform.position = new Vector3(size / 2f, 250, size / 2f);
        mainCamera.orthographicSize = size / 2f;
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

    private void OnApplicationQuit() {
        if(Tools.pathNodes.IsCreated){
            Tools.pathNodes.Dispose();
        }
    }
}
