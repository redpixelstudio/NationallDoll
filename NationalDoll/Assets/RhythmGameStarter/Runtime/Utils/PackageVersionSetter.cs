using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    public class PackageVersionSetter : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;
        public TextAsset packageJson;

        void Start()
        {
            var parsedPackage = JsonUtility.FromJson<PackageJson>(packageJson.text);
            if (parsedPackage != null)
            {
                label.text = $"v{parsedPackage.version}";
            }
        }

    }

    [Serializable]
    public class PackageJson
    {
        public string version;
    }

}