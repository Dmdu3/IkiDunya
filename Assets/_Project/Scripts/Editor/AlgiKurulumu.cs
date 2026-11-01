using IkiDunya.Camera;
using IkiDunya.Hazards;
using IkiDunya.Turtle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IkiDunya.Editor
{
    /// <summary>Var olan bir sahnede kaplumbağa algısını elle kurar: oyuncu gözüne algıyı ekler, lambaları ay, poşetleri denizanası yapar.
    /// Sahneleri baştan kuran komutlar bunu zaten kendisi ekler; bu araç elle kurulmuş deneme sahneleri içindir.</summary>
    public static class AlgiKurulumu
    {
        [MenuItem("İki Dünya/Kaplumbağa algısını kur (bu sahne)")]
        public static void Kur()
        {
            AlgiAyarlari ayarlar = GriKumsalKur.Ayar<AlgiAyarlari>("AlgiAyarlari");
            int gozSayisi = 0, lambaSayisi = 0, posetSayisi = 0;

            foreach (BirinciSahisKamera bakis in Object.FindObjectsByType<BirinciSahisKamera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                GameObject goz = bakis.gameObject;
                if (goz.GetComponent<KaplumbagaAlgisi>() != null) continue;
                var algi = goz.AddComponent<KaplumbagaAlgisi>();
                GriKumsalKur.Bagla(algi, "ayarlar", ayarlar);
                GriKumsalKur.Bagla(algi, "goz", goz.transform);
                EditorUtility.SetDirty(algi);
                gozSayisi++;
            }

            foreach (IsikKaynagi isik in Object.FindObjectsByType<IsikKaynagi>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (isik.GetComponent<AlgiNesnesi>() != null) continue;
                Transform direk = isik.transform.Find("Direk");
                Transform lamba = isik.transform.Find("Lamba");
                if (lamba == null) lamba = isik.transform;
                GriKumsalKur.AlgiEkle(isik.gameObject, AlgiTuru.Isik, AlgiTuru.Ay, direk, 1f, true, isik, lamba);
                EditorUtility.SetDirty(isik);
                lambaSayisi++;
            }

            foreach (Yiyecek yiyecek in Object.FindObjectsByType<Yiyecek>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (yiyecek.Tur != Yiyecek.Turu.Poset || yiyecek.GetComponent<AlgiNesnesi>() != null) continue;
                Transform gorunus = yiyecek.transform.childCount > 0 ? yiyecek.transform.GetChild(0) : null;
                GriKumsalKur.AlgiEkle(yiyecek.gameObject, AlgiTuru.Poset, AlgiTuru.Denizanasi, gorunus, 0.5f);
                EditorUtility.SetDirty(yiyecek);
                posetSayisi++;
            }

            foreach (UnityEngine.SceneManagement.Scene sahne in Sahneler())
                if (sahne.IsValid()) EditorSceneManager.MarkSceneDirty(sahne);

            Debug.Log($"Kaplumbağa algısı kuruldu: {gozSayisi} göz, {lambaSayisi} lamba (ay), {posetSayisi} poşet (denizanası).");
        }

        private static UnityEngine.SceneManagement.Scene[] Sahneler()
        {
            var sahneler = new UnityEngine.SceneManagement.Scene[UnityEngine.SceneManagement.SceneManager.loadedSceneCount];
            for (int i = 0; i < sahneler.Length; i++)
                sahneler[i] = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            return sahneler;
        }
    }
}
