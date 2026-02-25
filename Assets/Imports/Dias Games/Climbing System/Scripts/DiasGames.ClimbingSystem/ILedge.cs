using System.Collections.Generic;
using UnityEngine;

namespace DiasGames.ClimbingSystem
{
    public interface ILedge
    {
        public bool CanClimbUp { get; }
        public bool CanJumpSide { get; }
        List<Transform> GrabPoints { get; }
        public Transform GetClosestPoint(Vector3 origin, Vector3 normal);
        public Transform GetClosestPoint(Vector3 origin);
    }
}