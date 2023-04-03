using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Saves;

[RequireComponent(typeof(SaveHook))]
public class TestTable : MonoBehaviour
{
    SaveHook hook;

    public Vector3 test;

    [Button("Test")]
    private void Test()
    {
        test = (Vector3)hook.LoadKey();
    }

    private void Awake()
    {
        hook = GetComponent<SaveHook>();
    }

    // Start is called before the first frame update
    void Start()
    {
        hook.SaveKey(transform.position);
    }
}
