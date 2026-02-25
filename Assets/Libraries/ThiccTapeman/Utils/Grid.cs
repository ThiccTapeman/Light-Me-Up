using UnityEngine;

namespace ThiccTapeman.Utils
{
    public static class Grid
    {
        public static Vector2 GridSnap(Vector2 position, Vector2 gridSize, bool centered = false)
        {
            float x = position.x;
            float y = position.y;

            float posX = Mathf.FloorToInt(x / gridSize.x);
            float posY = Mathf.FloorToInt(y / gridSize.y);

            if(!centered) return new Vector2(posX, posY);

            posX += gridSize.x / 2;
            posY += gridSize.y / 2;

            return new Vector2(posX, posY);
        }
    }
}
