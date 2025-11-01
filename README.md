# Chat Socket Uygulaması

.NET Windows Forms ile geliştirilmiş TCP/IP tabanlı bir chat uygulaması.

## 📋 İçerik

Bu proje iki ayrı uygulamadan oluşur:
- **ChatServer**: Sunucu uygulaması (birden fazla istemciyi yönetir)
- **ChatClient**: İstemci uygulaması (sunucuya bağlanır ve mesajlaşır)

## 🚀 Özellikler

### Sunucu (Server)
- ✅ Birden fazla istemciyi aynı anda destekler
- ✅ Bağlı kullanıcıları listeler
- ✅ Tüm mesajları loglar
- ✅ Yerel IP adresini gösterir
- ✅ Port seçimi yapılabilir
- ✅ Sistem mesajları (kullanıcı girişi/çıkışı)

### İstemci (Client)
- ✅ Sunucuya kolay bağlantı
- ✅ Kullanıcı adı belirleme
- ✅ Renkli mesaj gösterimi
- ✅ Zaman damgalı mesajlar
- ✅ Enter tuşu ile mesaj gönderme
- ✅ Sistem bildirimlerini farklı renkte gösterme

## 📦 Gereksinimler

- .NET 6.0 SDK veya üzeri
- Windows işletim sistemi
- Visual Studio 2022 (önerilen) veya Visual Studio Code

## 🔧 Kurulum ve Çalıştırma

### Visual Studio ile:

1. **Projeyi açın:**
   ```
   ChatSocket.sln dosyasına çift tıklayın
   ```

2. **Sunucuyu başlatın:**
   - Solution Explorer'da `ChatServer` projesine sağ tıklayın
   - "Set as Startup Project" seçin
   - F5 tuşuna basın veya "Start" butonuna tıklayın
   - Sunucu penceresinde "Sunucuyu Başlat" butonuna tıklayın

3. **İstemciyi başlatın:**
   - Solution Explorer'da `ChatClient` projesine sağ tıklayın
   - "Debug" > "Start New Instance" seçin
   - Birden fazla istemci çalıştırmak için bu adımı tekrarlayın

### Komut Satırı ile:

1. **Sunucuyu çalıştırın:**
   ```bash
   cd ChatServer
   dotnet run
   ```

2. **Yeni bir terminal açın ve istemciyi çalıştırın:**
   ```bash
   cd ChatClient
   dotnet run
   ```

3. **Birden fazla istemci için:**
   - Her istemci için yeni bir terminal penceresi açın
   - Yukarıdaki komutu tekrarlayın

## 📱 Kullanım

### Sunucu:
1. Port numarasını girin (varsayılan: 8888)
2. "Sunucuyu Başlat" butonuna tıklayın
3. Yerel IP adresinizi not edin
4. İstemcilerin bağlanmasını bekleyin

### İstemci:
1. Sunucu IP adresini girin (aynı bilgisayarda test için: 127.0.0.1)
2. Port numarasını girin (sunucu ile aynı olmalı)
3. Kullanıcı adınızı girin
4. "Bağlan" butonuna tıklayın
5. Mesaj yazıp "Gönder" butonuna tıklayın veya Enter tuşuna basın

## 🌐 Ağ Üzerinden Kullanım

Farklı bilgisayarlar arasında kullanmak için:

1. **Sunucu bilgisayarında:**
   - Windows Güvenlik Duvarı'ndan ilgili portu açın
   - Sunucunun IP adresini öğrenin (ipconfig komutu ile)

2. **İstemci bilgisayarlarında:**
   - Sunucu IP adresi olarak sunucunun gerçek IP'sini girin
   - Bağlan butonuna tıklayın

### Windows Güvenlik Duvarı İzni:
```powershell
# PowerShell'i yönetici olarak çalıştırın
New-NetFirewallRule -DisplayName "Chat Server" -Direction Inbound -LocalPort 8888 -Protocol TCP -Action Allow
```

## 🎨 Özellikler Detay

### Mesaj Renkleri:
- **Yeşil**: Bağlantı başarılı mesajları
- **Kırmızı**: Hata ve bağlantı kesme mesajları
- **Mavi**: Sistem mesajları (kullanıcı girişi/çıkışı)
- **Siyah**: Diğer kullanıcıların mesajları
- **Koyu Yeşil**: Kendi gönderdiğiniz mesajlar

### Güvenlik:
- Port aralığı 1024-65535 ile sınırlı
- TCP bağlantısı üzerinden güvenli iletişim
- Hata yönetimi ve bağlantı kontrolü

## 🐛 Sorun Giderme

### "Bağlantı hatası" alıyorsanız:
- Sunucunun çalıştığından emin olun
- IP adresi ve port numarasının doğru olduğunu kontrol edin
- Güvenlik duvarı ayarlarını kontrol edin

### Mesajlar gönderilmiyorsa:
- Bağlantının aktif olduğunu kontrol edin
- Sunucu loglarını inceleyin
- İnternet bağlantınızı kontrol edin

### Port kullanımda hatası:
- Farklı bir port numarası deneyin
- Portun başka bir uygulama tarafından kullanılmadığından emin olun

## 📝 Teknik Detaylar

- **Dil**: C#
- **Framework**: .NET 6.0
- **UI**: Windows Forms
- **Protokol**: TCP/IP
- **Encoding**: UTF-8
- **Asenkron İşlemler**: async/await pattern
- **Threading**: Background threads for client handling

## 📂 Proje Yapısı

```
chatsocket/
│
├── ChatServer/
│   ├── Program.cs          # Sunucu giriş noktası
│   ├── ServerForm.cs       # Sunucu UI ve socket yönetimi
│   └── ChatServer.csproj   # Sunucu proje dosyası
│
├── ChatClient/
│   ├── Program.cs          # İstemci giriş noktası
│   ├── ClientForm.cs       # İstemci UI ve socket yönetimi
│   └── ChatClient.csproj   # İstemci proje dosyası
│
├── ChatSocket.sln          # Visual Studio solution dosyası
└── README.md               # Bu dosya
```

## 🎓 Eğitim Amaçlı Notlar

Bu proje aşağıdaki konuları öğrenmek için idealdir:
- Socket programlama temelleri
- TCP/IP protokolü
- Asenkron programlama (async/await)
- Thread yönetimi
- Windows Forms UI geliştirme
- Client-Server mimarisi
- Ağ programlama

## 💡 Geliştirme Fikirleri

Projeyi geliştirmek için fikirler:
- [ ] Özel mesaj gönderme (DM)
- [ ] Dosya paylaşımı
- [ ] Şifreleme ekle
- [ ] Kullanıcı kimlik doğrulama
- [ ] Mesaj geçmişi kaydetme
- [ ] Emoji desteği
- [ ] Bildirim sesleri
- [ ] Çevrimiçi/çevrimdışı durumu gösterme

## 📄 Lisans

Bu proje eğitim amaçlı hazırlanmıştır ve serbestçe kullanılabilir.

## 👨‍💻 Geliştirici Notu

Sorularınız veya önerileriniz için pull request açabilir veya issue oluşturabilirsiniz.

---

**Not**: Bu uygulama okul projesi olarak geliştirilmiştir. Üretim ortamında kullanmadan önce güvenlik ve performans iyileştirmeleri yapılmalıdır.

