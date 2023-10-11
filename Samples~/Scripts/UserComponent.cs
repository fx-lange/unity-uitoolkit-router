using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class UserComponent : PlaceholderComponent
    {
        [SerializeField] private Router _router;
        private Button _buttonProfile;
        private Button _buttonPosts;

        protected override void OnEnable()
        {
            base.OnEnable();
            _buttonProfile = View.Q<Button>("user-profile");
            _buttonPosts = View.Q<Button>("user-posts");
        }

        public override void Activate(Params @params)
        {
            base.Activate(@params);
            _buttonProfile.clickable.clicked += GoToProfile;
            _buttonPosts.clickable.clicked += GoToPosts;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            _buttonProfile.clickable.clicked -= GoToProfile;
            _buttonPosts.clickable.clicked -= GoToPosts;
        }

        private async void GoToPosts()
        {
            await _router.Push(DynamicNavigation.UserPosts);
        }

        private async void GoToProfile()
        {
            await _router.Push(DynamicNavigation.UserProfile);
        }
    }
}