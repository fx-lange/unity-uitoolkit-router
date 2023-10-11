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
            if (@params.ContainsKey("user"))
            {
                View.Q<Label>("message").text = $"Posts by {@params["user"]}";
            }
        }
    }
}