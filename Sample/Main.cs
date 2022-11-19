using System;
using System.Collections;
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
        [SerializeField] private DummyComponent _login;
        
        private Router _router;

        private void OnEnable()
        {
            var view = _uiDocument.rootVisualElement;
            _router = new Router(
                view: view,
                routes: new List<Route>
                {
                    new() { Name = "about", Component = _about },
                    new() { Name = "settings", Component = _settings },
                    new() { Name = "login", Component = _login },
                    new() { Name = "admin", Component = null }
                }
            );

            _router.BeforeEach((from, to) =>
            {
                if (to.Name == "admin")
                {
                    return new Target()
                    {
                        Name = "login"
                    };
                }

                return to;
            });


            view.Q<Button>("about").clickable.clicked += () => _router.Push("about");
            view.Q<Button>("settings").clickable.clicked += () => _router.Push("settings");
            view.Q<Button>("admin").clickable.clicked += () => _router.Push("admin");
            view.Q<Button>("login").clickable.clicked += () => _router.Push("login");
            view.Q<Button>("back").clickable.clicked += () => _router.Back();
        }
    }
}