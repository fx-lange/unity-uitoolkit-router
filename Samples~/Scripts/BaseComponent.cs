using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class BaseComponent : MonoBehaviour, IRoutable
    {
        [SerializeField] private string _uxmlName;
        
        public virtual void Initialize(VisualElement parent)
        {
            View = parent.Q<VisualElement>(_uxmlName);
            View.Q<Label>().text = _uxmlName;
        }

        public VisualElement View { get; private set; }
    }
}