using UnityEngine;

namespace _Game.Structs
{
    public struct SHitResult
    {
        public GameObject Attacker;
        public Vector3 HitPoint;
        public int Damage;
        public float HitForce;
    }
}