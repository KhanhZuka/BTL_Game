using System.Collections.Generic;
using UnityEngine;

public class FirePool : MonoBehaviour
{
    public static FirePool Instance;

    [Header("Pool Settings")]
    public GameObject firePrefab;
    public int poolSize = 10;

    private Queue<GameObject> firePool = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject fire = Instantiate(firePrefab);
            fire.SetActive(false);

            Fire fireScript = fire.GetComponent<Fire>();
            fireScript.SetPool(this);

            firePool.Enqueue(fire);
        }
    }

    public GameObject GetFire(Vector2 position, Quaternion rotation)
    {
        GameObject fire;

        if (firePool.Count > 0)
        {
            fire = firePool.Dequeue();
        }
        else
        {
            fire = Instantiate(firePrefab);

            Fire fireScript = fire.GetComponent<Fire>();
            fireScript.SetPool(this);
        }

        fire.transform.position = position;
        fire.transform.rotation = rotation;
        fire.SetActive(true);

        return fire;
    }

    public void ReturnFire(GameObject fire)
    {
        fire.SetActive(false);
        firePool.Enqueue(fire);
    }
}