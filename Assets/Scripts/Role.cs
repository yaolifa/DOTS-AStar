using System.Collections.Generic;
using UnityEngine;
public class Role: MonoBehaviour
{
    public int posx;
    public int posy;
    public List<int[]> path;
    public int index;
    private Map _map;
    private float speed = 10;
    private Vector3 tempVec3 = Vector3.zero;
    public void Init(int posx, int posy, Map map){
        this.posx = posx;
        this.posy = posy;
        _map = map;
        tempVec3.x = posx;
        tempVec3.z = posy;
        gameObject.transform.position = tempVec3;
        GetPath();
    }

    public void GetPath(){
        index = 1;
        path = _map.GetPath(posx, posy);
    }

    public void Update(){
        if(index >= path.Count){
            GetPath();
            return;
        }

        int[] pos = path[index];
        int dirX = pos[0] - posx;
        int dirY = pos[1] - posy;
        tempVec3.x = transform.position.x + Time.deltaTime * dirX * speed;
        tempVec3.z = transform.position.z + Time.deltaTime * dirY * speed;
        if(
            (dirX < 0 && tempVec3.x <= pos[0]) ||
            (dirX > 0 && tempVec3.x >= pos[0]) ||
            (dirY > 0 && tempVec3.z >= pos[1]) ||
            (dirY < 0 && tempVec3.z <= pos[1]) ){
            tempVec3.x = pos[0];
            tempVec3.z = pos[1];
            posx = pos[0];
            posy = pos[1];
            transform.position = tempVec3;
            index++;
        }else{
            transform.position = tempVec3;
        }
    }
}