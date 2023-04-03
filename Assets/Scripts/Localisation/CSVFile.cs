using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Localisations
{
    [System.Serializable]
    public class CSVFile
    {
        public TextAsset textAsset;

        public CSVFile(TextAsset csvTextAsset)
        {
            this.textAsset = csvTextAsset;
        }

        public static implicit operator CSVFile(TextAsset csvTextAsset)
        {
            return new CSVFile(csvTextAsset);
        }
    }
}
