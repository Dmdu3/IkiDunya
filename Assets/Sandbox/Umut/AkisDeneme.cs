using IkiDunya.Core;
using UnityEngine;

namespace IkiDunya.Sandbox
{
    /// <summary>Geçici deneme paneli: oyun akışını ve KumsalDurumu'nu ekrandan dener. Oyuna girmez.</summary>
    public class AkisDeneme : MonoBehaviour
    {
        [SerializeField] private KumsalDurumu kumsal;
        [Tooltip("Akışın olmadığı deneme sahnelerinde 'Kumsalı sıfırla' düğmesi için.")]
        [SerializeField] private int denemeBolgeSayisi = 8;
        [SerializeField] private int denemeKatmanEnFazla = 3;

        private int degisiklikSayisi;

        private void OnEnable()
        {
            OyunAkisi.AdimBasladi += AdimBasladi;
            OyunAkisi.MenuyeDonuldu += MenuyeDonuldu;
            kumsal.Degisti += KumsalDegisti;
        }

        private void OnDisable()
        {
            OyunAkisi.AdimBasladi -= AdimBasladi;
            OyunAkisi.MenuyeDonuldu -= MenuyeDonuldu;
            kumsal.Degisti -= KumsalDegisti;
        }

        private void AdimBasladi(Adim adim) => Debug.Log($"[AkisDeneme] Adım başladı: {adim}");
        private void MenuyeDonuldu() => Debug.Log("[AkisDeneme] Menüye dönüldü");
        private void KumsalDegisti() => degisiklikSayisi++;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 320, 520), GUI.skin.box);

            GUILayout.Label(OyunAkisi.MenudeMi ? "Menü" : $"Adım: {OyunAkisi.SuAnkiAdim}");
            GUILayout.Label($"Bölge: {kumsal.BolgeSayisi} · Yuva: {kumsal.Yuvalar.Count} · Kayıp izi: {kumsal.KayipIzleri.Count}");
            if (kumsal.BolgeSayisi > 0)
                GUILayout.Label($"Bölge 0 çöp: {kumsal.BolgeAl(0).Seviye(BolgeKatmani.Cop)} · Gölgelik: {kumsal.BolgeAl(0).Var(BolgeYapisi.Golgelik)}");
            GUILayout.Label($"Degisti event'i: {degisiklikSayisi} kez");

            if (OyunAkisi.MenudeMi)
            {
                if (GUILayout.Button("Yeni oyun")) OyunAkisi.YeniOyun();
            }
            else
            {
                if (GUILayout.Button("Adımı bitir")) OyunAkisi.AdimBitti();
                if (GUILayout.Button("Menüye dön")) OyunAkisi.MenuyeDon();
            }

            if (GUILayout.Button("Kumsalı sıfırla")) kumsal.Sifirla(denemeBolgeSayisi, denemeKatmanEnFazla);
            if (GUILayout.Button($"Kum ısınması +1 (şu an {kumsal.KumIsinmasi})")) kumsal.KumIsinmasiDegistir(1);
            if (GUILayout.Button("Kum ısınması −1")) kumsal.KumIsinmasiDegistir(-1);

            if (kumsal.BolgeSayisi > 0)
            {
                if (GUILayout.Button("Bölge 0: gölgelik kaldır")) kumsal.YapiKaldir(0, BolgeYapisi.Golgelik);
                if (GUILayout.Button("Bölge 0: çöp +1")) kumsal.KatmanDegistir(0, BolgeKatmani.Cop, 1);
                if (GUILayout.Button("Bölge 0: gölgelik koy")) kumsal.YapiKoy(0, BolgeYapisi.Golgelik);
                if (GUILayout.Button("Yuva ekle"))
                    kumsal.YuvaEkle(new Yuva(0, transform.position, SicaklikBandi.Yesil, false, 12, 6));
                if (GUILayout.Button("Kayıp izi ekle"))
                    kumsal.KayipIziEkle(new KayipIzi(0, transform.position, KayipNedeni.Marti));
            }

            GUILayout.EndArea();
        }
    }
}
