using Encore.Utility;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.SceneMasters
{
    [CreateAssetMenu(menuName = "SO/GlobalState/Global State Object ID", fileName = "GSO_Name_Init")]
    [InlineEditor]

    public class GlobalStateObject : ScriptableObject
    {
        public enum ActivationMode { AsIs, Active, Inactive, Destroyed }

        [System.Serializable]
        public class Activation
        {
            [System.Serializable]
            public class StageActivation
            {
                public GlobalState.Stage stage;
                public ActivationMode activationMode;

                public StageActivation(GlobalState.Stage stage, ActivationMode activationMode)
                {
                    this.stage = stage;
                    this.activationMode = activationMode;
                }
            }

            [HideInInspector]
            public GlobalStateObject gso;
            
            [HorizontalGroup("top"), ReadOnly, HideLabel]
            public GlobalState gs;

            [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
            [PropertySpace(SpaceAfter = 10)]
            public List<StageActivation> stageActivations = new List<StageActivation>();

            public Activation(GlobalStateObject gso, GlobalState gs)
            {
                this.gso = gso;
                this.gs = gs;
                this.stageActivations = new List<StageActivation>();
                foreach (var stage in gs.stages)
                {
                    stageActivations.Add(new StageActivation(stage, ActivationMode.AsIs));
                }
            }


            #region [Methods: Inspector]

            public void UpdateStages()
            {
                // Remove old stageActivations which do not exist in the GS
                for (int i = stageActivations.Count - 1; i >= 0; i--)
                {
                    var toDelete = true;
                    foreach (var stage in gs.stages)
                    {
                        if (stageActivations[i].stage.stageName == stage.stageName)
                        {
                            toDelete = false;
                            break;
                        }
                    }

                    if (toDelete) 
                        stageActivations.RemoveAt(i);
                }

                // Add new stageActivations which are just added to the GS
                foreach (var stage in gs.stages)
                {
                    var foundStage = stageActivations.Find(s => s.stage.stageName == stage.stageName);
                    if (foundStage == null)
                        stageActivations.Add(new StageActivation(stage, ActivationMode.AsIs));
                }
            }

            [HorizontalGroup("top", Width = 0.15f), Button("Del"), GUIColor("@Encore.Utility.ColorUtility.salmon")]
            public void Delete()
            {
                gso.activations.Remove(this);
                gs.objects.Remove(gso);
            }

            #endregion

        }

        #region [Methods: Inspector]

        [ListDrawerSettings(HideAddButton = true, DraggableItems = false, HideRemoveButton = true)]
        public List<Activation> activations = new List<Activation>();

        [Button("Add"), GUIColor("@Color.green")]
        public void AddActivation(GlobalState gs)
        {
            var foundActivation = activations.Find(act=>act.gs == gs);
            if (foundActivation == null)
            {
                activations.Add(new Activation(this, gs));
                gs.objects.Add(this);
            }
        }

        [HorizontalGroup("but"), Button("Update Stages")]
        public void UpdateStagesOfAllActivations()
        {
            foreach (var activation in activations)
                activation.UpdateStages();
        }

        [HorizontalGroup("but", Width = 0.2f), Button("Del"), GUIColor("@Encore.Utility.ColorUtility.salmon")]
        void Delete()
        {
#if UNITY_EDITOR
            if(UnityEditor.EditorUtility.DisplayDialog("Deleting this GSO", "Are you sure to delete this GSO?\n\nDeleting this will also remove GSO from all GS's objects list.", "Yes"))
            {
                for (int i = activations.Count - 1; i >= 0; i--)
                    activations[i].Delete();
                var thisPath = UnityEditor.AssetDatabase.GetAssetPath(GetInstanceID());
                UnityEditor.AssetDatabase.DeleteAsset(thisPath);
            }
#endif
        }

        public void RemoveActivationsWithGS(GlobalState gs)
        {
            for (int i = activations.Count - 1; i >= 0; i--)
            {
                if (activations[i].gs == gs)
                    activations.RemoveAt(i);
            }
        }

        #endregion
    }
}
