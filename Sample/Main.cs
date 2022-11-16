using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugins.Router.Sample
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [Space] 
        [SerializeField] private DummyComponent _about;
        [SerializeField] private DummyComponent _settings;
        
        private Router _router;
        
        private void OnEnable()
        {
            var view = _uiDocument.rootVisualElement;
            _router = new Router();
            _router.Setup(view, new List<Route>
            {
                new() {Name = "about", Component = _about},
                new() {Name = "settings", Component = _settings}
            });

            view.Q<Button>("about").clickable.clicked += () => _router.Push("about");
            view.Q<Button>("settings").clickable.clicked += () => _router.Push("settings");
            view.Q<Button>("back").clickable.clicked += () => _router.Back();
        }
    }
}
