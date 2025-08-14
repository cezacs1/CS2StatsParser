using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace CS2StatsParser
{
    public class CS2StatsParser
    {
        public class PlayerStats
        {
            public SteamProfile Steam { get; set; } = new SteamProfile();
            public LeetifyProfile Leetify { get; set; } = new LeetifyProfile();
            public FaceitProfile Faceit { get; set; } = new FaceitProfile();
            public List<WeaponStat> Weapons { get; set; } = new List<WeaponStat>();
            public List<CollectibleItem> Collectibles { get; set; } = new List<CollectibleItem>();
        }

        public class SteamProfile
        {
            public string Username { get; set; } = "";
            public string VanityUrl { get; set; } = "";
            public int Friends { get; set; }
            public int XpLevel { get; set; }
            public int SteamLevel { get; set; }
            public string TotalPlaytime { get; set; } = "";
            public string RecentPlaytime { get; set; } = "";
            public string GeneralTotalPlaytime { get; set; } = "";
            public string GeneralRecentPlaytime { get; set; } = "";
            public int TotalGames { get; set; }
            public int FriendlyCommendations { get; set; }
            public int LeaderCommendations { get; set; }
            public int TeacherCommendations { get; set; }
            public bool HasHighCommendations { get; set; }
            public string ProfileUrl { get; set; } = "";
            public DateTime RegisterDate { get; set; }
            public bool HasGameBans { get; set; }
            public int GameBanCount { get; set; }
            public string LastBanDate { get; set; } = "";
            public string LastBanText { get; set; } = "";
        }

        public class LeetifyProfile
        {
            public string Username { get; set; } = "";
            public double Aim { get; set; }
            public double Utility { get; set; }
            public double Position { get; set; }
            public double Clutch { get; set; }
            public double Opening { get; set; }
            public double KD { get; set; }
            public double Rating { get; set; }
            public double Party { get; set; }
            public double Preaim { get; set; }
            public int TimeToDamage { get; set; }
            public double AvgHeDamageEnemies { get; set; }
            public double AvgHeDamageTeammates { get; set; }
            public int WinrateAllTime { get; set; }
            public int WinrateRecent { get; set; }
            public int Matches { get; set; }
            public double BannedMates { get; set; }
            public bool HasSuspiciousPreaim { get; set; }
            public bool HasSuspiciousTTD { get; set; }
            public bool IsLeetifyUser { get; set; }
            public string MaxRating { get; set; } = "";
            public bool IsBannedInMatchmaking { get; set; }
        }

        public class FaceitProfile
        {
            public bool Found { get; set; }
            public string ErrorMessage { get; set; } = "";
            public string Username { get; set; } = "";
            public DateTime RegisterDate { get; set; }
            public string Country { get; set; } = "";
            public string Language { get; set; } = "";
            public FaceitGameStats CSGO { get; set; } = new FaceitGameStats();
            public FaceitGameStats CS2 { get; set; } = new FaceitGameStats();
        }

        public class FaceitGameStats
        {
            public int Elo { get; set; }
            public int Level { get; set; }
            public double Winrate { get; set; }
            public int Matches { get; set; }
            public int HeadshotPercent { get; set; }
            public double KD { get; set; }
            public double ADR { get; set; }
            public int Clutch1v1 { get; set; }
            public int Clutch1v2 { get; set; }
            public double UtilDamage { get; set; }
            public DateTime LastMatch { get; set; }
            public string RecentResults { get; set; } = "";
        }

        public class WeaponStat
        {
            public string WeaponName { get; set; } = "";
            public int Kills { get; set; }
            public double? Accuracy { get; set; }
            public string ImagePath { get; set; } = "";
        }

        public class CollectibleItem
        {
            public string Name { get; set; } = "";
            public string ImagePath { get; set; } = "";
        }

        public static class HtmlParser
        {
            public static PlayerStats ParsePlayerStats(string html)
            {
                var stats = new PlayerStats();
                ParseSteamProfile(html, stats.Steam);
                ParseLeetifyProfile(html, stats.Leetify);
                ParseFaceitProfile(html, stats.Faceit);
                ParseWeaponStats(html, stats.Weapons);
                ParseCollectibles(html, stats.Collectibles);
                return stats;
            }

            private static void ParseSteamProfile(string html, SteamProfile steam)
            {
                // Steam kullanıcı adı ve profil URL
                var usernameMatch = Regex.Match(html, @"<a href=""https://steamcommunity\.com/[^""]*""[^>]*>\s*([^<]+?)\s*</a>");
                if (usernameMatch.Success)
                {
                    steam.Username = usernameMatch.Groups[1].Value.Trim();
                    steam.ProfileUrl = Regex.Match(html, @"href=""(https://steamcommunity\.com/[^""]*)""").Groups[1].Value;
                }

                // Steam seviyesi
                var steamLevelMatch = Regex.Match(html, @"<p class=""text-white friendPlayerLevel[^""]*""[^>]*>\s*(\d+)\s*</p>");
                if (steamLevelMatch.Success) steam.SteamLevel = int.Parse(steamLevelMatch.Groups[1].Value);

                // Vanity URL
                var vanityMatch = Regex.Match(html, @"<p class=""text-xs"">VANITY</p>\s*<p class=""text-white"">([^<]+)</p>");
                if (vanityMatch.Success) steam.VanityUrl = vanityMatch.Groups[1].Value.Trim();

                // Arkadaş sayısı
                var friendsMatch = Regex.Match(html, @"<p class=""text-xs"">FRIENDS</p>\s*<p class=""text-white"">([^<]+)</p>");
                if (friendsMatch.Success && int.TryParse(friendsMatch.Groups[1].Value.Replace(",", ""), out int friends))
                    steam.Friends = friends;

                // Oyun sayısı - birden fazla format dene
                var gamesMatch = Regex.Match(html, @"<p class=""text-xs"">GAMES</p>\s*<p class=""text-white"">([^<]+)</p>");
                if (gamesMatch.Success && int.TryParse(gamesMatch.Groups[1].Value.Replace(",", "").Replace(".", ""), out int games))
                {
                    steam.TotalGames = games;
                }
                else
                {
                    // Alternatif format - "games" kelimesini ara
                    var altGamesMatch = Regex.Match(html, @">(\d+)\s*games?<", RegexOptions.IgnoreCase);
                    if (altGamesMatch.Success && int.TryParse(altGamesMatch.Groups[1].Value, out int altGames))
                        steam.TotalGames = altGames;
                }

                // XP seviyesi
                var xpMatch = Regex.Match(html, @"<p class=""text-xs"">XP LEVEL</p>\s*<span class=""text-white"">\s*<span[^>]*>\s*(\d+)");
                if (xpMatch.Success) steam.XpLevel = int.Parse(xpMatch.Groups[1].Value);

                // Genel oyun süresi
                var generalPlaytimeMatch = Regex.Match(html, @"<p class=""text-xs"">PLAYTIME</p>\s*<p class=""text-white"">\s*<span[^>]*>([^<]+)</span>\s*/\s*<span[^>]*>([^<]+)</span>");
                if (generalPlaytimeMatch.Success)
                {
                    steam.GeneralTotalPlaytime = generalPlaytimeMatch.Groups[1].Value.Trim();
                    steam.GeneralRecentPlaytime = generalPlaytimeMatch.Groups[2].Value.Trim();
                }

                // CS2 oyun süresi - birden fazla format dene
                var cs2PlaytimeMatch = Regex.Match(html, @"<p class=""text-xs"">CS2 PLAYTIME</p>\s*<p class=""text-white"">\s*<span[^>]*>([^<]+)</span>\s*/\s*<span[^>]*>([^<]+)</span>");
                if (cs2PlaytimeMatch.Success)
                {
                    steam.TotalPlaytime = cs2PlaytimeMatch.Groups[1].Value.Trim();
                    steam.RecentPlaytime = cs2PlaytimeMatch.Groups[2].Value.Trim();
                }
                else
                {
                    // Alternatif format dene
                    var altPlaytimeMatch = Regex.Match(html, @"CS2 PLAYTIME[^>]*>.*?<span[^>]*>([^<]*)</span>[^/]*/[^<]*<span[^>]*>([^<]*)</span>", RegexOptions.Singleline);
                    if (altPlaytimeMatch.Success)
                    {
                        steam.TotalPlaytime = altPlaytimeMatch.Groups[1].Value.Trim();
                        steam.RecentPlaytime = altPlaytimeMatch.Groups[2].Value.Trim();
                    }
                }

                // Ban bilgileri
                var banMatch = Regex.Match(html, @"<div class=""text-red-500 text-sm"">\s*<p>Game bans: (\d+)</p>");
                if (banMatch.Success)
                {
                    steam.HasGameBans = true;
                    steam.GameBanCount = int.Parse(banMatch.Groups[1].Value);
                    var lastBanMatch = Regex.Match(html, @"<p>Last ban ([^<]+)</p>");
                    if (lastBanMatch.Success) steam.LastBanText = lastBanMatch.Groups[1].Value.Trim();
                }

                // Beğeniler
                var friendlyMatch = Regex.Match(html, @"<div class=""tooltip"" data-tip=""Friendy"">\s*<span class=""([^""]*)"">([^<]+)</span>");
                if (friendlyMatch.Success)
                {
                    if (int.TryParse(friendlyMatch.Groups[2].Value.Replace(",", ""), out int friendly))
                    {
                        steam.FriendlyCommendations = friendly;
                        steam.HasHighCommendations = friendlyMatch.Groups[1].Value.Contains("text-orange-500");
                    }
                }

                var leaderMatch = Regex.Match(html, @"<div class=""tooltip"" data-tip=""Leader"">\s*<span class=""([^""]*)"">([^<]+)</span>");
                if (leaderMatch.Success && int.TryParse(leaderMatch.Groups[2].Value.Replace(",", ""), out int leader))
                    steam.LeaderCommendations = leader;

                var teacherMatch = Regex.Match(html, @"<div class=""tooltip"" data-tip=""Teacher"">\s*<span class=""([^""]*)"">([^<]+)</span>");
                if (teacherMatch.Success && int.TryParse(teacherMatch.Groups[2].Value.Replace(",", ""), out int teacher))
                    steam.TeacherCommendations = teacher;

                // Kayıt tarihi
                var dateMatch = Regex.Match(html, @"<time datetime=""([^""]+)"">");
                if (dateMatch.Success && DateTime.TryParse(dateMatch.Groups[1].Value, out DateTime regDate))
                    steam.RegisterDate = regDate;
            }

            private static void ParseLeetifyProfile(string html, LeetifyProfile leetify)
            {
                // Leetify kullanıcı adı
                var leetifyUsernameMatch = Regex.Match(html, @"<a href=""https://leetify\.com/[^""]*""[^>]*>\s*([^<]+?)\s*</a>");
                if (leetifyUsernameMatch.Success) leetify.Username = leetifyUsernameMatch.Groups[1].Value.Trim();

                // Leetify kullanıcısı kontrolü
                leetify.IsLeetifyUser = Regex.IsMatch(html, @"<div class=""tooltip[^""]*"" data-tip=""Leetify User"">");

                // İstatistikler
                var aimMatch = Regex.Match(html, @"<p class=""text-xs"">AIM</p>\s*<p class=""[^""]*"">\s*([\d.]+)");
                if (aimMatch.Success && double.TryParse(aimMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double aim))
                    leetify.Aim = aim;

                var utilityMatch = Regex.Match(html, @"<p class=""text-xs"">UTILITY</p>\s*<p class=""text-white"">\s*([\d.]+)");
                if (utilityMatch.Success && double.TryParse(utilityMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double utility))
                    leetify.Utility = utility;

                var positionMatch = Regex.Match(html, @"<p class=""text-xs"">POSITION</p>\s*<p class=""text-white"">\s*([\d.]+)");
                if (positionMatch.Success && double.TryParse(positionMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double position))
                    leetify.Position = position;

                var clutchMatch = Regex.Match(html, @"<p class=""text-xs"">CLUTCH</p>\s*<p class=""text-white"">\s*([+-]?[\d.]+)");
                if (clutchMatch.Success && double.TryParse(clutchMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double clutch))
                    leetify.Clutch = clutch;

                var openingMatch = Regex.Match(html, @"<p class=""text-xs"">OPENING</p>\s*<p class=""text-white"">\s*([+-]?[\d.]+)");
                if (openingMatch.Success && double.TryParse(openingMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double opening))
                    leetify.Opening = opening;

                var kdMatch = Regex.Match(html, @"<p class=""text-xs"">KD</p>\s*<p class=""[^""]*"">\s*([\d.]+)");
                if (kdMatch.Success && double.TryParse(kdMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double kd))
                    leetify.KD = kd;

                var ratingMatch = Regex.Match(html, @"<p class=""text-xs"">RATING</p>\s*<span[^>]*>\s*([+-]?[\d.]+)");
                if (ratingMatch.Success && double.TryParse(ratingMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double rating))
                    leetify.Rating = rating;

                var partyMatch = Regex.Match(html, @"<p class=""text-xs""><span[^>]*>PARTY</span></p>\s*<p class=""text-white"">\s*([\d.]+)");
                if (partyMatch.Success && double.TryParse(partyMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double party))
                    leetify.Party = party;

                // Preaim
                var preaimMatch = Regex.Match(html, @"<p class=""text-xs"">PREAIM</p>\s*<p class=""([^""]*)"">([^<]+?)°</p>");
                if (preaimMatch.Success)
                {
                    string preaimValue = preaimMatch.Groups[2].Value.Replace("Â", "").Trim();
                    if (double.TryParse(preaimValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double preaim))
                    {
                        leetify.Preaim = preaim;
                        leetify.HasSuspiciousPreaim = preaimMatch.Groups[1].Value.Contains("text-red-500");
                    }
                }

                // Time to damage
                var ttdMatch = Regex.Match(html, @"<p class=""text-xs"">TIME TO DMG</p>\s*<p class=""([^""]*)"">(\d+)ms</p>");
                if (ttdMatch.Success && int.TryParse(ttdMatch.Groups[2].Value, out int ttd))
                {
                    leetify.TimeToDamage = ttd;
                    leetify.HasSuspiciousTTD = ttdMatch.Groups[1].Value.Contains("text-red-500");
                }

                // Max rating
                var maxRatingMatch = Regex.Match(html, @"<p class=""text-xs"">MAX RATING</p>\s*<p[^>]*><div class=""cs2rating[^""]*"">\s*<span>\s*(\d+)\s*<span[^>]*>([^<]*)</span>");
                if (maxRatingMatch.Success) leetify.MaxRating = maxRatingMatch.Groups[1].Value + maxRatingMatch.Groups[2].Value;

                // HE damage
                var heDmgMatch = Regex.Match(html, @"<p class=""text-xs"">AVG HE DMG</p>\s*<p><span[^>]*>([\d.]+)</span>\s*/\s*<span[^>]*>([\d.]+)</span></p>");
                if (heDmgMatch.Success)
                {
                    if (double.TryParse(heDmgMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double enemyDmg))
                        leetify.AvgHeDamageEnemies = enemyDmg;
                    if (double.TryParse(heDmgMatch.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double teamDmg))
                        leetify.AvgHeDamageTeammates = teamDmg;
                }

                // Winrate
                var winrateMatch = Regex.Match(html, @"<p class=""text-xs"">WINRATE</p>\s*<span class=""text-white"">\s*<span[^>]*>\s*<span>\s*(\d+)%\s*</span>\s*</span>\s*</span>\s*<span[^>]*>\s*/\s*<span>\s*(\d+)%");
                if (winrateMatch.Success)
                {
                    if (int.TryParse(winrateMatch.Groups[1].Value, out int allTime)) leetify.WinrateAllTime = allTime;
                    if (int.TryParse(winrateMatch.Groups[2].Value, out int recent)) leetify.WinrateRecent = recent;
                }

                // Matches
                var matchesMatch = Regex.Match(html, @"<button[^>]*class=""link text-white""[^>]*>\s*(\d+)\s*</button>");
                if (matchesMatch.Success && int.TryParse(matchesMatch.Groups[1].Value, out int matches))
                    leetify.Matches = matches;

                // Banned mates
                var bannedMatesMatch = Regex.Match(html, @"<p class=""text-xs"">BANNED MATES</p>\s*<p class=""text-white"">([\d.]+)%</p>");
                if (bannedMatesMatch.Success && double.TryParse(bannedMatesMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double bannedMates))
                    leetify.BannedMates = bannedMates;

                // Matchmaking ban
                leetify.IsBannedInMatchmaking = Regex.IsMatch(html, @"<p class=""text-red-500"">\s*Banned in matchmaking\s*</p>");
            }

            private static void ParseFaceitProfile(string html, FaceitProfile faceit)
            {
                // Faceit kullanıcı adı
                var faceitUsernameMatch = Regex.Match(html, @"<a href=""https://www\.faceit\.com/[^""]*""[^>]*>\s*([^<]+?)\s*</a>");
                if (faceitUsernameMatch.Success)
                {
                    faceit.Found = true;
                    faceit.Username = faceitUsernameMatch.Groups[1].Value.Trim();
                }
                else
                {
                    faceit.Found = false;
                    faceit.ErrorMessage = "Faceit profili bulunamadı";
                    return;
                }

                // Kayıt tarihi
                var faceitRegMatch = Regex.Match(html, @"<p class=""text-xs"">REGISTERED</p>\s*<p class=""text-white""><span[^>]*data-tip=""[^""]*"">\s*<time datetime=""([^""]+)"">");
                if (faceitRegMatch.Success && DateTime.TryParse(faceitRegMatch.Groups[1].Value, out DateTime regDate))
                    faceit.RegisterDate = regDate;

                // Ülke
                var countryMatch = Regex.Match(html, @"<img[^>]*src=""https://flagsapi\.com/([^/]*)/");
                if (countryMatch.Success) faceit.Country = countryMatch.Groups[1].Value;

                // Dil
                var languageMatch = Regex.Match(html, @"<span class=""tooltip"" data-tip=""Language"">\s*<span class=""text-white"">([^<]+)</span>");
                if (languageMatch.Success) faceit.Language = languageMatch.Groups[1].Value.Trim();

                // CSGO ve CS2 istatistikleri
                ParseFaceitGameStats(html, "csgo", faceit.CSGO);
                ParseFaceitGameStats(html, "cs2", faceit.CS2);
            }

            private static void ParseFaceitGameStats(string html, string gameType, FaceitGameStats stats)
            {
                var tabPattern = $@"<input[^>]*aria-label=""{gameType}""[^>]*>\s*<div role=""tabpanel""[^>]*>(.*?)</div>\s*</div>";
                var tabMatch = Regex.Match(html, tabPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (!tabMatch.Success) return;

                string tabContent = tabMatch.Groups[1].Value;

                // ELO ve Level
                var eloMatch = Regex.Match(tabContent, @"<p class=""text-xs"">ELO</p>\s*<p class=""text-white"">\s*(\d+)\s*<img src=""[^""]*faceit_levels/(\d+)\.svg""");
                if (eloMatch.Success)
                {
                    if (int.TryParse(eloMatch.Groups[1].Value, out int elo)) stats.Elo = elo;
                    if (int.TryParse(eloMatch.Groups[2].Value, out int level)) stats.Level = level;
                }

                // Diğer istatistikler
                var winrateMatch = Regex.Match(tabContent, @"<p class=""text-xs"">WINRATE</p>\s*<p class=""text-white"">([\d.]+)%</p>");
                if (winrateMatch.Success && double.TryParse(winrateMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double winrate))
                    stats.Winrate = winrate;

                var matchesMatch = Regex.Match(tabContent, @"<p class=""text-xs"">MATCHES</p>\s*<p class=""text-white"">(\d+)</p>");
                if (matchesMatch.Success && int.TryParse(matchesMatch.Groups[1].Value, out int matches))
                    stats.Matches = matches;

                var hsMatch = Regex.Match(tabContent, @"<p class=""text-xs"">HS%</p>\s*<p class=""text-white"">(\d+)%</p>");
                if (hsMatch.Success && int.TryParse(hsMatch.Groups[1].Value, out int hs))
                    stats.HeadshotPercent = hs;

                var kdMatch = Regex.Match(tabContent, @"<p class=""text-xs"">KD</p>\s*<p class=""text-white"">([\d.]+)</p>");
                if (kdMatch.Success && double.TryParse(kdMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double kd))
                    stats.KD = kd;

                var adrMatch = Regex.Match(tabContent, @"<p class=""text-xs"">ADR</p>\s*<p class=""text-white"">([\d.]+)</p>");
                if (adrMatch.Success && double.TryParse(adrMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double adr))
                    stats.ADR = adr;

                var clutch1v1Match = Regex.Match(tabContent, @"<p class=""text-xs"">CLUTCH 1V1</p>\s*<p class=""text-white"">(\d+)%</p>");
                if (clutch1v1Match.Success && int.TryParse(clutch1v1Match.Groups[1].Value, out int clutch1v1))
                    stats.Clutch1v1 = clutch1v1;

                var clutch1v2Match = Regex.Match(tabContent, @"<p class=""text-xs"">CLUTCH 1V2</p>\s*<p class=""text-white"">(\d+)%</p>");
                if (clutch1v2Match.Success && int.TryParse(clutch1v2Match.Groups[1].Value, out int clutch1v2))
                    stats.Clutch1v2 = clutch1v2;

                var utilDmgMatch = Regex.Match(tabContent, @"<p class=""text-xs""><span[^>]*>UTIL DMG</span></p>\s*<p class=""text-white"">([\d.]+)</p>");
                if (utilDmgMatch.Success && double.TryParse(utilDmgMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double utilDmg))
                    stats.UtilDamage = utilDmg;

                var lastMatchMatch = Regex.Match(tabContent, @"<p class=""text-xs"">LAST MATCH</p>\s*<p class=""text-white""><span[^>]*>\s*<time datetime=""([^""]+)"">");
                if (lastMatchMatch.Success && DateTime.TryParse(lastMatchMatch.Groups[1].Value, out DateTime lastMatch))
                    stats.LastMatch = lastMatch;

                var recentResultsMatch = Regex.Match(tabContent, @"<p class=""text-xs"">RECENT</p>\s*<span>(.*?)</span>", RegexOptions.Singleline);
                if (recentResultsMatch.Success)
                {
                    var resultMatches = Regex.Matches(recentResultsMatch.Groups[1].Value, @"<span class=""text-(green|red)-400"">([WL])</span>");
                    stats.RecentResults = string.Join("", resultMatches.Cast<Match>().Select(m => m.Groups[2].Value));
                }
            }

            private static void ParseWeaponStats(string html, List<WeaponStat> weapons)
            {
                // Birinci format - tooltip ile
                var weaponMatches = Regex.Matches(html, @"<span class=""tooltip[^""]*"" data-tip=""(\d+) kills(?:, ([\d.]+)% acc)?""[^>]*>\s*<img src=""([^""]*/([\w]+)\.svg)""");
                foreach (Match match in weaponMatches)
                {
                    var weapon = new WeaponStat
                    {
                        WeaponName = match.Groups[4].Value,
                        Kills = int.Parse(match.Groups[1].Value),
                        ImagePath = match.Groups[3].Value
                    };

                    if (!string.IsNullOrEmpty(match.Groups[2].Value) &&
                        double.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double accuracy))
                        weapon.Accuracy = accuracy;

                    weapons.Add(weapon);
                }

                // Eğer silah bulunamadıysa alternatif format dene
                if (weapons.Count == 0)
                {
                    var altWeaponMatches = Regex.Matches(html, @"data-tip=""[^""]*(\d+)[^""]*kills[^""]*""[^>]*>\s*<img[^>]*src=""[^""]*equipment[^""]*/([\w]+)\.svg""", RegexOptions.IgnoreCase);
                    foreach (Match match in altWeaponMatches)
                    {
                        weapons.Add(new WeaponStat
                        {
                            WeaponName = match.Groups[2].Value,
                            Kills = int.Parse(match.Groups[1].Value),
                            ImagePath = ""
                        });
                    }
                }
            }

            private static void ParseCollectibles(string html, List<CollectibleItem> collectibles)
            {
                var collectibleMatches = Regex.Matches(html, @"<img src=""([^""]*)"" alt=""([^""]*)"" title=""[^""]*"" class=""h-7 inline-block[^""]*""[^>]*>");
                foreach (Match match in collectibleMatches)
                {
                    collectibles.Add(new CollectibleItem
                    {
                        Name = match.Groups[2].Value,
                        ImagePath = match.Groups[1].Value
                    });
                }
            }

            public static void DisplayStats(PlayerStats stats)
            {
                Console.WriteLine("=== CS2 OYUNCU İSTATİSTİKLERİ ===");

                // Steam bilgileri
                Console.WriteLine($"\n[STEAM PROFİLİ]");
                Console.WriteLine($"Kullanıcı Adı: {stats.Steam.Username}");
                Console.WriteLine($"Steam Seviyesi: {stats.Steam.SteamLevel}");
                Console.WriteLine($"Vanity URL: {stats.Steam.VanityUrl}");
                Console.WriteLine($"Profil URL: {stats.Steam.ProfileUrl}");
                Console.WriteLine($"Kayıt Tarihi: {stats.Steam.RegisterDate:yyyy-MM-dd}");
                Console.WriteLine($"Arkadaşlar: {stats.Steam.Friends:N0}");
                Console.WriteLine($"Toplam Oyunlar: {stats.Steam.TotalGames:N0}");
                Console.WriteLine($"XP Seviyesi: {stats.Steam.XpLevel}");

                if (!string.IsNullOrEmpty(stats.Steam.GeneralTotalPlaytime))
                    Console.WriteLine($"Genel Oyun Süresi: {stats.Steam.GeneralTotalPlaytime} / {stats.Steam.GeneralRecentPlaytime}");

                Console.WriteLine($"CS2 Oyun Süresi: {stats.Steam.TotalPlaytime} / {stats.Steam.RecentPlaytime}");

                var commendStatus = stats.Steam.HasHighCommendations ? " (YÜKSEK SAYILAR!)" : "";
                Console.WriteLine($"Beğeniler - Arkadaş Canlısı: {stats.Steam.FriendlyCommendations:N0}, Lider: {stats.Steam.LeaderCommendations:N0}, Öğretmen: {stats.Steam.TeacherCommendations:N0}{commendStatus}");

                if (stats.Steam.HasGameBans)
                    Console.WriteLine($"⚠️  OYUN YASAKLARI: {stats.Steam.GameBanCount} - {stats.Steam.LastBanText}");

                // Leetify bilgileri
                Console.WriteLine($"\n[LEETİFY PROFİLİ]");
                Console.WriteLine($"Kullanıcı Adı: {stats.Leetify.Username}{(stats.Leetify.IsLeetifyUser ? " ✓ (Leetify Kullanıcısı)" : "")}");
                Console.WriteLine($"Nişan Alma: {stats.Leetify.Aim}");
                Console.WriteLine($"Yardımcı: {stats.Leetify.Utility}");
                Console.WriteLine($"Pozisyon: {stats.Leetify.Position}");
                Console.WriteLine($"Clutch: {stats.Leetify.Clutch:+0.0;-0.0;0.0}");
                Console.WriteLine($"Açılış: {stats.Leetify.Opening:+0.0;-0.0;0.0}");
                Console.WriteLine($"K/D: {stats.Leetify.KD}");
                Console.WriteLine($"Rating: {stats.Leetify.Rating:+0.0;-0.0;0.0}");
                Console.WriteLine($"Parti: {stats.Leetify.Party}");
                Console.WriteLine($"Ön-nişan: {stats.Leetify.Preaim}° {(stats.Leetify.HasSuspiciousPreaim ? "(ŞÜPHELİ)" : "")}");
                Console.WriteLine($"Hasara Kadar Süre: {stats.Leetify.TimeToDamage}ms {(stats.Leetify.HasSuspiciousTTD ? "(ŞÜPHELİ)" : "")}");
                Console.WriteLine($"HE Hasarı - Düşman: {stats.Leetify.AvgHeDamageEnemies}, Takım: {stats.Leetify.AvgHeDamageTeammates}");

                if (!string.IsNullOrEmpty(stats.Leetify.MaxRating))
                    Console.WriteLine($"Max Rating: {stats.Leetify.MaxRating}");

                Console.WriteLine($"Kazanma Oranı: {stats.Leetify.WinrateAllTime}% / {stats.Leetify.WinrateRecent}%");
                Console.WriteLine($"Maçlar: {stats.Leetify.Matches}");
                Console.WriteLine($"Yasaklı Takım Arkadaşları: {stats.Leetify.BannedMates}%");

                if (stats.Leetify.IsBannedInMatchmaking)
                    Console.WriteLine($"⚠️  MATCHMAKİNG'DE YASAKLI");

                // Faceit bilgileri
                Console.WriteLine($"\n[FACEİT PROFİLİ]");
                Console.WriteLine($"Bulundu: {stats.Faceit.Found}");
                if (!stats.Faceit.Found)
                {
                    Console.WriteLine($"Durum: {stats.Faceit.ErrorMessage}");
                }
                else
                {
                    Console.WriteLine($"Kullanıcı Adı: {stats.Faceit.Username}");
                    if (!string.IsNullOrEmpty(stats.Faceit.Country) || !string.IsNullOrEmpty(stats.Faceit.Language))
                        Console.WriteLine($"Ülke: {stats.Faceit.Country} | Dil: {stats.Faceit.Language}");
                    if (stats.Faceit.RegisterDate != default(DateTime))
                        Console.WriteLine($"Kayıt: {stats.Faceit.RegisterDate:yyyy-MM-dd}");

                    if (stats.Faceit.CSGO.Matches > 0)
                    {
                        Console.WriteLine($"\n  [CSGO İstatistikleri]");
                        Console.WriteLine($"  ELO: {stats.Faceit.CSGO.Elo} (Seviye {stats.Faceit.CSGO.Level})");
                        Console.WriteLine($"  Kazanma Oranı: {stats.Faceit.CSGO.Winrate:F1}% | Maçlar: {stats.Faceit.CSGO.Matches}");
                        Console.WriteLine($"  HS%: {stats.Faceit.CSGO.HeadshotPercent}% | K/D: {stats.Faceit.CSGO.KD:F2}");
                    }

                    if (stats.Faceit.CS2.Matches > 0)
                    {
                        Console.WriteLine($"\n  [CS2 İstatistikleri]");
                        Console.WriteLine($"  ELO: {stats.Faceit.CS2.Elo} (Seviye {stats.Faceit.CS2.Level})");
                        Console.WriteLine($"  Kazanma Oranı: {stats.Faceit.CS2.Winrate:F1}% | Maçlar: {stats.Faceit.CS2.Matches}");
                        Console.WriteLine($"  HS%: {stats.Faceit.CS2.HeadshotPercent}% | K/D: {stats.Faceit.CS2.KD:F2} | ADR: {stats.Faceit.CS2.ADR:F1}");
                        if (stats.Faceit.CS2.Clutch1v1 > 0 || stats.Faceit.CS2.Clutch1v2 > 0)
                            Console.WriteLine($"  Clutch 1v1: {stats.Faceit.CS2.Clutch1v1}% | 1v2: {stats.Faceit.CS2.Clutch1v2}%");
                        if (stats.Faceit.CS2.UtilDamage > 0)
                            Console.WriteLine($"  Yardımcı Hasarı: {stats.Faceit.CS2.UtilDamage:F1}");
                        if (!string.IsNullOrEmpty(stats.Faceit.CS2.RecentResults))
                            Console.WriteLine($"  Son Sonuçlar: {stats.Faceit.CS2.RecentResults}");
                        if (stats.Faceit.CS2.LastMatch != default(DateTime))
                            Console.WriteLine($"  Son Maç: {stats.Faceit.CS2.LastMatch:yyyy-MM-dd}");
                    }

                    if (stats.Faceit.CSGO.Matches == 0 && stats.Faceit.CS2.Matches == 0)
                        Console.WriteLine($"  Oyun istatistiği mevcut değil");
                }

                // En çok kullanılan silahlar
                Console.WriteLine($"\n[EN ÇOK KULLANILAN SİLAHLAR] ({stats.Weapons.Count} toplam)");
                var topWeapons = stats.Weapons.OrderByDescending(w => w.Kills).ToList();
                foreach (var weapon in topWeapons)
                {
                    string accuracyText = weapon.Accuracy.HasValue ? $", {weapon.Accuracy:F1}% isabet" : "";
                    Console.WriteLine($"{weapon.WeaponName}: {weapon.Kills:N0} leş{accuracyText}");
                }

                // Koleksiyon eşyaları
                Console.WriteLine($"\n[KOLEKSİYON EŞYALARI] ({stats.Collectibles.Count} eşya)");
                foreach (var item in stats.Collectibles)
                    Console.WriteLine($"- {item.Name}");

                Console.WriteLine("\n=== AYRIŞDIRMA TAMAMLANDI ===");
            }
        }

        public static class UrlHelper
        {
            public static string GetSteamUrl(string url)
            {
                var customMatch = Regex.Match(url, @"/id/([^/]+)");
                if (customMatch.Success)
                {
                    Console.WriteLine($"Tip: Özel ID, Değer: {customMatch.Groups[1].Value}");
                    return $"https://steamcommunity.rip/id/{customMatch.Groups[1].Value}/";
                }

                var steamIdMatch = Regex.Match(url, @"/profiles/(\d+)");
                if (steamIdMatch.Success)
                {
                    Console.WriteLine($"Tip: SteamID64, Değer: {steamIdMatch.Groups[1].Value}");
                    return $"https://steamcommunity.rip/profiles/{steamIdMatch.Groups[1].Value}/";
                }

                throw new ArgumentException("Geçersiz Steam URL");
            }
        }
    }
}