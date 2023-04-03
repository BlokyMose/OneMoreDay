using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Interactables;
using Encore.Phone.ToDo;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/ToDo Setter")]
    public class ToDoSetter : Interactable
    {
        [Title(nameof(ToDoSetter))]

        [SerializeField]
        ToDoData toDoData;
        public ToDoData ToDoData { get { return toDoData; } }

        public enum ToDoSetterMode { Add, Edit }
        [SerializeField]
        ToDoSetterMode mode = ToDoSetterMode.Add;

        [SerializeField, ShowIf("@" + nameof(mode) + "==ToDoSetterMode.Edit")]
        bool isEditStatus = false;
        [SerializeField, ShowIf("@" + nameof(mode) + "==ToDoSetterMode.Edit" + "&&" + nameof(isEditStatus))]
        ToDoData.ToDoStatus editStatus = ToDoData.ToDoStatus.Done;

        [SerializeField, ShowIf("@" + nameof(mode) + "==ToDoSetterMode.Edit")]
        bool isEditText = false;
        [SerializeField, ShowIf("@" + nameof(mode) + "==ToDoSetterMode.Edit" + "&&" + nameof(isEditText))]
        string editText = "";

        [SerializeField, ShowIf("@" + nameof(mode) + "==ToDoSetterMode.Edit")]
        bool isEditTag = false;
        [SerializeField, ShowIf("@" + nameof(mode) + "==ToDoSetterMode.Edit" + "&&" + nameof(isEditTag))]
        ToDoTag editTag;

        [SerializeField, ShowIf("@" + nameof(mode) + "==ToDoSetterMode.Edit")]
        bool isEditLocation = false;
        [SerializeField, ShowIf("@" + nameof(mode) + "==ToDoSetterMode.Edit" + "&&" + nameof(isEditLocation))]
        Locations.Location editLocation;

        [SerializeField, ShowIf("@" + nameof(mode) + "==ToDoSetterMode.Edit")]
        bool isEditIsKnownToPlayer = false;
        [SerializeField, ShowIf("@" + nameof(mode) + "==ToDoSetterMode.Edit" + "&&" + nameof(isEditIsKnownToPlayer))]
        bool editIsKnownToPlayer = true;

        protected override void InteractModule(GameObject interactor)
        {
            switch (mode)
            {
                case ToDoSetterMode.Add:
                    GameManager.Instance.PhoneManager.ToDoApp.AddToDo(toDoData);
                    break;
                case ToDoSetterMode.Edit:
                    editText = string.IsNullOrEmpty(editText) ? toDoData.Text : editText;

                    var edittedData = new Serializables.ToDoData(
                        id: toDoData.ID,
                        status: isEditStatus ? (int)editStatus : (int)toDoData.Status,
                        text: isEditText ? editText : toDoData.Text,
                        tag: isEditTag ? editTag.TagName : toDoData.Tag.TagName,
                        location: isEditLocation ? editLocation.SceneName : toDoData.Location.SceneName
                        );

                    GameManager.Instance.PhoneManager.ToDoApp.EditToDo(
                        edittedData, 
                        isEditIsKnownToPlayer 
                        ? editIsKnownToPlayer 
                            ? Utility.BoolStatus.True 
                            : Utility.BoolStatus.False 
                        : Utility.BoolStatus.AsIs);

                    break;
            }
        }

        public void ForceInteract()
        {
            InteractModule(null);
        }

    }

}