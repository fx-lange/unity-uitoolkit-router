using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class StaticMain : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private Router _router;
        [Space] 
        [SerializeField] private AccountComponent _account;
        [SerializeField] private UserListComponent _users;
        [SerializeField] private PostsComponent _posts;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;
            
            _account.Initialize(root);
            _users.Initialize(root);
            _posts.Initialize(root);
            
            _router.Setup(
                view: root,
                routes: new List<Route>
                {
                    new() { Name = "account", Component = _account },
                    new() { Name = "users", Component = _users },
                    new() { Name = "posts", Component = _posts },
                }
            );
            
            root.Q<Button>("account").clickable.clicked += async () => await _router.Push("account");
            root.Q<Button>("users").clickable.clicked += async () => await _router.Push("users");

            var backButton = root.Q<Button>("back");
            backButton.clickable.clicked += async () => await _router.Back();
            backButton.SetEnabled(false);
            
            _router.AfterEach += (_, _) => backButton.SetEnabled(_router.HasHistory);
        }
    }
}