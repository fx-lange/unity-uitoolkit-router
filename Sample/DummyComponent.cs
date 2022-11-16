using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugins.Router.Sample
{
    public class DummyComponent : MonoBehaviour, IRouteComponent
    {
        public VisualTreeAsset asset;
        private VisualElement _view;

        public VisualElement View => _view;

        public void OnEnable()
        {
            _view = asset.Instantiate();
        }

        public void Show(Params @params)
        {
        }

        public void Hide()
        {
        }
    }
}
