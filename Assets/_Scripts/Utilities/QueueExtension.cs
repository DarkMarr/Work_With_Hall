using System.Collections.Generic;
using UnityEngine;

namespace QuizGame.Utilities
{
    public static class QueueExtension
    {
        public static T DequeueNullOnNon<T>(this Queue<T> queue) where T : class
        {
            return queue.Count > 0 ? queue.Dequeue() : null;
        }
    }
}
