using Encore.SceneMasters;
using Encore.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Serializables
{
    [System.Serializable]
    public class GlobalStateData
    {
        public string stateName;
        public string currentStage;

        public GlobalStateData(string stateName, string currentStage = "init")
        {
            this.stateName = stateName;
            this.currentStage = currentStage;
        }

        public GlobalStateData(GlobalState gs, string currentStage = "init")
        {
            this.stateName = gs.stateName;
            this.currentStage = currentStage;
        }

        public GlobalStateData(GlobalStateSetter.GlobalStateModified stateModified)
        {
            this.stateName = stateModified.gs.stateName;
            ModifyData(stateModified);
        }

        public void ModifyData(GlobalStateSetter.GlobalStateModified stateModified)
        {
            this.currentStage = stateModified.targetStage;
        }
    }
}