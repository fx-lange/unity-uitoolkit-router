using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class AccountComponent : BaseComponent
    {
        [SerializeField] private Router _router;

        public override void Initialize(VisualElement parent)
        {
            base.Initialize(parent);
            var element = new VisualElement();
            element.RegisterCallback<ClickEvent>(async _ =>
            {
                await _router.Push("posts", new Params { { "user", "me" } });
            });
            element.Add(new Label("My posts"));
            View.Add(element);
        }
    }
}