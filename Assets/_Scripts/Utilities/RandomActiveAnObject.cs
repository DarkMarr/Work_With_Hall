using System;
using NaughtyAttributes;
using UnityEngine;

namespace QuizGame.Utilities
{
    public class RandomActiveAnObject : MonoBehaviour
    {
        [SerializeField] 
        private GameObject[] objectsToActivate;

        private void Start()
        {
            ActivateRandomObject();
        }

        [Button]
        private void ActivateRandomObject()
        {
            if (objectsToActivate == null || objectsToActivate.Length == 0)
            {
                Debug.LogWarning("No objects assigned to activate.");
                return;
            }

            var randomIndex = UnityEngine.Random.Range(0, objectsToActivate.Length);

            for (int i = 0; i < objectsToActivate.Length; i++)
            {
                if (objectsToActivate[i] != null)
                {
                    objectsToActivate[i].SetActive(i == randomIndex);
                }
                else
                {
                    Debug.LogWarning($"The object at index {i} is null.");
                }
            }
        }

        [Button]
        private void ActiveAllObjects()
        {
            if (objectsToActivate == null || objectsToActivate.Length == 0)
            {
                Debug.LogWarning("No objects assigned to activate.");
                return;
            }

            foreach (var obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("One of the objects in the array is null.");
                }
            }
        }
    }
}
