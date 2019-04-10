using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Akatsuki.Validators
{
    public class NonSaveBaseProcessor : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            var find = paths.Any(p => p.Contains(".unity"));
            if (!find)
            {
                return paths;
            }

            var report = Object.FindObjectOfType<NonSaveBase>();
            if (report == null)
            {
                return paths;
            }

            var gameObjects = new List<GameObject>();
            foreach (Transform child in report.transform)
            {
                gameObjects.Add(child.gameObject);
            }

            foreach (var child in gameObjects)
            {
                Object.DestroyImmediate(child);
            }

            return paths;
        }
    }
}
