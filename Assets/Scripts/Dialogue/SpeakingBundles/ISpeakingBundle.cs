using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Dialogues
{
    public interface ISpeakingBundle
    {
        public TextAsset CSV { get; }
        public List<string> ActorNames { get; }
    }
}