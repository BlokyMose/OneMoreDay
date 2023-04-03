using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace Encore.VisualScripting
{
    [UnitTitle("On Interscript Event")]//Custom Event node to receive the event. Adding On to the node title as an event naming convention.
    [UnitCategory("Events\\_MyEvents")]//Setting the path to find the node in the fuzzy finder in Events > My Events.
    public class InterscriptEvent : EventUnit<InterscriptEventParameters>
    {
        [DoNotSerialize]// No need to serialize ports.
        public ValueOutput parameters { get; private set; }// The event output data to return when the event is triggered.
        protected override bool register => true;

        // Adding an EventHook with the name of the event to the list of visual scripting events.
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.InterscriptEvent);
        }
        protected override void Definition()
        {
            base.Definition();
            // Setting the value on our port.
            parameters = ValueOutput<InterscriptEventParameters>(nameof(parameters));
        }
        // Setting the value on our port.
        protected override void AssignArguments(Flow flow, InterscriptEventParameters data)
        {
            flow.SetValue(parameters, data);
        }
    }
}