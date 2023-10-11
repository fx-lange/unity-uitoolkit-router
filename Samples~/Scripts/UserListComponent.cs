using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class UserListComponent : BaseComponent, IRoutable
    {
        [SerializeField] private Router _router;

        [Header("UserData")] [SerializeField] private List<string> _users = new()
        {
            "Tyler",
            "Amelie",
            "Marty",
            "Leeloo"
        };

        public override void Setup(VisualElement parent, string viewName)
        {
            base.Setup(parent, viewName);
            View.Add(new VisualElement() { name = "users" });
        }

        public void Activate(Params @params)
        {
            var usersView = View.Q<VisualElement>("users");
            usersView.Clear();
            foreach (var user in _users)
            {
                var userView = new VisualElement();
                userView.Add(new Label() { text = $"{user}" });
                userView.RegisterCallback<ClickEvent>(async _ =>
                {
                    Debug.Log($"{user} clicked");
                    await _router.Push("posts", new Params { { "user", user } });
                });
                usersView.Add(userView);
            }
        }
    }
}