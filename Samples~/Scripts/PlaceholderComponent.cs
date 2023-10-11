using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class PlaceholderComponent : MonoBehaviour, IRoutable
    {
        public VisualTreeAsset asset;
        [FormerlySerializedAs("name")] 
        public string viewName;
        private VisualElement _view;

        public VisualElement View => _view;

        protected virtual void OnEnable()
        {
            _view = asset.Instantiate();
            _view.Q<Label>().text = viewName;
        }

        public virtual void Activate(Params @params)
        {
        }

        public virtual void Deactivate()
        {
        }
    }
}
