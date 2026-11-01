using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IkiDunya.Editor
{
    /// <summary>Tek tık araçlar: bütün sahneleri doğru sırayla kurmak ve Windows build'i almak.</summary>
    public static class Araclar
    {
        private const string MenuSahnesi = "Assets/_Project/Scenes/Levels/Menu.unity";
        private const string BuildKlasoru = "Build/Windows";

        /// <summary>Oyunun sahneleri, build sırasıyla (Menu ilk).</summary>
        private static readonly string[] Sahneler =
        {
            MenuSahnesi,
            "Assets/_Project/Scenes/Levels/Bolum1_Donus.unity",
            "Assets/_Project/Scenes/Levels/Kumsal.unity",
            "Assets/_Project/Scenes/Levels/Bolum5_IlkYuzus.unity",
            "Assets/_Project/Scenes/Levels/Parkur_PosetSurusu.unity",
            "Assets/_Project/Scenes/Levels/Parkur_CopSeli.unity",
            "Assets/_Project/Scenes/Levels/Parkur_CopGirdabi.unity",
        };

        [MenuItem("İki Dünya/Hepsini kur", priority = 0)]
        public static void HepsiniKur()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Hepsini kur", "Menü, kumsal, Bölüm 1, Bölüm 5 ve üç parkur sahnesi baştan kurulacak. Sahnelerde elle yapılan değişiklikler gider.", "Kur", "Vazgeç"))
                return;

            // Menü önce: Sistemler kalıbını kurar, diğer sahneler onu koyar.
            var adimlar = new (string ad, Action kur)[]
            {
                ("Menü ve duraklatma", MenuKur.Kur),
                ("Kumsal", GriKumsalKur.KumsalSahnesiKur),
                ("Bölüm 1", GriDenizKur.Bolum1Kur),
                ("Bölüm 5", GriDenizKur.Bolum5Kur),
                ("Parkur: Poşet sürüsü", GriDenizKur.PosetSurusuKur),
                ("Parkur: Çöp girdabı", GriDenizKur.CopGirdabiKur),
                ("Parkur: Çöp seli", GriKumsalKur.CopSeliKur),
            };

            GriKumsalKur.SormadanKur = true;
            try
            {
                for (int i = 0; i < adimlar.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Hepsini kur", adimlar[i].ad, (float)i / adimlar.Length);
                    adimlar[i].kur();
                }
            }
            finally
            {
                GriKumsalKur.SormadanKur = false;
                EditorUtility.ClearProgressBar();
            }
            SahneListesiniDuzelt();
            EditorSceneManager.OpenScene(MenuSahnesi);
            Debug.Log("Hepsi kuruldu. Menu sahnesi açık; Play'e basabilirsin.");
        }

        [MenuItem("İki Dünya/PC build al", priority = 1)]
        public static void PcBuildAl()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string[] sahneler = SahneListesiniDuzelt();
            if (sahneler.Length == 0)
            {
                EditorUtility.DisplayDialog("PC build", "Oyun sahneleri bulunamadı. Önce İki Dünya → Hepsini kur.", "Tamam");
                return;
            }

            Directory.CreateDirectory(BuildKlasoru);
            var secenekler = new BuildPlayerOptions
            {
                scenes = sahneler,
                locationPathName = Path.Combine(BuildKlasoru, "IkiDunya.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };
            BuildReport rapor = BuildPipeline.BuildPlayer(secenekler);
            if (rapor.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"PC build hazır: {Path.GetFullPath(BuildKlasoru)} ({rapor.summary.totalSize / (1024 * 1024)} MB, {rapor.summary.totalTime.TotalSeconds:0} sn).");
                EditorUtility.RevealInFinder(Path.Combine(BuildKlasoru, "IkiDunya.exe"));
            }
            else Debug.LogError($"PC build olmadı: {rapor.summary.result}, {rapor.summary.totalErrors} hata. Console'daki kırmızı satırlara bak.");
        }

        /// <summary>Build listesini oyunun sahneleriyle aynı sıraya getirir (var olanlar); listenin kendisini döner.</summary>
        private static string[] SahneListesiniDuzelt()
        {
            var liste = new List<EditorBuildSettingsScene>();
            var yollar = new List<string>();
            foreach (string yol in Sahneler)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(yol) == null) continue;
                liste.Add(new EditorBuildSettingsScene(yol, true));
                yollar.Add(yol);
            }
            EditorBuildSettings.scenes = liste.ToArray();
            return yollar.ToArray();
        }
    }
}
