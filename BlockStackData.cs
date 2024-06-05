using System;

namespace BlockStackGridLibrary
{
    [Serializable]public struct BlockStackData
    {
        public BlockSubStack[] subStacks;
        
        //To string
        public override string ToString()
        {
            string result = "";
            foreach (var subStack in subStacks)
            {
                result += subStack.type + " " + subStack.numberOfStack + " ";
            }

            return result;
        }

        public bool Compare(BlockStackData blockStackData)
        {
            if (subStacks.Length != blockStackData.subStacks.Length) return false;
            for (int i = 0; i < subStacks.Length; i++)
            {
                if (subStacks[i].type != blockStackData.subStacks[i].type) return false;
                if (subStacks[i].numberOfStack != blockStackData.subStacks[i].numberOfStack) return false;
            }

            return true;
        }
    }
}