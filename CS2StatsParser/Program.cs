using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using static CS2StatsParser.CS2StatsParser;

namespace CS2StatsParser
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Console.WriteLine("Steam profil URL'sini yapıştırın:");
            var url = Console.ReadLine();

            try
            {
                // DLL'den URL işleme işlevini kullan
                url = UrlHelper.GetSteamUrl(url);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                Environment.Exit(0);
            }


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new Form { Width = 1024, Height = 768, Text = "CS2 İstatistik Ayrıştırıcısı" };
            var webView = new WebView2 { Dock = DockStyle.Fill };
            form.Controls.Add(webView);

            form.Load += async (s, e) =>
            {
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.NavigationCompleted += async (sender, args) =>
                {
                    if (args.IsSuccess)
                    {
                        Console.WriteLine("Sayfa yüklendi, kritik veriler kontrol ediliyor...");

                        // Gelişmiş veri kontrolü - kritik verileri bekle
                        bool dataReady = false;
                        for (int i = 0; i < 10 && !dataReady; i++)
                        {
                            string checkScript = @"
                                (() => {
                                    // Steam verileri
                                    const steamUser = document.querySelector('a[href*=""steamcommunity.com""]');
                                    const steamLevel = document.querySelector('p.text-white.friendPlayerLevel');
                                    const friends = document.querySelector('p:contains(""FRIENDS"")');
                                    const playtime = document.querySelector('p:contains(""CS2 PLAYTIME"")');
                                    
                                    // Leetify verileri
                                    const leetifyUser = document.querySelector('a[href*=""leetify.com""]');
                                    const aimStat = document.querySelector('p:contains(""AIM"")');
                                    
                                    // Silah verileri - en kritik
                                    const weapons = document.querySelectorAll('span[data-tip*=""kills""] img[src*=""equipment""]');
                                    
                                    // Kollektif veriler
                                    const collectibles = document.querySelectorAll('img[alt*=""Pin""], img[alt*=""Coin""], img[alt*=""tournament""]');
                                    
                                    // Commendation verileri
                                    const commendations = document.querySelectorAll('div[data-tip=""Friendy""], div[data-tip=""Leader""], div[data-tip=""Teacher""]');
                                    
                                    // Temel veriler hazır mı?
                                    const basicReady = steamUser && steamLevel;
                                    
                                    // Tam veriler hazır mı?
                                    const fullReady = basicReady && weapons.length > 3 && collectibles.length > 0;
                                    
                                    return {
                                        ready: fullReady || (basicReady && weapons.length > 0),
                                        steamUser: !!steamUser,
                                        steamLevel: !!steamLevel,
                                        weaponCount: weapons.length,
                                        collectibleCount: collectibles.length,
                                        commendationCount: commendations.length,
                                        leetify: !!leetifyUser
                                    };
                                })();";

                            try
                            {
                                string resultJson = await webView.CoreWebView2.ExecuteScriptAsync(checkScript);
                                var result = JsonConvert.DeserializeObject<dynamic>(JsonConvert.DeserializeObject<string>(resultJson));
                                dataReady = (bool)result.ready;

                                if (!dataReady)
                                {
                                    // Her 5 saniyede detaylı bilgi göster
                                    if (i % 5 == 0 || i < 10)
                                    {
                                        Console.WriteLine($"Bekleniyor... ({i + 1}/10) - Steam: {result.steamUser}, Silahlar: {result.weaponCount}, Koleksiyon: {result.collectibleCount}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Bekleniyor... ({i + 1}/10)");
                                    }
                                    await Task.Delay(1000);
                                }
                                else
                                {
                                    Console.WriteLine($"✅ Veriler hazır! Silahlar: {result.weaponCount}, Koleksiyon: {result.collectibleCount}");
                                }
                            }
                            catch
                            {
                                Console.WriteLine($"Bekleniyor... ({i + 1}/10) - Kontrol hatası");
                                await Task.Delay(1000);
                            }
                        }

                        if (!dataReady)
                        {
                            Console.WriteLine("⚠️ Maksimum bekleme süresi doldu, mevcut verilerle devam ediliyor...");
                        }

                        // HTML'i al ve ayrıştır
                        string htmlJson = await webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
                        string html = JsonConvert.DeserializeObject<string>(htmlJson);

                        // DLL'den parsing işlevini kullan
                        var playerStats = HtmlParser.ParsePlayerStats(html);
                        HtmlParser.DisplayStats(playerStats);
                    }
                    else
                    {
                        Console.WriteLine($"Sayfa yüklenemedi: {args.WebErrorStatus}");
                    }
                };

                webView.Source = new Uri(url);
            };

            Application.Run(form);
        }
    }
}