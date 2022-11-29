namespace Plugins.Router
{
    public class NavTarget
    {
        public string Name;
        public Params Params;

        public static implicit operator NavTarget(string name)
        {
            return new NavTarget()
            {
                Name = name
            };
        }
    }

    public class NavState : NavTarget
    {
        public Route Route;
    }
}