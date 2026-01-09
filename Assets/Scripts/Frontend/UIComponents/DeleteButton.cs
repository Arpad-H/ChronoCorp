using System;
using UnityEngine;
using UnityEngine.UI;

namespace Frontend.UIComponents
{
    public class DeleteButton : MonoBehaviour
    {
        [SerializeField] private Button button;
    
        public void Init(Action onDelete)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onDelete());
        }
    }
}