using Unity.VisualScripting;
using UnityEngine;

namespace Encore.VisualScripting
{

    //Custom node to send the event
    [UnitTitle("Send Interscript Event")]
    [UnitCategory("Events\\_MyEvents")]//Setting the path to find the node in the fuzzy finder in Events > My Events.
    public class SendInterscriptEvent : Unit
    {
        [DoNotSerialize]// Mandatory attribute, to make sure we don’t serialize data that should never be serialized.
        [PortLabelHidden]// Hiding the port label as we normally hide the label for default Input and Output triggers.
        public ControlInput inputTrigger { get; private set; }
        [DoNotSerialize]
        public ValueInput parameters;
        [DoNotSerialize]
        [PortLabelHidden]// Hiding the port label as we normally hide the label for default Input and Output triggers.
        public ControlOutput outputTrigger { get; private set; }

        protected override void Definition()
        {
            inputTrigger = ControlInput(nameof(inputTrigger), Trigger);
            parameters = ValueInput(nameof(parameters), new InterscriptEventParameters());
            outputTrigger = ControlOutput(nameof(outputTrigger));
            Succession(inputTrigger, outputTrigger);
        }

        //Sending the Event MyCustomEvent with the integer value from the ValueInput port myValueA.
        private ControlOutput Trigger(Flow flow)
        {
            EventBus.Trigger(EventNames.InterscriptEvent, flow.GetValue<InterscriptEventParameters>(parameters));
            return outputTrigger;
        }
    }
}