using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    public class DynamicMain : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private Router _router;
        [SerializeField] private DynamicRouting _routing;

        private void OnEnable()
        {
            var view = _uiDocument.rootVisualElement;
            _routing.Init(view);
            _router.BeforeEach += (to, from ) =>
            {
                if (to.Name == DynamicRouting.Admin)
                {
                    return DynamicRouting.Login;
                }

                return to;
            };

            var backButton = view.Q<Button>("back");
            backButton.clickable.clicked += async () => await _router.Back();
            view.Q<Button>("about").clickable.clicked += async () => await _router.Push(DynamicRouting.About);
            view.Q<Button>("settings").clickable.clicked += async () => await _router.Push(DynamicRouting.Settings);
            view.Q<Button>("admin").clickable.clicked += async () => await _router.Push(DynamicRouting.Admin);
            view.Q<Button>("login").clickable.clicked += async () => await _router.Push(DynamicRouting.Login);
            view.Q<Button>("user").clickable.clicked += async () => await _router.Push(DynamicRouting.User);
       
            backButton.SetEnabled(false);
            _router.AfterEach += (_, _) => { backButton.SetEnabled(_router.HasHistory); };
        }
    }
}