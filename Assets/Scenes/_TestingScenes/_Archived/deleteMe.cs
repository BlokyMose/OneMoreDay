using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class deleteMe : MonoBehaviour
{
    TMP_InputField inputField;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        UnityAction<string, int, int> unityAction = Process;
        inputField.onTextSelection.AddListener(unityAction);
    }

    int selectEnd, selectStart;
    void Process(string text, int start, int end)
    {
        selectStart = start;
        selectEnd = end;
    }

    [Button]
    public void Bold()
    {
        int before = inputField.text.Length;
        char[] chars = inputField.text.ToCharArray();
        string newText = GetString(chars, 0, selectStart) + "<b>" + 
            GetString(chars, selectStart, selectEnd) + "</b>" + 
            GetString(chars, selectEnd, chars.Length);

        inputField.SetTextWithoutNotify(newText);
        int after = inputField.text.Length;
    }

    public string GetString(char[]chars, int start, int end)
    {

        Debug.Log("start: "+start+"  end:"+end);
        string value = "";
        for (int i = start; i < end; i++)
        {
            value += chars[i];
        }

        return value;
    }
}
