﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MapsUniversalApps.ViewModels;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MapsUniversalApps
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<BasicGeoposition> BasicPositions { get; set; }

        private const string MYLOCATION = "mylocation";

        public MainPage()
        {
            this.InitializeComponent();
            BasicPositions = new List<BasicGeoposition>();
        }        

        private async void MyLocationBtn_Clicked(object sender, RoutedEventArgs e)
        {
            // Get user location
            Geolocator geolocator = new Geolocator();
            Geoposition geoposition = await geolocator.GetGeopositionAsync();

            BasicGeoposition position = new BasicGeoposition() { Latitude = geoposition.Coordinate.Point.Position.Latitude, Longitude = geoposition.Coordinate.Point.Position.Longitude };
            this.BasicPositions.Add(position);

            // Set position in map
            Map.SetPosition(position, 15);

            // add/update the pushpin
            var list = Map.GetPushpinList();

            var myPushpin = list.Where(pin => pin.IsMyLocation).FirstOrDefault();
            if (myPushpin == null)
            {
                myPushpin = new PushpinViewModel(true);
                myPushpin.Title = "Estou aqui!";
                myPushpin.Position = position;

                // Add pin
                Map.AddPin(myPushpin);
            }
            else
            {
                myPushpin.Position = position;
            }
        }

        private void RoadBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Map.SetMapType(MapTypeEnum.Road);
        }

        private void BirdseyeBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Map.SetMapType(MapTypeEnum.Birdseye);
        }

        private void AerialBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Map.SetMapType(MapTypeEnum.Aerial);
        }

        private void ToggleTrafficBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Map.ShowTraffic = !Map.ShowTraffic;
        }

        private void SetZoomButton_Checked(object sender, RoutedEventArgs e)
        {
            Map.SetAutomaticallyZoom(BasicPositions);
        }

        private async void AddRandomPushpin_Clicked(object sender, RoutedEventArgs e)
        {
            // Get user location
            Geolocator geolocator = new Geolocator();
            Geoposition geoposition = await geolocator.GetGeopositionAsync();
            BasicGeoposition position = new BasicGeoposition() { Latitude = geoposition.Coordinate.Point.Position.Latitude, Longitude = geoposition.Coordinate.Point.Position.Longitude };
            
            double latitude = position.Latitude;
            double longitude = position.Longitude;

            Random random = new Random();

            for (int i = 0; i < 20; i++)
            {
                PushpinViewModel pushpinViewModel = new PushpinViewModel();                

                int signalRandom = random.Next(-1, 1);
                signalRandom = signalRandom == 0 ? 1 : signalRandom;
                double latitudeDelta = (random.NextDouble() / 50) * signalRandom;

                signalRandom = random.Next(-1, 1);
                signalRandom = signalRandom == 0 ? 1 : signalRandom;
                double longitudeDelta = (random.NextDouble() / 50) * signalRandom;

                latitude += latitudeDelta;
                longitude += longitudeDelta;

                pushpinViewModel.Position = new BasicGeoposition() { Latitude = latitude, Longitude = longitude };

                Map.AddPin(pushpinViewModel);

                BasicPositions.Add(new BasicGeoposition() { Latitude = latitude, Longitude = longitude });
            }
        }     
   
        private void ChangeMyPushpin_Clicked(object sender, RoutedEventArgs e)
        {
            var list = Map.GetPushpinList();

            var myPushpin = list.Where(pin => pin.IsMyLocation).FirstOrDefault();
            if (myPushpin != null)
            {
                myPushpin.ChangePushpinImage();
            }
        }
    }
}