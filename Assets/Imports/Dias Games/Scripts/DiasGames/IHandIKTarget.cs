using UnityEngine;

namespace DiasGames
{
    public interface IHandIKTarget
    {
        Transform GetLeftHandTarget();
        Transform GetRightHandTarget();
    }
}