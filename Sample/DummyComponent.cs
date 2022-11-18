using UnityEngine;
using UnityEngine.UIElements;

namespace Plugins.Router.Sample
{
    public class DummyComponent : MonoBehaviour, IRouteComponent
    {
        public VisualTreeAsset asset;
        public string name;
        private VisualElement _view;

        public VisualElement View => _view;

        public void OnEnable()
        {
            _view = asset.Instantiate();
            _view.Q<Label>().text = name;
        }

        public void Show(Params @params)
        {
        }

        public void Hide()
        {
        }
    }
}
