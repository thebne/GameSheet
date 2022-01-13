using System;
using UnityEngine;

namespace OneHamsa.Dexterity.Visual.Builtins
{
    /// <summary>
    /// helper class that saves node override state and allows to easily clear it
    /// </summary>
    public class NodeFieldOverride {
        public Node node;
        public NodeReference.Gate gate;

        public void Clear() {
            node.RemoveGate(gate);
        }
    }

    public static class NodeOverrideExtensions
    {
        /// <summary>
        /// Sets a boolean override value 
        /// </summary>
        /// <param name="fieldId">Field definition ID (from Manager)</param>
        /// <param name="value">Bool value for field</param>
        public static NodeFieldOverride SetOverride(this Node node, int fieldId, bool value)
        {
            var definition = Manager.instance.GetFieldDefinition(fieldId);
            if (definition.type != Node.FieldType.Boolean)
                Debug.LogWarning($"setting a boolean override for a non-boolean field {definition.name}", node);

            return node.SetOverrideRaw(fieldId, value ? 1 : 0);
        }

        /// <summary>
        /// Sets an enum override value
        /// </summary>
        /// <param name="fieldId">Field definition ID (from Manager)</param>
        /// <param name="value">Enum value for field (should appear in field definition)</param>
        public static NodeFieldOverride SetOverride(this Node node, int fieldId, string value)
        {
            var definition = Manager.instance.GetFieldDefinition(fieldId);
            if (definition.type != Node.FieldType.Enum)
                Debug.LogWarning($"setting an enum (string) override for a non-enum field {definition.name}", node);

            int index;
            if ((index = Array.IndexOf(definition.enumValues, value)) == -1)
            {
                Debug.LogError($"trying to set enum {definition.name} value to {value}, " +
                    $"but it is not a valid enum value", node);
                return null;
            }

            return node.SetOverrideRaw(fieldId, index);
        }

        /// <summary>
        /// Sets raw override value (sugar for AddGate() of type constant with OverrideType.Always)
        /// </summary>
        /// <param name="fieldId">Field definition ID (from Manager)</param>
        /// <param name="value">Field value (0 or 1 for booleans, index for enums)</param>
        public static NodeFieldOverride SetOverrideRaw(this Node node, int fieldId, int value)
        {
            var gate = new NodeReference.Gate {
                outputFieldName = Manager.instance.GetFieldDefinition(fieldId).name,
                overrideType = NodeReference.Gate.OverrideType.Always,
                field = new ConstantField() {
                    constant = value,
                },
            };
            
            node.AddGate(gate);

            return new NodeFieldOverride {
                node = node,
                gate = gate,
            };
        }

        public static NodeFieldOverride SetOverride(this Node.OutputField field, bool value) {
            return field.node.SetOverride(field.definitionId, value);
        }
        public static NodeFieldOverride SetOverride(this Node.OutputField field, string value) {
            return field.node.SetOverride(field.definitionId, value);
        }
        public static NodeFieldOverride SetOverrideRaw(this Node.OutputField field, int value) {
            return field.node.SetOverrideRaw(field.definitionId, value);
        }
    }
}