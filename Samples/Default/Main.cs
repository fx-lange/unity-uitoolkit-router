using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [Space] 
        [SerializeField] private PlaceholderComponent _about;
        [SerializeField] private PlaceholderComponent _settings;
        [SerializeField] private PlaceholderComponent _login;
        [Space] 
        [SerializeField] private PlaceholderComponent _user;
        [SerializeField] private PlaceholderComponent _userProfile;
        [SerializeField] private PlaceholderComponent _userPosts;
        
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
                    new() { Name = "admin", Component = null },
                    new() { Name = "user", Component = _user, Children =
                    new(){
                        new(){
                            Name = "user/profile", Component = _userProfile
                        },
                        new ()
                        {
                            Name = "user/posts", Component = _userPosts
                        }
                    }}
                }
            );

            _router.BeforeEach((to, from) =>
            {
                if (to.Name == "admin")
                {
                    return "login";
                }

                return to;
            });

            view.Q<Button>("about").clickable.clicked += () => _router.Push("about");
            view.Q<Button>("settings").clickable.clicked += () => _router.Push("settings");
            view.Q<Button>("admin").clickable.clicked += () => _router.Push("admin");
            view.Q<Button>("login").clickable.clicked += () => _router.Push("login");
            view.Q<Button>("user").clickable.clicked += () => _router.Push("user");
            //TODO glaobal router access to link those buttons inside a User Component
            view.Q<Button>("user-profile").clickable.clicked += () => _router.Push("user/profile");
            view.Q<Button>("user-posts").clickable.clicked += () => _router.Push("user/posts");
            view.Q<Button>("back").clickable.clicked += () => _router.Back();
        }
    }
}