using System.Collections.Generic;
using System;

namespace Encore.Dialogues
{
    [Serializable]
    public class MonologuesList
    {
        public string title;
        public List<Monologue> monologues;

        public MonologuesList(string title, List<Monologue> monologues)
        {
            this.title = title;
            this.monologues = monologues;
        }

        public List<string> GetActorNames()
        {
            List<string> actors = new List<string>();
            foreach (var monologue in monologues)
            {
                if (!actors.Contains(monologue.Actor.ActorName)) actors.Add(monologue.Actor.ActorName);
            }
            return actors;
        }
    }
}