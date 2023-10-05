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

        public virtual void Show(Params @params)
        {
            _view = asset.Instantiate();
            _view.Q<Label>().text = viewName;
        }

        public void Hide()
        {
        }
    }
}
