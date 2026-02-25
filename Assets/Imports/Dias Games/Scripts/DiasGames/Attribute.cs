using System;
using UnityEngine;

namespace DiasGames
{
    public enum AttributeType
    {
        Health
    }

    [Serializable]
    public class Attribute
    {
        private const int MinValue = 0; 
        
        [SerializeField] private AttributeType _attributeType;
        [SerializeField] private float _maxValue;
        [SerializeField] private float _initialValue;

        private float _currentValue;

        public float CurrentValue => _currentValue;
        public float InitialValue => _initialValue;
        public float MaxValue => _maxValue;
        public AttributeType AttributeType => _attributeType;
        
        public event Action<float> OnAttributeChanged;
        public event Action OnAttributeEmpty;

        public Attribute(AttributeType type, float maxValue, float initialValue)
        {
            _initialValue = initialValue;
            _maxValue = maxValue;
            _attributeType = type;

            _currentValue = _initialValue;
        }

        public void SetValue(float newValue)
        {
            if (Mathf.Approximately(newValue - _currentValue, 0.0f))
            {
                return;
            }

            _currentValue = Mathf.Clamp(newValue, MinValue, _maxValue);
            OnAttributeChanged?.Invoke(_currentValue);

            if (_currentValue <= MinValue)
            {
                OnAttributeEmpty?.Invoke();
            }
        }

        public void AddValue(float amount)
        {
            SetValue(_currentValue + amount);
        }
    }
}