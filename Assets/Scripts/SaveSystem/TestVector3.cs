using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Saves;

[RequireComponent(typeof(SaveHook))]
public class TestVector3 : MonoBehaviour
{
    SaveHook hook;
    public Vector3 vector3ToSave;

    public float x;
    public float y;
    public float z;

    [Button("Save")]
    private void Save()
    {
        hook.SaveKey(vector3ToSave);
    }

    [Button("Load")]
    private void Load()
    {
        Debug.Log(hook.LoadKey());
        Vector3 v3 = (Vector3)hook.LoadKey();

        x = v3.x;
        y = v3.y;
        z = v3.z;
    }

    private void Awake()
    {
        hook = GetComponent<SaveHook>();
    }
}
