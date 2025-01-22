using System.Collections.Generic;
using _CasualBusJam.Scripts._Enum;
using UnityEngine;

namespace _CasualBusJam.Scripts._Data
{
    [CreateAssetMenu(fileName = "ScriptableObject", menuName = "ScriptableObject/MaterialHolder", order = 0)]
    public class MaterialHolder : ScriptableObject
    {
        [Header("------ Material Elements ------")]
        public List<Material> materialsList;
        private Dictionary<ColorEnum, Material> _materialDictionary;

        public void InitializeMaterialDictionary()
        {
            _materialDictionary = new Dictionary<ColorEnum, Material>();

            foreach (Material material in materialsList)
            {
                foreach (ColorEnum color in System.Enum.GetValues(typeof(ColorEnum)))
                {
                    if (material.name.Contains(color.ToString()))
                    {
                        _materialDictionary.Add(color, material);
                        break;
                    }
                }
            }
        }

        public Material FindMaterialByName(ColorEnum color)
        {
            if (_materialDictionary.ContainsKey(color))
            {
                return _materialDictionary[color];
            }
            else
            {
                Debug.Log("Material Not Found With the color " + color);
                return null;
            }
        }
    }
}