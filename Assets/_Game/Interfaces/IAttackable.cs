using _Game.Structs;
using UnityEngine;

namespace _Game.Interfaces
{
    public interface IAttackable
    {
        public SAttackResult OnAttack(SHitResult hitResult);
    }
}