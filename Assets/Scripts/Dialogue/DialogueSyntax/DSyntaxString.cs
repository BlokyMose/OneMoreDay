using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Dialogues
{
    [System.Serializable]
    public class DSyntaxString
    {
        public TextAsset textAsset;
        public string dSyntax;

        public DSyntaxString(TextAsset textAsset, string dSyntax)
        {
            this.textAsset = textAsset;
            this.dSyntax = dSyntax;
        }

        //public static implicit operator DSyntaxString(TextAsset textAsset, string dSyntax)
        //{
        //    return new DSyntaxString(textAsset, dSyntax);
        //}
    }
}
