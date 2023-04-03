using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Conditions
{
    public interface ICheckBool
    {
        public CheckBool GetBoolContainer();
        public void SetBoolContainer(CheckBool boolContainer);
    }
}

