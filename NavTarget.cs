using System;

namespace Plugins.Router
{
    public class NavTarget : IEquatable<NavTarget>
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
        
        public bool Equals(NavTarget other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Equals(Params, other.Params);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NavTarget)obj);
        }


        public static bool operator ==(NavTarget lhs, NavTarget rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(NavTarget lhs, NavTarget rhs) => !(lhs == rhs);

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Params);
        }
    }
}