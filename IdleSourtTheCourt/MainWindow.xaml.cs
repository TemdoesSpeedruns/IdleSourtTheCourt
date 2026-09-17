using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SourtTheCourtIdle
{
    public partial class MainWindow : Window
    {
        private GameData game;

        private readonly DispatcherTimer gameTimer;
        private readonly DispatcherTimer visitorTimer;
        private readonly DispatcherTimer populationTimer;

        private readonly Random random = new Random();

        private double visitorTimeRemaining;

        private const string SaveFile = "save.json";


        public MainWindow()
        {
            InitializeComponent();

            game = LoadGame();

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(100);
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            visitorTimer = new DispatcherTimer();
            visitorTimer.Interval = TimeSpan.FromSeconds(1);
            visitorTimer.Tick += VisitorTimer_Tick;

            populationTimer = new DispatcherTimer();
            populationTimer.Interval = TimeSpan.FromSeconds(10);
            populationTimer.Tick += PopulationTimer_Tick;
            populationTimer.Start();

            UpdateUI();

            StartInitialCooldown();
        }


        // ==========================================
        // GAME LOOP
        // ==========================================

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            game.Gold += game.IncomePerSecond / 10;

            UpdateUI();
        }


        // ==========================================
        // PASSIVE POPULATION GROWTH
        // ==========================================

        private void PopulationTimer_Tick(object? sender, EventArgs e)
        {
            int populationChange = GetPopulationGrowth();

            if (populationChange < 0 && game.Happiness >= 50)
            {
                populationChange = 0;
            }

            if (populationChange != 0)
            {
                game.Population += populationChange;

                if (game.Population < 0)
                    game.Population = 0;

                CheckKingdomUpgrade();

                if (populationChange > 0)
                {
                    StatusText.Text =
                        $"{populationChange} new citizen(s) joined because the kingdom is happy.";
                }
                else
                {
                    StatusText.Text =
                        $"{Math.Abs(populationChange)} citizen(s) left because the kingdom is unhappy.";
                }
            }

            UpdateUI();
        }


        private int GetPopulationGrowth()
        {
            if (game.Happiness >= 90)
                return 3;

            if (game.Happiness >= 75)
                return 2;

            if (game.Happiness >= 60)
                return 1;

            if (game.Happiness >= 50)
                return 0;

            if (game.Happiness >= 40)
                return -1;

            if (game.Happiness >= 20)
                return -5;

            return -10;
        }


        // ==========================================
        // VISITOR TIMER
        // ==========================================

        private void VisitorTimer_Tick(object? sender, EventArgs e)
        {
            visitorTimeRemaining--;

            UpdateTimer();

            if (visitorTimeRemaining <= 0)
            {
                visitorTimer.Stop();

                WaitingPanel.Visibility = Visibility.Collapsed;
                VisitorPanel.Visibility = Visibility.Visible;

                ShowVisitor();
            }
        }


        private void StartVisitorCooldown()
        {
            VisitorPanel.Visibility = Visibility.Collapsed;
            WaitingPanel.Visibility = Visibility.Visible;

            visitorTimeRemaining = random.Next(15, 31);

            UpdateTimer();

            visitorTimer.Start();
        }


        private void UpdateTimer()
        {
            int seconds = (int)Math.Max(0, visitorTimeRemaining);

            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;

            TimerText.Text = $"{minutes:00}:{remainingSeconds:00}";
        }


        private void StartInitialCooldown()
        {
            VisitorPanel.Visibility = Visibility.Collapsed;
            WaitingPanel.Visibility = Visibility.Visible;

            visitorTimeRemaining = 15;

            UpdateTimer();

            visitorTimer.Start();
        }


        // ==========================================
        // VISITORS
        // ==========================================

        private Visitor currentVisitor = new Visitor();


        private void ShowVisitor()
        {
            currentVisitor = GetRandomVisitor();

            VisitorNameText.Text = currentVisitor.Name;
            VisitorRequestText.Text = currentVisitor.Request;

            StatusText.Text = "A visitor has arrived.";

            YesButton.IsEnabled = true;
            NoButton.IsEnabled = true;
        }


        private Visitor GetRandomVisitor()
        {
            Visitor[] visitors =
            {
                new Visitor
                {
                    Name = "A Farmer",
                    Request = "The farmers want to hold a harvest festival. Will you support them?",
                    YesGold = -50,
                    YesPopulation = 0,
                    YesHappiness = 10,

                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = -5
                },

                new Visitor
                {
                    Name = "A Merchant",
                    Request = "May I organize a large market day for the people?",
                    YesGold = -100,
                    YesPopulation = 0,
                    YesHappiness = 12,

                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = -3
                },

                new Visitor
                {
                    Name = "A Family",
                    Request = "Our family wishes to settle in your kingdom. May we stay?",
                    YesGold = 0,
                    YesPopulation = 5,
                    YesHappiness = 3,

                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = -4
                },

                new Visitor
                {
                    Name = "A Guard Captain",
                    Request = "The people feel unsafe. Shall we hire more guards?",
                    YesGold = -200,
                    YesPopulation = 0,
                    YesHappiness = 15,

                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = -10
                },

                new Visitor
                {
                    Name = "A Bard",
                    Request = "May I perform music in the town square?",
                    YesGold = -25,
                    YesPopulation = 0,
                    YesHappiness = 8,

                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = -2
                },

                new Visitor
                {
                    Name = "A Builder",
                    Request = "The villagers want a new public well. Should we build one?",
                    YesGold = -150,
                    YesPopulation = 0,
                    YesHappiness = 18,

                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = -8
                },

                new Visitor
                {
                    Name = "A Tax Collector",
                    Request = "Shall we increase taxes to fill the royal treasury?",
                    YesGold = 250,
                    YesPopulation = 0,
                    YesHappiness = -15,

                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = 5
                },

                new Visitor
                {
                    Name = "A Strange Man",
                    Request = "Give me access to the royal treasury. I promise nothing bad will happen.",
                    YesGold = 150,
                    YesPopulation = -2,
                    YesHappiness = -15,

                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = 3
                },

                new Visitor
                {
                    Name = "A Healer",
                    Request = "The people need medicine. Shall we pay for supplies?",
                    YesGold = -120,
                    YesPopulation = 1,
                    YesHappiness = 14,

                    NoGold = 0,
                    NoPopulation = -1,
                    NoHappiness = -12
                },

                new Visitor
                {
                    Name = "A Traveller",
                    Request = "May travellers enter the kingdom freely?",
                    YesGold = 0,
                    YesPopulation = 3,
                    YesHappiness = 5,

                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = -3
                },

                new Visitor
                {
                    Name = "A Thief",
                    Request = "I can sneak into noble estates and bring you some coin. Shall I steal for your crown?",
                    YesGold = 200,
                    YesPopulation = 0,
                    YesHappiness = -8,
                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = 3
                },

                new Visitor
                {
                    Name = "A Devil",
                    Request = "I offer great riches in exchange for... sacrifices. Take my deal?",
                    YesGold = 1000,
                    YesPopulation = -50,
                    YesHappiness = -50,
                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = 5
                },

                new Visitor
                {
                    Name = "A Cat",
                    Request = "A cat demands a belly rub. Will you indulge this royal feline?",
                    YesGold = 0,
                    YesPopulation = 0,
                    YesHappiness = 1,
                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = -1
                },
                new Visitor {
                    Name = "A Knight",
                    Request = "I wish to join your court and protect you with my life, would you allow me to?",
                    YesGold = -100,
                    YesPopulation = 1,
                    YesHappiness = 1,
                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = -2
                },

                new Visitor
                {
                    Name = "Some Settlers",
                    Request = "We heard your kingdom is growing. Can we join?",
                    YesGold = 0,
                    YesPopulation = 5,
                    YesHappiness = 5,

                    NoGold = 0,
                    NoPopulation = 0,
                    NoHappiness = -5
                }
            };

            List<Visitor> pool = visitors.ToList();

            if (game.Gold < 100)
            {
                pool = pool.Where(v => !(v.YesGold < 0 || v.NoGold < 0)).ToList();
            }

            
            if (pool.Count == 0)
                pool = visitors.ToList();

            return pool[random.Next(pool.Count)];
        }


        // ==========================================
        // YES / NO
        // ==========================================

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CanAfford(currentVisitor.YesGold))
            {
                StatusText.Text = "You cannot afford this decision.";
                return;
            }

            game.Gold += currentVisitor.YesGold;
            game.Population += currentVisitor.YesPopulation;
            game.Happiness += currentVisitor.YesHappiness;

            ClampValues();

            StatusText.Text = BuildDecisionResultText(
                currentVisitor.YesGold,
                currentVisitor.YesPopulation,
                currentVisitor.YesHappiness);

            CheckKingdomUpgrade();

            AutoSave();

            StartVisitorCooldown();
        }


        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CanAfford(currentVisitor.NoGold))
            {
                StatusText.Text = "You cannot afford this decision.";
                return;
            }

            game.Gold += currentVisitor.NoGold;
            game.Population += currentVisitor.NoPopulation;
            game.Happiness += currentVisitor.NoHappiness;

            ClampValues();

            StatusText.Text = BuildDecisionResultText(
                currentVisitor.NoGold,
                currentVisitor.NoPopulation,
                currentVisitor.NoHappiness);

            CheckKingdomUpgrade();

            AutoSave();

            StartVisitorCooldown();
        }


        private bool CanAfford(double amount)
        {
            return amount >= 0 || game.Gold >= Math.Abs(amount);
        }


        private void ClampValues()
        {
            if (game.Population < 0)
                game.Population = 0;

            game.Happiness =
                Math.Clamp(game.Happiness, 0, 100);
        }


        private string BuildDecisionResultText(
            double gold,
            int population,
            int happiness
        )
        {
            string result = "Decision result:";

            if (gold > 0)
                result += $" +{FormatNumber(gold)} gold";

            if (gold < 0)
                result += $" -{FormatNumber(Math.Abs(gold))} gold";

            if (population > 0)
                result += $" +{population} population";

            if (population < 0)
                result += $" {population} population";

            if (happiness > 0)
                result += $" +{happiness}% happiness";

            if (happiness < 0)
                result += $" {happiness}% happiness";

            if (gold == 0 &&
                population == 0 &&
                happiness == 0)
            {
                result += " nothing changed";
            }

            return result;
        }


        // ==========================================
        // UPGRADES
        // ==========================================

        private void FarmButton_Click(object sender, RoutedEventArgs e)
        {
            double price = GetFarmPrice();

            if (game.Population < 10)
                return;

            if (game.Gold < price)
                return;

            game.Gold -= price;

            game.Farms++;

            game.IncomePerSecond += 1;

            StatusText.Text = "You built a new farm.";

            UpdateUI();

            AutoSave();
        }


        private void MarketButton_Click(object sender, RoutedEventArgs e)
        {
            double price = GetMarketPrice();

            if (game.Population < 25)
                return;

            if (game.Gold < price)
                return;

            game.Gold -= price;

            game.Markets++;

            game.IncomePerSecond += 5;

            StatusText.Text = "You built a market.";

            UpdateUI();

            AutoSave();
        }


        private void MineButton_Click(object sender, RoutedEventArgs e)
        {
            double price = GetMinePrice();

            if (game.Population < 75)
                return;

            if (game.Gold < price)
                return;

            game.Gold -= price;

            game.Mines++;

            game.IncomePerSecond += 15;

            StatusText.Text = "You opened a mine.";

            UpdateUI();

            AutoSave();
        }


        private double GetFarmPrice()
        {
            return 50 * Math.Pow(1.15, game.Farms);
        }


        private double GetMarketPrice()
        {
            return 500 * Math.Pow(1.25, game.Markets);
        }


        private double GetMinePrice()
        {
            return 5000 * Math.Pow(1.35, game.Mines);
        }


        // ==========================================
        // KINGDOM LEVEL
        // ==========================================

        private void CheckKingdomUpgrade()
        {
            string oldLevel = game.KingdomLevel;

            if (game.Population >= 1000)
            {
                game.KingdomLevel = "Kingdom";
            }
            else if (game.Population >= 400)
            {
                game.KingdomLevel = "City";
            }
            else if (game.Population >= 100)
            {
                game.KingdomLevel = "Town";
            }
            else if (game.Population >= 50)
            {
                game.KingdomLevel = "Village";
            }
            else
            {
                game.KingdomLevel = "Hamlet";
            }

            if (oldLevel != game.KingdomLevel)
            {
                StatusText.Text =
                    $"Your kingdom has grown into a {game.KingdomLevel}!";

                ChangeBackground();
            }
        }


        private void ChangeBackground()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string imgDir = Path.Combine(baseDir, "img");

                string level = (game.KingdomLevel ?? "Hamlet").ToLowerInvariant();
                string imageName = level + ".png";
                string imagePath = Path.Combine(imgDir, imageName);

                if (File.Exists(imagePath))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(imagePath, UriKind.Absolute);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze();

                    KingdomBackground.Background = new ImageBrush(image) { Stretch = Stretch.UniformToFill };
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChangeBackground] Exception: {ex}");
            }

            // Fallback solid colour
            switch (game.KingdomLevel)
            {
                case "Hamlet": KingdomBackground.Background = new SolidColorBrush(Color.FromRgb(48, 59, 47)); break;
                case "Village": KingdomBackground.Background = new SolidColorBrush(Color.FromRgb(45, 70, 50)); break;
                case "Town": KingdomBackground.Background = new SolidColorBrush(Color.FromRgb(65, 65, 85)); break;
                case "City": KingdomBackground.Background = new SolidColorBrush(Color.FromRgb(70, 55, 80)); break;
                case "Kingdom": KingdomBackground.Background = new SolidColorBrush(Color.FromRgb(80, 65, 40)); break;
                default: KingdomBackground.Background = new SolidColorBrush(Color.FromRgb(48, 59, 47)); break;
            }
        }


        // ==========================================
        // UI
        // ==========================================

        private void UpdateUI()
        {
            GoldText.Text = FormatNumber(game.Gold);

            IncomeText.Text =
                $"+{FormatNumber(game.IncomePerSecond)} / sec";

            PopulationText.Text =
                game.Population.ToString();

            HappinessText.Text =
                $"{game.Happiness}%";

            RebirthText.Text =
                game.RebirthCount.ToString();

            KingdomNameText.Text = "Kingdom";
            KingdomLevelText.Text = game.KingdomLevel;

            UpdatePopulationProgress();
            UpdateUpgradeButtons();

            ChangeBackground();
        }


        private void UpdatePopulationProgress()
        {
            int nextRequirement =
                GetNextPopulationRequirement();

            PopulationProgress.Maximum =
                nextRequirement;

            PopulationProgress.Value =
                Math.Min(game.Population, nextRequirement);

            PopulationProgressText.Text =
                $"{game.Population} / {nextRequirement} population";
        }


        private int GetNextPopulationRequirement()
        {
            if (game.Population < 50)
                return 50;

            if (game.Population < 200)
                return 200;

            if (game.Population < 450)
                return 450;

            if (game.Population < 1000)
                return 1000;

            return 1000;
        }


        private void UpdateUpgradeButtons()
        {
            double farmPrice = GetFarmPrice();
            double marketPrice = GetMarketPrice();
            double minePrice = GetMinePrice();

            FarmPriceText.Text =
                $"{FormatNumber(farmPrice)} gold";

            MarketPriceText.Text =
                $"{FormatNumber(marketPrice)} gold";

            MinePriceText.Text =
                $"{FormatNumber(minePrice)} gold";

            FarmButton.IsEnabled =
                game.Population >= 10 &&
                game.Gold >= farmPrice;

            MarketButton.IsEnabled =
                game.Population >= 25 &&
                game.Gold >= marketPrice;

            MineButton.IsEnabled =
                game.Population >= 75 &&
                game.Gold >= minePrice;
        }


        private string FormatNumber(double number)
        {
            if (number >= 1_000_000)
                return $"{number / 1_000_000:0.##}M";

            if (number >= 1_000)
                return $"{number / 1_000:0.##}K";

            return $"{number:0.##}";
        }


        // ==========================================
        // SAVE
        // ==========================================

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SaveGame();

            StatusText.Text = "Game saved.";
        }


        private void AutoSave()
        {
            SaveGame();
        }


        private void SaveGame()
        {
            try
            {
                game.LastSave = DateTime.Now;

                string json =
                    JsonSerializer.Serialize(
                        game,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                File.WriteAllText(
                    SaveFile,
                    json);
            }
            catch
            {
                StatusText.Text =
                    "Could not save the game.";
            }
        }


        // ==========================================
        // LOAD
        // ==========================================

        private GameData LoadGame()
        {
            try
            {
                if (!File.Exists(SaveFile))
                    return CreateNewGame();

                string json =
                    File.ReadAllText(SaveFile);

                GameData? loadedGame =
                    JsonSerializer.Deserialize<GameData>(json);

                if (loadedGame == null)
                    return CreateNewGame();

                if (loadedGame.Happiness == 0)
                {
                    loadedGame.Happiness = 50;
                }

                TimeSpan offlineTime =
                    DateTime.Now - loadedGame.LastSave;

                if (offlineTime.TotalSeconds > 0)
                {
                    double secondsAway =
                        Math.Min(
                            offlineTime.TotalSeconds,
                            12 * 60 * 60);

                    loadedGame.Gold +=
                        loadedGame.IncomePerSecond *
                        secondsAway;
                }

                return loadedGame;
            }
            catch
            {
                MessageBox.Show(
                    "The save file could not be loaded. A new game will be started.",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return CreateNewGame();
            }
        }


        private GameData CreateNewGame()
        {
            return new GameData
            {
                Gold = 0,

                Population = 5,

                Happiness = 50,

                IncomePerSecond = 1,

                KingdomLevel = "Hamlet",

                Farms = 0,

                Markets = 0,

                Mines = 0,

                RebirthCount = 0,

                LastSave = DateTime.Now
            };
        }


        protected override void OnClosed(EventArgs e)
        {
            SaveGame();

            base.OnClosed(e);
        }

        private void RebirthButton_Click(object sender, RoutedEventArgs e)
        {
            if (game.Population < 1500)
            {
                StatusText.Text = "You need at least 1500 population to rebirth.";
                return;
            }

            if (MessageBox.Show(
                "Are you sure you want to reset your progress? This cannot be undone.",
                "Confirm Rebirth",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                int rebirthCount = game.RebirthCount + 1;
                
                game = CreateNewGame();
                game.RebirthCount = rebirthCount;
                
                game.IncomePerSecond += rebirthCount * 0.5;
                
                if (File.Exists(SaveFile))
                    File.Delete(SaveFile);
                
                StatusText.Text = $"Your kingdom has been reborn. (Rebirth #{rebirthCount})";
                UpdateUI();
                AutoSave();
            }
        }
    }


    // ==========================================
    // GAME DATA
    // ==========================================

    public class GameData
    {
        public double Gold { get; set; }

        public int Population { get; set; }

        public int Happiness { get; set; }

        public double IncomePerSecond { get; set; }

        public string KingdomLevel { get; set; } =
            "Hamlet";

        public int Farms { get; set; }

        public int Markets { get; set; }

        public int Mines { get; set; }

        public int RebirthCount { get; set; }

        public DateTime LastSave { get; set; }
    }


    // ==========================================
    // VISITOR DATA
    // ==========================================

    public class Visitor
    {
        public string Name { get; set; } = "";

        public string Request { get; set; } = "";

        public double YesGold { get; set; }

        public int YesPopulation { get; set; }

        public int YesHappiness { get; set; }


        public double NoGold { get; set; }

        public int NoPopulation { get; set; }

        public int NoHappiness { get; set; }
    }
}