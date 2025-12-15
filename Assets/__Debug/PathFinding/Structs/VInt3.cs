using System;

namespace PathFinding.Structs
{
    // Custom VInt3 struct
    public struct VInt3
    {
        public readonly int x;
        public readonly int y;
        public readonly int z;

        public VInt3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static VInt3 operator +(VInt3 a, VInt3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
        public static VInt3 operator -(VInt3 a, VInt3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z);

        public static readonly VInt3 Zero = new(0, 0, 0);


        public static readonly VInt3 Forward = new(0, 0, 1);
        public static readonly VInt3 Back = new(0, 0, -1);
        public static readonly VInt3 Right = new(1, 0, 0);
        public static readonly VInt3 Left = new(-1, 0, 0);
        public static readonly VInt3 Up = new(0, 1, 0);
        public static readonly VInt3 Down = new(0, -1, 0);

        // Diagonals
        public static readonly VInt3 ForwardRight = new(1, 0, 1);
        public static readonly VInt3 ForwardLeft = new(-1, 0, 1);
        public static readonly VInt3 BackRight = new(1, 0, -1);
        public static readonly VInt3 BackLeft = new(-1, 0, -1);

        // Utility
        public static float Distance(VInt3 a, VInt3 b)
        {
            float num1 = a.x - b.x;
            float num2 = a.y - b.y;
            float num3 = a.z - b.z;
            
            return (float)Math.Sqrt(num1 * num1 +
                                    num2 * num2 +
                                    num3 * num3);
        }
    }
}