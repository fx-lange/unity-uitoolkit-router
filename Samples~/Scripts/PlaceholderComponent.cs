using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class PlaceholderComponent : MonoBehaviour, IRoutable
    {
        public VisualTreeAsset asset;
        public string name;
        private VisualElement _view;

        public VisualElement View => _view;

        public void OnEnable()
        {
        }

        public void Show(Params @params)
        {
            _view = asset.Instantiate();
            _view.Q<Label>().text = name;
        }

        public void Hide()
        {
        }
    }
}
