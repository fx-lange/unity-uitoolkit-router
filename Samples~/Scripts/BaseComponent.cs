using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class BaseComponent : MonoBehaviour, IRoutable
    {
        public virtual void Setup(VisualElement parent, string viewName)
        {
            View = parent.Q<VisualElement>(viewName);
            View.Q<Label>().text = viewName;
            Hide();
        }

        public VisualElement View { get; private set; }

        public virtual void Show(Params @params)
        {
            View.style.display = DisplayStyle.Flex;
        }

        public virtual void Hide()
        {
            View.style.display = DisplayStyle.None;
        }
    }
}