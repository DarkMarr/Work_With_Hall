using System;
using UnityEngine;

namespace QuizGame.Utilities
{
    public static class TransformExtension
    {
        public static void DoActionOnChildren(this Transform parent, Action<Transform> action)
        {
            foreach (Transform item in parent)
            {
                action?.Invoke(item);
            }
        }
    }
}
