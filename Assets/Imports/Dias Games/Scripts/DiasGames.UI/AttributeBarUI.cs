using UnityEngine;
using UnityEngine.UI;
using DiasGames.Components;

namespace DiasGames.UI {

    public class AttributeBarUI : MonoBehaviour
    {
        [SerializeField] private Image _barImage;
        [SerializeField] private AttributesComponent _attributesComponent;
        [SerializeField] private AttributeType _attributeType;

        private Attribute _attribute;

        private void Start()
        {
            _attribute = _attributesComponent.GetAttributeByType(_attributeType);
            _attribute.OnAttributeChanged += UpdateBar;
            UpdateBar(_attribute.CurrentValue);

        }

        private void OnDisable()
        {
            if (_attribute != null)
            {
                _attribute.OnAttributeChanged -= UpdateBar;
            }
        }

        private void UpdateBar(float newValue)
        {
            _barImage.fillAmount = newValue / _attribute.MaxValue;
        }
    }
}