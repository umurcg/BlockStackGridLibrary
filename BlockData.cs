using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BlockStackGridLibrary
{
    [Serializable]public struct BlockData
    {
        public string typeName;
        [ValidateInput(nameof(ValidateBrick)),AssetsOnly] public GameObject prefab;
        
        private bool ValidateBrick(GameObject obj)
        {
            if(obj==null) return true;
            return obj.TryGetComponent(out Block brick);
        }

    }
}