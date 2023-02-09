using UnityEngine;

namespace Utils
{
    public static class AngleTranslator
    {
        public static float GetAngle(float angle)
        {
            return Mathf.Min(Mathf.Abs(angle), Mathf.Abs(360 - angle));
        }
    }
}