using UnityEditor;
using UnityEngine;

namespace IkiDunya.Editor
{
    /// <summary>README'deki layer'ları (Ground, Obstacle, Trash, Hazard, Cover, Structure, Turtle, Predator) projeye ekler.</summary>
    public static class KatmanKurulumu
    {
        // Water Unity'de hazır gelir; diğerleri boş ilk yuvalara eklenir.
        private static readonly string[] Katmanlar =
            { "Ground", "Obstacle", "Trash", "Hazard", "Cover", "Structure", "Turtle", "Predator" };

        [MenuItem("İki Dünya/Layer'ları kur")]
        public static void Kur()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            foreach (string ad in Katmanlar)
            {
                if (LayerMask.NameToLayer(ad) >= 0) continue;

                bool eklendi = false;
                // 0–7 Unity'nin kendi layer'ları; kullanıcı layer'ları 8'den başlar.
                for (int i = 8; i < layers.arraySize && !eklendi; i++)
                {
                    SerializedProperty yuva = layers.GetArrayElementAtIndex(i);
                    if (!string.IsNullOrEmpty(yuva.stringValue)) continue;
                    yuva.stringValue = ad;
                    eklendi = true;
                }
                if (!eklendi) Debug.LogError($"KatmanKurulumu: '{ad}' için boş layer yok.");
            }

            tagManager.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }
    }
}
