using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Encore.Saves
{
    [AddComponentMenu("Encore/Saves/Player Saver")]
    public class PlayerSaver : CharacterSaver
    {
        protected override bool CheckInstantiation(GameManager.GameAssets gameAssets)
        {
            // Note: actually this is always have to be false, because the game cannot run without a player
            //return SceneManager.GetActiveScene().name != gameAssets.systemData.currentScene;
            return false;
        }
    }
}