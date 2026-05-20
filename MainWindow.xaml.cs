using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace SlotMachineWpf;

public partial class MainWindow : Window
{
   
    private const int SymbolsOnReel = 8;

    private const int StartBalance = 100;
    private const int SpinCost = 10;
    private const int BigWinAmount = 50;
    private const int SmallWinAmount = 20;

    private readonly Random _random = new();
    private readonly List<SymbolItem> _baseSymbols =
    [
        new("Cseresznye", "🍒", "cherry.png"),
        new("Citrom", "🍋", "lemon.png"),
        new("Szőlő", "🍇", "grapes.png"),
        new("Görögdinnye", "🍉", "watermelon.png"),
        new("Piros alma", "🍎", "apple.png")
    ];

    private List<SymbolItem> _reelSymbols = [];
    private readonly int[] _reelPositions = new int[3];
    private int _balance = StartBalance;
    private int _spins;
    private int _wins;
    private int _losses;
    private bool _isSpinning;

    public MainWindow()
    {
        InitializeComponent();
        BuildReelSymbolList();
        InitializeRandomPositions();
        UpdateAllReels();
        UpdateUi();
    }

    private void BuildReelSymbolList()
    {
        _reelSymbols = [];

        for (int i = 0; i < SymbolsOnReel; i++)
        {
            _reelSymbols.Add(_baseSymbols[i % _baseSymbols.Count]);
        }
    }

    private void InitializeRandomPositions()
    {
        for (int i = 0; i < _reelPositions.Length; i++)
        {
            _reelPositions[i] = _random.Next(_reelSymbols.Count);
        }
    }

    private async void SpinButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isSpinning)
        {
            return;
        }

        if (_balance < SpinCost)
        {
            ResultText.Text = "Nincs elég kredited a pörgetéshez.";
            ResultText.Foreground = Brushes.OrangeRed;
            return;
        }

        _isSpinning = true;
        SpinButton.IsEnabled = false;
        ResultText.Text = "Pörög...";
        ResultText.Foreground = Brushes.White;

        _balance -= SpinCost;
        _spins++;
        UpdateUi();

        //ahhoz, hogy a 3 tárcsa külön-külön álljon meg, ai segítséget használtam.
        Task reel1 = SpinReelAsync(0, 1400);
        Task reel2 = SpinReelAsync(1, 1900);
        Task reel3 = SpinReelAsync(2, 2400);

        await Task.WhenAll(reel1, reel2, reel3);
        EvaluateMiddleRow();

        UpdateUi();
        SpinButton.IsEnabled = true;
        _isSpinning = false;
    }

    private async Task SpinReelAsync(int reelIndex, int totalDurationMs)
    {
        int elapsed = 0;
        int delay = 60;

        while (elapsed < totalDurationMs)
        {
            _reelPositions[reelIndex] = WrapIndex(_reelPositions[reelIndex] + _random.Next(1, 4));
            UpdateReel(reelIndex);

            await Task.Delay(delay);
            elapsed += delay;

            if (elapsed > totalDurationMs * 0.55)
            {
                delay = Math.Min(delay + 12, 180);
            }
        }
    }

    private void EvaluateMiddleRow()
    {
        SymbolItem a = GetVisibleSymbol(0, 0);
        SymbolItem b = GetVisibleSymbol(1, 0);
        SymbolItem c = GetVisibleSymbol(2, 0);

        if (a.Name == b.Name && b.Name == c.Name)
        {
            _balance += BigWinAmount;
            _wins++;
            ResultText.Text = $"WIN! 3 egyforma: {a.Name} (+{BigWinAmount} kredit)";
            ResultText.Foreground = Brushes.LimeGreen;
            return;
        }

        if (a.Name == b.Name || a.Name == c.Name || b.Name == c.Name)
        {
            _balance += SmallWinAmount;
            _wins++;
            ResultText.Text = $"WIN! 2 egyforma (+{SmallWinAmount} kredit)";
            ResultText.Foreground = Brushes.Gold;
            return;
        }

        _losses++;
        ResultText.Text = "LOSE! Most nem nyertél.";
        ResultText.Foreground = Brushes.OrangeRed;
    }

    private SymbolItem GetVisibleSymbol(int reelIndex, int rowOffset)
    {
        int symbolIndex = WrapIndex(_reelPositions[reelIndex] + rowOffset);
        return _reelSymbols[symbolIndex];
    }

    private int WrapIndex(int index)
    {
        int count = _reelSymbols.Count;
        return ((index % count) + count) % count;
    }

    private void UpdateAllReels()
    {
        UpdateReel(0);
        UpdateReel(1);
        UpdateReel(2);
    }

    private void UpdateReel(int reelIndex)
    {
        SymbolItem top = GetVisibleSymbol(reelIndex, -1);
        SymbolItem center = GetVisibleSymbol(reelIndex, 0);
        SymbolItem bottom = GetVisibleSymbol(reelIndex, 1);

        switch (reelIndex)
        {
            case 0:
                SetSymbolVisual(Reel1TopImage, Reel1TopFallback, top);
                SetSymbolVisual(Reel1CenterImage, Reel1CenterFallback, center);
                SetSymbolVisual(Reel1BottomImage, Reel1BottomFallback, bottom);
                break;
            case 1:
                SetSymbolVisual(Reel2TopImage, Reel2TopFallback, top);
                SetSymbolVisual(Reel2CenterImage, Reel2CenterFallback, center);
                SetSymbolVisual(Reel2BottomImage, Reel2BottomFallback, bottom);
                break;
            case 2:
                SetSymbolVisual(Reel3TopImage, Reel3TopFallback, top);
                SetSymbolVisual(Reel3CenterImage, Reel3CenterFallback, center);
                SetSymbolVisual(Reel3BottomImage, Reel3BottomFallback, bottom);
                break;
        }
    }

    //Voltak bajok a képek megjelenítésével, szóval AI segítséggel bombabiztosra tettem, hogy megjelenjen a kép/szimbólum

    private void SetSymbolVisual(Image image, TextBlock fallback, SymbolItem symbol)
    {
        BitmapImage? bitmap = TryLoadImage(symbol.FileName);

        if (bitmap is not null)
        {
            image.Source = bitmap;
            image.Visibility = Visibility.Visible;
            fallback.Visibility = Visibility.Collapsed;
            return;
        }

        image.Source = null;
        image.Visibility = Visibility.Collapsed;
        fallback.Text = symbol.Emoji;
        fallback.Visibility = Visibility.Visible;
    }

    private BitmapImage? TryLoadImage(string fileName)
    {
        try
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(appDir, "Images", fileName);

            if (!File.Exists(fullPath))
            {
                return null;
            }

            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    private void UpdateUi()
    {
        BalanceText.Text = _balance.ToString();
        SpinCostText.Text = SpinCost.ToString();
        BigWinText.Text = BigWinAmount.ToString();
        SmallWinText.Text = SmallWinAmount.ToString();
        StatsText.Text = $"Pörgetések: {_spins} | Győzelem: {_wins} | Vereség: {_losses}";
    }

    private sealed record SymbolItem(string Name, string Emoji, string FileName);
}
