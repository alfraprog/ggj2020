using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public float moveSpeed = 30.0f;

    public Transform target;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Make sure not to change the z
        if (target != null)
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.position.x, target.position.y, transform.position.z), Time.deltaTime * moveSpeed);


    }

    public void ChangeTarget(Transform transform)
    {
        target = transform;
    }
}
