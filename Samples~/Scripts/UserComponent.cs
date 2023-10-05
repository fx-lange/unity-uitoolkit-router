using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class UserComponent : PlaceholderComponent
    {
        [SerializeField] private Router _router;
        public override void Show(Params @params)
        {
            base.Show(@params);
            View.Q<Button>("user-profile").clickable.clicked += async () => await _router.Push(DynamicRouting.UserProfile);
            View.Q<Button>("user-posts").clickable.clicked += async () => await _router.Push(DynamicRouting.UserPosts);
        }
    }
}