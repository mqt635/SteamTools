using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive.Linq;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    public partial class ArchiSteamFarmPlusPageViewModel : TabItemViewModel
    {
        readonly IArchiSteamFarmService asfSerivce = DI.Get<IArchiSteamFarmService>();

        public ArchiSteamFarmPlusPageViewModel()
        {
            IconKey = nameof(ArchiSteamFarmPlusPageViewModel);


            ASFService.Current.SteamBotsSourceList
                      .Connect()
                      .ObserveOn(RxApp.MainThreadScheduler)
                      .Sort(SortExpressionComparer<Bot>.Descending(x => x.BotName))
                      .Bind(out _SteamBots)
                      .Subscribe();
        }

        public string? WebUrl => asfSerivce.GetIPCUrl();

        /// <summary>
        /// ASF bots
        /// </summary>
        private ReadOnlyObservableCollection<Bot> _SteamBots;
        public ReadOnlyObservableCollection<Bot> SteamBots => _SteamBots;

        public void RunOrStopASF()
        {
            if (!ASFService.Current.IsASFRuning)
                ASFService.Current.InitASF();
            else
                ASFService.Current.StopASF();
        }


        public void ShowAddBotWindow()
        {
            IShowWindowService.Instance.Show(CustomWindow.ASF_AddBot, new ASF_AddBotWindowViewModel(), string.Empty, ResizeModeCompat.CanResize);
        }

        public async void PauseOrResumeBotFarming(Bot bot)
        {
            (bool success, string message) r;

            if (bot.CardsFarmer.Paused)
            {
                r = bot.Actions.Resume();
            }
            else
            {
                r = await bot.Actions.Pause(true).ConfigureAwait(false);
            }
            var message = r.success ? r.message : string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, r.message);

            Toast.Show("<" + bot.BotName + "> " + message);
        }
    }
}
