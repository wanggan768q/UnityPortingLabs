using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Windows.Foundation;
using Windows.Devices.Geolocation;

using UnityApp = UnityPlayer.UnityApp;
using UnityBridge = WinRTBridge.WinRTBridge;

namespace DemoGame
{
	public partial class MainPage : PhoneApplicationPage
	{
		private bool _unityStartedLoading;
		private bool _useLocation;

		// Constructor
		public MainPage()
		{
			var bridge = new UnityBridge();
			UnityApp.SetBridge(bridge);
			InitializeComponent();
			bridge.Control = DrawingSurfaceBackground;

            WindowsGateway.OnClickPlay = OnClickPlay;
            
		}

        private void OnClickPlay()
        {
            Dispatcher.BeginInvoke( () =>
                {
                    btnBuy.Visibility = System.Windows.Visibility.Collapsed;
                    btnPause.Visibility = System.Windows.Visibility.Visible;
                }
            );
            
        }

		private void DrawingSurfaceBackground_Loaded(object sender, RoutedEventArgs e)
		{
			if (!_unityStartedLoading)
			{
				_unityStartedLoading = true;

				UnityApp.SetLoadedCallback(() => { Dispatcher.BeginInvoke(Unity_Loaded); });

				var content = Application.Current.Host.Content;
				var width = (int)Math.Floor(content.ActualWidth * content.ScaleFactor / 100.0 + 0.5);
				var height = (int)Math.Floor(content.ActualHeight * content.ScaleFactor / 100.0 + 0.5);

				UnityApp.SetNativeResolution(width, height);
				UnityApp.SetRenderResolution(width, height);
				UnityPlayer.UnityApp.SetOrientation((int)Orientation);

				DrawingSurfaceBackground.SetBackgroundContentProvider(UnityApp.GetBackgroundContentProvider());
				DrawingSurfaceBackground.SetBackgroundManipulationHandler(UnityApp.GetManipulationHandler());
			}
		}

		private void Unity_Loaded()
		{
			SetupGeolocator();
		}

		private void PhoneApplicationPage_BackKeyPress(object sender, CancelEventArgs e)
		{
			e.Cancel = UnityApp.BackButtonPressed();

            
            btnBuy.Visibility = System.Windows.Visibility.Visible;
            btnPause.Visibility = System.Windows.Visibility.Collapsed;
            
		}

		private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
		{
			UnityApp.SetOrientation((int)e.Orientation);
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!UnityApp.IsLocationEnabled())
                return;
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
                _useLocation = (bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"];
            else
            {
                MessageBoxResult result = MessageBox.Show("Can this application use your location?",
                    "Location Services", MessageBoxButton.OKCancel);
                _useLocation = result == MessageBoxResult.OK;
                IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = _useLocation;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

		private void SetupGeolocator()
        {
            if (!_useLocation)
                return;

            try
            {
				UnityApp.EnableLocationService(true);
                Geolocator geolocator = new Geolocator();
				geolocator.ReportInterval = 5000;
                IAsyncOperation<Geoposition> op = geolocator.GetGeopositionAsync();
                op.Completed += (asyncInfo, asyncStatus) =>
                    {
                        if (asyncStatus == AsyncStatus.Completed)
                        {
                            Geoposition geoposition = asyncInfo.GetResults();
                            UnityApp.SetupGeolocator(geolocator, geoposition);
                        }
                        else
                            UnityApp.SetupGeolocator(null, null);
                    };
            }
            catch (Exception)
            {
                UnityApp.SetupGeolocator(null, null);
            }
        }

        private void OnClickPause(object sender, RoutedEventArgs e)
        {

        }

        private void OnClickBuy(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Hud/StorePage.xaml", UriKind.Relative));
        }

	}
}