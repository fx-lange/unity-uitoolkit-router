using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class DynamicRouting : MonoBehaviour
    {
        [SerializeField] private Router _router;

        [SerializeField] private PlaceholderComponent _about;
        [SerializeField] private PlaceholderComponent _settings;
        [SerializeField] private PlaceholderComponent _login;
        [Space] 
        [SerializeField] private PlaceholderComponent _user;
        [SerializeField] private PlaceholderComponent _userProfile;
        [SerializeField] private PlaceholderComponent _userPosts;

        public static string UserProfile = "user/profile";
        public static string About = "about";
        public static string Settings = "settings";
        public static string Login = "login";
        public static string Admin = "admin";
        public static string User = "user";
        public static string UserPosts = "user/posts";
            
        public void Init(VisualElement root)
        {
            _router.Setup(
                view: root,
                routes: new List<Route>
                {
                    new() { Name = About, Component = _about },
                    new() { Name = Settings, Component = _settings },
                    new() { Name = Login, Component = _login },
                    new() { Name = Admin, Component = null },
                    new() { Name = User, Component = _user, Children =
                        new List<Route>
                        {
                            new(){
                                Name = UserProfile, Component = _userProfile
                            },
                            new ()
                            {
                                Name = UserPosts, Component = _userPosts
                            }
                        }}
                }
            );
        }
    }
}