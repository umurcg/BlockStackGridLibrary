using System;
using ObjectType;

namespace BlockStackGridLibrary
{
    [Serializable]public struct BlockSubStack
    {
        public int numberOfStack;
        public ObjectTypeEnum type;
        public string TypeName => type.typeName;
        
        public void SetType(string typeName)
        {
            this.type = ObjectTypeEnum.GetEnum(typeName);
        }
        
        
        public BlockSubStack(string type,int numberOfStack)
        {
            this.numberOfStack = numberOfStack;
            this.type = ObjectTypeEnum.GetEnum(type);
        }

    }
}