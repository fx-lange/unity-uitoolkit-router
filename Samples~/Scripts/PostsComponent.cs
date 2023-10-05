using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class PostsComponent : BaseComponent
    {
        public override void Setup(VisualElement parent, string viewName)
        {
            base.Setup(parent, viewName);
            View.Add(new Label{name = "message"});
        }

        public override void Show(Params @params)
        {
            if (@params.ContainsKey("user"))
            {
                View.Q<Label>("message").text = $"Posts by {@params["user"]}";
            }
            base.Show(@params);
        }
    }
}