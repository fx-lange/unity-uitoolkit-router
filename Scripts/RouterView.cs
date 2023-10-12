using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UITK.Router
{
    public class RouterView : VisualElement
    {
        public bool UseTransition { get; set; } = false;
        public string TransitionName { get; set; } = "Fade";


        public new class UxmlFactory : UxmlFactory<RouterView, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlBoolAttributeDescription _useTransition = new()
                { name = "use-transition", defaultValue = false };

            private readonly UxmlStringAttributeDescription _transitionName = new()
                { name = "transition-name", defaultValue = "fade" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((RouterView)ve).TransitionName = _transitionName.GetValueFromBag(bag, cc);
                ((RouterView)ve).UseTransition = _useTransition.GetValueFromBag(bag, cc);
            }
        }
    }
}