using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace IdleKingdom
{
    public partial class MainWindow : Window
    {
        private int gold = 0;
        private int wood = 0;
        private int population = 5;

        private int goldPerSecond = 1;
        private int woodPerSecond = 0;

        private int villageLevel = 1;

        // Bezoekers
        private bool visitorAvailable = true;
        private int visitorCooldown = 0; // seconds remaining

        private DispatcherTimer idleTimer;

        public MainWindow()
        {
            InitializeComponent();

            idleTimer = new DispatcherTimer();
            idleTimer.Interval = TimeSpan.FromSeconds(1);
            idleTimer.Tick += IdleTimer_Tick;
            idleTimer.Start();

            UpdateUI();
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            // Idle income
            gold += goldPerSecond;
            wood += woodPerSecond;

            // Bezoeker cooldown & live countdown
            if (!visitorAvailable)
            {
                visitorCooldown--;

                if (visitorCooldown <= 0)
                {
                    visitorAvailable = true;

                    EventText.Text = "Een nieuwe bezoeker is aangekomen!";

                    YesButton.IsEnabled = true;
                    NoButton.IsEnabled = true;
                }
                else
                {
                    var ts = TimeSpan.FromSeconds(visitorCooldown);
                    EventText.Text = $"Geen bezoekers op dit moment.\n\nNieuwe bezoeker over {ts:mm\\:ss}";
                }
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            GoldText.Text = gold.ToString();
            WoodText.Text = wood.ToString();
            PopulationText.Text = population.ToString();

            IncomeText.Text =
                $"+{goldPerSecond} goud / seconde   |   +{woodPerSecond} hout / seconde";

            int cost = UpgradeCost();
            int requiredPopulation = RequiredPopulation();

            UpgradeButton.Content =
                $"Upgrade ({cost} goud, {requiredPopulation} bewoners)";

            // Upgrade alleen beschikbaar als beide voorwaarden gehaald zijn
            UpgradeButton.IsEnabled =
                gold >= cost &&
                population >= requiredPopulation;
        }

        // Cost to perform the upgrade (kept as current level * 50 so first upgrade costs 50)
        private int UpgradeCost()
        {
            return villageLevel * 50;
        }

        // Required population to reach the next village level
        private int RequiredPopulation()
        {
            int nextLevel = villageLevel + 1;

            // Next-level requirements:
            // Level 2 (Gehucht)  -> 20 bewoners
            // Level 3 (Stad)     -> 50 bewoners
            // Level 4 (Koninkrijk)-> 100 bewoners
            // Level 5 (Groot rijk)-> 200 bewoners
            // Level 6+           -> 300+ bewoners
            return nextLevel switch
            {
                2 => 20,
                3 => 50,
                4 => 100,
                5 => 200,
                6 => 300,
                _ => 500
            };
        }

        private void UpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            int cost = UpgradeCost();
            int requiredPopulation = RequiredPopulation();

            if (gold < cost)
            {
                MessageBox.Show($"Je hebt {cost} goud nodig.");
                return;
            }

            if (population < requiredPopulation)
            {
                MessageBox.Show($"Je hebt {requiredPopulation} bewoners nodig.");
                return;
            }

            // Betaal upgrade
            gold -= cost;

            villageLevel++;

            // Upgrade inkomsten
            goldPerSecond += 2;
            woodPerSecond += 1;

            // Nieuwe bewoners
            population += 2;

            VillageNameText.Text = GetVillageName();

            UpdateUI();
        }

        private string GetVillageName()
        {
            return villageLevel switch
            {
                1 => "Je dorp",
                2 => "Je gehucht",
                3 => "Je stad",
                4 => "Je koninkrijk",
                5 => "Je grote rijk",
                _ => "Je enorme rijk"
            };
        }

        private void StartVisitorCooldown()
        {
            visitorAvailable = false;

            // 2 minuten = 120 seconden
            visitorCooldown = 120;

            YesButton.IsEnabled = false;
            NoButton.IsEnabled = false;

            var ts = TimeSpan.FromSeconds(visitorCooldown);
            EventText.Text = $"Geen bezoekers op dit moment.\n\nNieuwe bezoeker over {ts:mm\\:ss}";
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!visitorAvailable)
                return;

            gold += 25;
            population++;

            // Inform player and start cooldown with live countdown
            EventText.Text = "Je hebt de bezoeker geaccepteerd!\n\nNieuwe bezoeker over 2:00";

            StartVisitorCooldown();

            UpdateUI();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!visitorAvailable)
                return;

            gold += 5;

            // Inform player and start cooldown with live countdown
            EventText.Text = "Je hebt de bezoeker geweigerd.\n\nNieuwe bezoeker over 2:00";

            StartVisitorCooldown();

            UpdateUI();
        }
    }
}