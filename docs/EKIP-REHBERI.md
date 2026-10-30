---
ozet: Ekip için kurulum ve günlük çalışma rehberi. Kod yazmayanlar da bu adımlarla çalışabilir.
---

# Ekip rehberi

## 1. Bir kere: kurulum

1. **Unity Hub** kur → Installs → **Unity 6000.3.25f1**. Herkes bu sürümü kullanır; farklı sürüm proje dosyalarını bozar.
2. **GitHub hesabı** aç (yoksa) ve kullanıcı adını Umut'a gönder.
3. E-postana gelen **repo davetini** kabul et.
4. **GitHub Desktop** kur ve hesabınla giriş yap. (Toprak ve Umut isterse komut satırı kullanır.)
5. GitHub Desktop → **File → Clone repository** → `IkiDunya` → bilgisayarında bir klasör seç.
6. **Git LFS:** GitHub Desktop'ta kuruludur; komut satırı kullanıyorsan bir kere `git lfs install`.
7. Unity Hub → **Add → Add project from disk** → klonladığın klasörün kendisi (`IkiDunya`). İlk açılış birkaç dakika sürer.
8. Unity'de `Assets/Sandbox/<Ad>/` içinde kendi deneme sahneni oluştur.

## 2. Her çalışmada

1. GitHub Desktop → **Current branch: main** → **Fetch origin** → **Pull**.
2. **Current branch → New branch** → `isim/kisa-is` (ör. `yusuf/kart-metinleri`).
3. Çalış. Unity'de **sadece kendi klasörlerinde ve kendi Sandbox'ında** değişiklik yap (`docs/klasorler.md`). Dosyaları Unity'nin Project penceresinden taşı ve sil.
4. GitHub Desktop'ta değişikliklere bak; ilgisiz dosya varsa sağ tık → **Discard**.
5. Altta kısa bir açıklama yaz → **Commit** → **Push origin**.
6. **Create Pull Request** → tarayıcıda şablonu doldur → **Create**.
7. Kendi alanınsa birleştir (**Merge**); değilse Umut'a haber ver.

Belgeler (`docs/`) GitHub sitesinden de düzenlenebilir: dosyayı aç → kalem → en altta **Create a new branch** → Propose changes.

## 3. Çakışma olursa

GitHub Desktop "conflict" derse dosyaya dokunma, Umut'a yaz. Sahne ve prefab çakışmaları elle çözülmez; kimin sürümünün kalacağına birlikte karar verilir.

## Unity akıllı birleştirme (Toprak ve Umut, isteğe bağlı)

Prefab ve ScriptableObject çakışmalarını azaltır. Repo klasöründe bir kere:

```
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "\"C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Data/Tools/UnityYAMLMerge.exe\" merge -p %O %B %A %A"
```
