using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class DemoScript : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUILayout.Button("按钮"))
        {
            Debug.Log("aaaaaaaaaaaaaaaaa");
            var renderer = GetComponent<MeshRenderer>();
            renderer.material.color = Color.red;
        }

        if (GUILayout.Button("全部组件"))
        {
            Debug.Log(this.gameObject);
            var allComponents = GetComponents<Component>();
            foreach (var component in allComponents)
            {
                Debug.Log(component);
            }
        }
    }

    private void Awake()
    {
        Debug.Log("awake");
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
        Transform[] father = GetComponentsInChildren<Transform>();
        foreach (var child in father)
        {
            Debug.Log(String.Format("{}-{}-{}-{}", child.name, child.position, child.localPosition, transform.TransformPoint(child.localPosition)));

        }
    }

    private void FixedUpdate()
    {
        // Debug.Log(Time.time);
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(Time.time);
    }
}