namespace PathFinding.MethodExtensions
{
    public static class Vector3Extensions
    {
        public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 v)
        {
            return new UnityEngine.Vector3(v.X, v.Y, v.Z);
        }
        
        public static System.Numerics.Vector3 ToUnity(this UnityEngine.Vector3 v)
        {
            return new System.Numerics.Vector3(v.x, v.y, v.z);
        }
    }
}