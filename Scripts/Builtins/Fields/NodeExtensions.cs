using System.Collections.Generic;
using OneHamsa.Dexterity.Utilities;
using UnityEngine;

namespace OneHamsa.Dexterity.Builtins
{
    public static class NodeExtensions
    {
        public static NodeRaycastRouter GetRaycastRouter(this BaseStateNode node)
        {
            return node.GetOrAddComponent<NodeRaycastRouter>();
        }
        
        public static void AddUpstream(this FieldNode.OutputField field, FieldNode.OutputField other, 
            NodeReference.Gate.OverrideType overrideType = NodeReference.Gate.OverrideType.Additive,
            bool negate = false)
        {
            field.node.enabled = false;
            field.node.AddGate(new NodeReference.Gate
            {
                outputFieldName = field.definition.name,
                overrideType = overrideType,
                field = new NodeField
                {
                    targetNodes = new List<FieldNode> { other.node },
                    fieldName = other.definition.name,
                    negate = negate
                }
            });
            field.node.enabled = true;
        }
        public static string GetEnumValue(this BaseField field)
        {
            if (field.definition.type != FieldNode.FieldType.Enum)
            {
                Debug.LogError($"GetEnumValue: {field.definition.name} is not of type enum");
                return null;
            }
            var value = field.GetValue();
            if (value == FieldNode.emptyFieldValue)
                value = 0;

            return field.definition.enumValues[value];
        }

        public static string GetValueAsString(this BaseField field) {
            if (field.GetValue() == FieldNode.emptyFieldValue)
                return "(empty)";

            switch (field.definition.type) {
                case FieldNode.FieldType.Boolean:
                    return field.GetBooleanValue().ToString();
                case FieldNode.FieldType.Enum:
                    return field.GetEnumValue().ToString();
                default:
                    return field.GetValue().ToString();
            }
        }
    }
}
