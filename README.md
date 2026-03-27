# TicketFlow - Yapay Zeka Destekli Akıllı Bilet Yönetim Sistemi

TicketFlow, müşteri destek süreçlerini uçtan uca otomatize eden, kurumsal standartlarda tasarlanmış tam yığın (full-stack) bir yardım masası uygulamasıdır. Projenin temel odak noktası, destek ekiplerinin üzerindeki operasyonel iş yükünü **Yapay Zeka (Makine Öğrenmesi)** entegrasyonu ile sıfıra indirmektir.

Proje, hem ön yüz (Frontend) hem de arka uç (Backend) kodlarının tek bir çatı altında izole bir şekilde yönetildiği **Monorepo** mimarisiyle kurgulanmıştır.

## 🧠 Projenin Amacı ve Sistem Mimarisi

Geleneksel biletleme sistemlerinin aksine TicketFlow, kullanıcı taleplerini pasif bir şekilde veritabanına yazmaz. Sisteme entegre edilen makine öğrenmesi motoru, kullanıcının girdiği destek talebini (başlık ve içerik) doğal dil işleme mantığıyla anlık olarak analiz eder. Sistem, insan müdahalesine gerek kalmadan talebin hangi kategoriye (örn: Teknik Destek, Fatura, Şikayet) ait olduğunu yüksek doğruluk payıyla tahmin eder ve veritabanına otonom olarak kaydeder.

Sürdürülebilirlik ve kod kalitesi için arka uçta **Katmanlı Mimari (Layered Architecture)** ve **Repository Design Pattern** prensipleri uygulanmış; veri erişimi ile iş kuralları (Business Logic) birbirinden tamamen soyutlanmıştır. Kurumsal ölçekte yüksek performans ve güvenlik sağlamak amacıyla ORM (Object-Relational Mapping) araçlarının hantallığından kaçınılmış, tüm veritabanı CRUD operasyonları doğrudan **Stored Procedure**'ler aracılığıyla gerçekleştirilmiştir.

## 🚀 Öne Çıkan Özellikler

* **🤖 Otonom Bilet Sınıflandırma:** Arka uca entegre edilen özel eğitilmiş **ML.NET** modeli ile akıllı kategori tahmini.
* **🏗️ Yüksek Performanslı Veri Erişimi:** Stored Procedure tabanlı, SQL Server üzerinde optimize edilmiş veritabanı mimarisi.
* **🔐 Uçtan Uca Güvenlik:** JWT (JSON Web Token) ile rol tabanlı (Admin/Kullanıcı) yetkilendirme.
* **⚡ Dinamik SPA Deneyimi:** Angular ile geliştirilmiş; `AuthInterceptor` ile istek güvenliği, `AuthGuard` ile sayfa erişim güvenliği sağlanan hızlı önyüz.
* **📂 Monorepo Tasarımı:** Ön yüz ve arka yüz kodlarının aynı hiyerarşide, senkronize çalıştığı modern klasör yapısı.

## 🛠️ Kullanılan Teknolojiler

**Arka Uç (Backend)**
* C# .NET Core Web API
* ML.NET (Machine Learning Model Builder)
* Microsoft SQL Server & Stored Procedures
* JWT Authentication
* Repository Design Pattern

**Ön Yüz (Frontend)**
* Angular (SPA)
* TypeScript
* RxJS (Asenkron Veri Yönetimi)
* HTML5 / CSS3

## 📁 Monorepo Klasör Yapısı

```text
TicketFlow/
 ├── Backend/
 │    ├── Controllers/         # API Uç Noktaları
 │    ├── Data/                # Repository Pattern Sınıfları ve Stored Procedure Çağrıları
 │    ├── DTOs/                # Veri Transfer Objeleri
 │    ├── TicketClassifier/    # ML.NET Yapay Zeka Modeli
 │    └── Program.cs           # API Konfigürasyonları ve CORS
 │
 └── Frontend/
      ├── src/app/core/        # Interceptor, Guard, Service ve Modeller
      ├── src/app/pages/       # Auth ve Dashboard Ekranları (UI Components)
      └── package.json         # Angular Bağımlılıkları
