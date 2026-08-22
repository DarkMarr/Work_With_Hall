using System.Collections.Generic;
using System.Linq;
using QuizGame.Utilities;
using UnityEngine;

namespace QuizGame.Resources
{
    public abstract class ResourceManager<TManager, TContent> : MonoSingleton<TManager> where TContent : ScriptableObject, IHasID where TManager : ResourceManager<TManager, TContent>
    {
        public abstract string ContentResourcePath { get; }

        private Dictionary<string, TContent> resourcesByID = new Dictionary<string, TContent>();

        protected override void Awake()
        {
            base.Awake();
            LoadAllResourcesAtPath(ContentResourcePath);
        }

        public TContent[] LoadAllResourcesAtPath(string path)
        {
            var resources = UnityEngine.Resources.LoadAll<TContent>(path);
            foreach (var resource in resources)
            {
                if (!resourcesByID.ContainsKey(resource.GetID()))
                {
                    resourcesByID.Add(resource.GetID(), resource);
                }
                 else
                {
                    Debug.LogError($"[{GetType().Name}] ID: {resource .GetID()} already exist.");
                }
            }
            return resources;
        }

        public int Count() => resourcesByID.Count();

        public TContent[] GetAllResources() => resourcesByID.Values.ToArray();

        public TContent GetRandomResource() => GetResource(GetRandomResourceID());

        public IEnumerable<TContent> GetRandomResources(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                yield return GetRandomResource();
            }
        }

        public TContent GetResourceAtIndex(int index)
        {
            if (index >= Count() || index < 0)
            {
                Debug.LogError($"[{GetType().Name}] index out of range.");
                return null;
            }
            return resourcesByID.ElementAt(index).Value;
        }

        public string[] GetAllResourcesID() => resourcesByID.Keys.ToArray();

        public TContent GetResource(string id)
        {
            if (resourcesByID.TryGetValue(id, out var item))
            {
                return item;
            }
            Debug.LogError($"[{GetType().Name}] ID: {id} doesn't exist.");
            return null;
        }

        public TContent GetResource<T>(string id) where T : TContent
        {
            if (resourcesByID.TryGetValue(id, out var item))
            {
                return item as T;
            }
            Debug.LogError($"[{GetType().Name}] ID: {id} doesn't exist.");
            return null;
        }

        public string GetRandomResourceID()
        {
            if (resourcesByID == null || resourcesByID.Count == 0)
            {
                Debug.LogWarning($"[{GetType().Name}] Dictionary is empty. Cannot get a random item.");
                return null;
            }
            var allItemIDs = resourcesByID.Values.Select(x => x.GetID()).ToArray();
            var randomIndex = Random.Range(0, allItemIDs.Count());
            return allItemIDs[randomIndex];
        }
    }
}
