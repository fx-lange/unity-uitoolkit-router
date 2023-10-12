using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class PostsComponent : BaseComponent, IRoutable
    {
        public override void Setup(VisualElement parent, string viewName)
        {
            base.Setup(parent, viewName);
            View.Add(new Label{name = "message"});
        }

        public void Activate(Params @params)
        {
            if (@params.TryGetValue("user", out var userObj))
            {
                var user = userObj as string;
                View.Q<Label>("message").text = $"Posts by {user}";
            }
        }
    }
}