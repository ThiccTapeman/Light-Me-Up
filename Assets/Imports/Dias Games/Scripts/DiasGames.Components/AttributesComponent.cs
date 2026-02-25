using System.Collections.Generic;
using UnityEngine;

namespace DiasGames.Components
{
    public class AttributesComponent : MonoBehaviour
    {
        [SerializeField] private List<Attribute> _attributes = new List<Attribute>();

        private readonly Dictionary<AttributeType, Attribute> _attributesMap =
            new Dictionary<AttributeType, Attribute>(3);

        private void Awake()
        {
            foreach (Attribute attribute in _attributes)
            {
                if (_attributesMap.ContainsKey(attribute.AttributeType))
                {
                    Debug.LogError($"Found more than one attribute of the same type: {attribute.AttributeType}. Only the first type of attribute will be considered.");
                    continue;
                }
                
                attribute.SetValue(attribute.InitialValue);
                _attributesMap.Add(attribute.AttributeType, attribute);
            }
        }

        public Attribute GetAttributeByType(AttributeType attributeType)
        {
            return _attributesMap[attributeType];
        }

        public void ApplyChange(AttributeType attributeType, float delta, GameObject fromGameObject)
        {
            if (!_attributesMap.ContainsKey(attributeType))
            {
                return;
            }

            Attribute attribute = _attributesMap[attributeType];
            attribute.AddValue(delta);
        }
    }
}