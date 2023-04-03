using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Saves;

[RequireComponent(typeof(SaveHook))]
public class Mop : MonoBehaviour
{
    SaveHook hook;
    bool mopDown = false;

    [Button(Name = "Toggle Mop")]
    private void ToggleMop()
    {
        mopDown = !mopDown;
        Debug.Log("Mop Down is set to " + mopDown.ToString());
        hook.SaveKey(mopDown);
    }
    
    private void Awake()
    {
        hook = GetComponent<SaveHook>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (hook.LoadKey() == null)
        {
            mopDown = false;
            hook.SaveKey(mopDown);
        }
        else
        {
            mopDown = (bool)hook.LoadKey();
        }

        if (mopDown)
        {
            Debug.Log("Mop down");
        }
        else
        {
            Debug.Log("Mop still up");
        }
    }
}
