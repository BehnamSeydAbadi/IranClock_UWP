using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IranClock_UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer _clockTimer;
        private int _lastTileUpdateMinute = -1;

        public MainPage()
        {
            this.InitializeComponent();

            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += ClockTimer_Tick;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UpdateClock();
            _clockTimer.Start();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _clockTimer.Stop();
        }

        private void ClockTimer_Tick(object sender, object e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            var now = DateTimeOffset.Now;
            var utcNow = DateTimeOffset.UtcNow;
            var zone = TimeZoneInfo.Local;

            PhoneTimeTextBlock.Text = now.ToString("HH:mm:ss");
            UtcTimeTextBlock.Text = utcNow.ToString("HH:mm:ss");
            TimeZoneTextBlock.Text = zone.DisplayName;

            var noDstNow = utcNow.ToOffset(zone.BaseUtcOffset);
            SuggestedTimeTextBlock.Text = noDstNow.ToString("HH:mm:ss");

            // Update Live Tile once per minute
            if (noDstNow.Minute != _lastTileUpdateMinute)
            {
                _lastTileUpdateMinute = noDstNow.Minute;
                UpdateLiveTile(noDstNow, zone);
            }
        }

        private void UpdateLiveTile(DateTimeOffset noDstNow, TimeZoneInfo zone)
        {
            var correctedTime = noDstNow.ToString("HH:mm");
            var correctedDate = noDstNow.ToString("yyyy/MM/dd");
            var offsetText = FormatOffset(zone.BaseUtcOffset);

            var tileXmlString =
                $@"<tile>
            <visual branding='name'>
                <binding template='TileMedium'>
                    <text hint-style='caption'>Corrected Time</text>
                    <text hint-style='title'>{correctedTime}</text>
                    <text hint-style='caption'>{offsetText}</text>
                </binding>
                <binding template='TileWide'>
                    <text hint-style='caption'>Iran No-DST Time</text>
                    <text hint-style='title'>{correctedTime}</text>
                    <text hint-style='caption'>{correctedDate}</text>
                    <text hint-style='caption'>{offsetText}</text>
                </binding>
                <binding template='TileLarge'>
                    <text hint-style='caption'>Iran No-DST Time</text>
                    <text hint-style='header'>{correctedTime}</text>
                    <text hint-style='caption'>{correctedDate}</text>
                    <text hint-style='caption'>Expected offset: {offsetText}</text>
                </binding>
            </visual>
        </tile>";

            var tileXml = new XmlDocument();
            tileXml.LoadXml(tileXmlString);

            var tileNotification = new TileNotification(tileXml);

            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        private string FormatOffset(TimeSpan offset)
        {
            string sign = offset >= TimeSpan.Zero ? "+" : "-";
            offset = offset.Duration();

            return $"UTC{sign}{offset.Hours:00}:{offset.Minutes:00}";
        }

    }
}
