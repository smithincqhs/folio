using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    public List<GameObject> enemies;
    public int maxEnemies;
    public float spawnRate;
    public GameObject easyEnemy, midEnemy, Enemy;
    public GameObject player;
    public float minX1, maxX1, minY1, maxY1, minZ1, maxZ1, minX2, maxX2, minY2, maxY2, minZ2, maxZ2;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), spawnRate, spawnRate);
    }

    private void SpawnEnemy()
    {
        if(enemies.Count < maxEnemies)
        {
            if(Random.Range(0, 1) > 0.5)
            {
                Enemy = Instantiate<GameObject>(easyEnemy);
            }
            else
            {
                Enemy = Instantiate<GameObject>(midEnemy);
            }
            Enemy.GetComponent<AIController>().target = player;
            float _x = 0;
            float _y = 0;
            float _z = 0;
            bool _xSet = false, _ySet = false, _zSet = false;
            _x = Random.Range(minX1, maxX2);
            if((_x > minX1 && _x < maxX1)||(_x > minX2 && _x < maxX2))
            {
                _xSet = true;
            }
            else
            {
                _x = 55;
            }
            _y = Random.Range(minY1, maxY2);
            if ((_y > minY1 && _y < maxY1) || (_y > minY2 && _y < maxY2))
            {
                _ySet = true;
            }
            else
            {
                _y = 0;
            }
            _z = Random.Range(minZ1, maxZ2);
            if ((_z > minZ1 && _z < maxZ1) || (_z > minZ2 && _z < maxZ2))
            {
                _zSet = true;
            }
            else
            {
                _z = -40;
            }
            Enemy.transform.position = new Vector3(_x, _y, _z);
            enemies.Add(Enemy);
        }
    }
}
