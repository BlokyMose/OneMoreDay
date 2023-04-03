using System.Collections;
using UnityEngine;

namespace Encore
{
    public interface IPersistentSystem
    {
        void OnBeforeSceneLoad();
        void OnAfterSceneLoad();
    }
}