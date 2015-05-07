//-----------------------------------------------------------------------
// <author>Instituto de Pesquisas Eldorado</author>
// <copyright file="MapView" company=Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MapsUniversalApps.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Geolocation;

#if WINDOWS_APP

using Bing.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

#elif WINDOWS_PHONE_APP

using Windows.UI.Xaml.Controls.Maps;
using Windows.Foundation;
using Windows.UI;

#endif

namespace MapsUniversalApps
{
    /// <summary>
    /// Enum with map types
    /// </summary>
    public enum MapTypeEnum
    {
        Aerial,
        Birdseye,
        Road
    }

    public class MapView : Grid, INotifyPropertyChanged
    {
        #region Private Fields

        private const double earthRadius = 6371000D;
        private const double Circumference = 2D * Math.PI * earthRadius;
        private double currentZoom;
        private TimeSpan myLocationZoomAnimation = TimeSpan.FromSeconds(1);

#if WINDOWS_APP
        private Map map;
#elif WINDOWS_PHONE_APP
        private MapControl map;
#endif
        private List<PushpinViewModel> pushpinViewModelList;

        #endregion Private Fields

        #region Constructor

        public MapView()
        {
#if WINDOWS_APP
            map = new Map();            

            map.ViewChanged += (s, e) =>
            {                
                //Update pushpins scale based on zomom level, if changed
                this.UpdatePushpins();
                this.currentZoom = map.ZoomLevel;
            };
#elif WINDOWS_PHONE_APP
            map = new MapControl();
            map.ZoomLevelChanged += (s, e) =>
            {
                this.currentZoom = map.ZoomLevel;
            };

#endif            
            this.currentZoom = map.ZoomLevel; 
            pushpinViewModelList = new List<PushpinViewModel>();            

            this.Children.Add(map);
        }

        #endregion Constructor

        #region Public Properties

        public bool ShowTraffic
        {
            get
            {
#if WINDOWS_APP
                return map.ShowTraffic;
#elif WINDOWS_PHONE_APP
                return map.TrafficFlowVisible;
#endif
            }
            set
            {
#if WINDOWS_APP
                map.ShowTraffic = value;
#elif WINDOWS_PHONE_APP
                map.TrafficFlowVisible = value;
#endif

                RaisePropertyChanged();
            }
        }

        public string Credentials
        {
            get
            {
#if WINDOWS_APP
                return map.Credentials;
#elif WINDOWS_PHONE_APP
                return string.Empty;
#endif
            }
            set
            {
#if WINDOWS_APP
                if (!string.IsNullOrEmpty(value))
                {
                    map.Credentials = value;
                }
#endif

                RaisePropertyChanged();
            }
        }

        public string MapServiceToken
        {
            get
            {
#if WINDOWS_APP
                return string.Empty;
#elif WINDOWS_PHONE_APP
                return map.MapServiceToken;
#endif
            }
            set
            {
#if WINDOWS_PHONE_APP
                if (!string.IsNullOrEmpty(value))
                {
                    map.MapServiceToken = value;
                }
#endif

                RaisePropertyChanged();
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void SetPosition(BasicGeoposition center, double zoom, Action zoomCompleted)
        {
#if WINDOWS_APP
            map.SetView(new Location(center.Latitude, center.Longitude), zoom, myLocationZoomAnimation);
            RaisePropertyChanged("Center");
            RaisePropertyChanged("Zoom");

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = myLocationZoomAnimation;
            timer.Tick += (sender, args) => {
                zoomCompleted();
            };
            timer.Start();
#elif WINDOWS_PHONE_APP
            map.Center = new Geopoint(center);
            map.ZoomLevel = zoom;
            zoomCompleted();
#endif
        }

        public void SetMapType(MapTypeEnum type)
        {
#if WINDOWS_APP
            switch (type)
            {
                case MapTypeEnum.Aerial:
                    map.MapType = MapType.Aerial;
                    break;

                case MapTypeEnum.Birdseye:
                    map.MapType = MapType.Birdseye;
                    break;

                case MapTypeEnum.Road:
                default:
                    map.MapType = MapType.Road;
                    break;
            }
#elif WINDOWS_PHONE_APP
            switch (type)
            {
                case MapTypeEnum.Aerial:
                    map.Style = MapStyle.Aerial;
                    break;

                case MapTypeEnum.Birdseye:
                    map.Style = MapStyle.AerialWithRoads;
                    break;

                case MapTypeEnum.Road:
                default:
                    map.Style = MapStyle.Terrain;
                    break;
            }
#endif
        }

        public async void SetAutomaticallyZoom(List<BasicGeoposition> basicPositions)
        {
#if WINDOWS_APP
            LocationCollection locationCollection = new LocationCollection();
            foreach (PushpinViewModel pushpinViewModel in pushpinViewModelList)
            {
                Bing.Maps.Location location = new Location(pushpinViewModel.Position.Latitude, pushpinViewModel.Position.Longitude);
                locationCollection.Add(location);
            }
            map.SetView(new LocationRect(locationCollection));

#elif WINDOWS_PHONE_APP
            map.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(basicPositions), null, MapAnimationKind.Default);

#endif
        }

        public void AddPin(PushpinViewModel pushpinViewModel)
        {
            pushpinViewModelList.Add(pushpinViewModel);

#if WINDOWS_APP
                        
            Pushpin pushpin = new Pushpin();
            pushpin.DataContext = pushpinViewModel;
            pushpin.Margin = new Windows.UI.Xaml.Thickness((-1) * pushpin.Width / 2, (-1) * pushpin.Height, 0, 0);
            map.Children.Add(pushpin);            
            MapLayer.SetPosition(pushpin, new Location(pushpinViewModel.Position.Latitude, pushpinViewModel.Position.Longitude));
                        
            if (pushpinViewModel.IsMyLocation && pushpinViewModel.Accuracy > 0)
            {
                AccuracyCircle precision = (AccuracyCircle)map.Children.Where(c => c is AccuracyCircle).FirstOrDefault();
                precision = GenerateMapAccuracyCircle(pushpinViewModel.Position, pushpinViewModel.Accuracy, precision);
                precision.DataContext = pushpinViewModel;
                map.Children.Add(precision);
                MapLayer.SetPosition(precision, new Location(pushpinViewModel.Position.Latitude, pushpinViewModel.Position.Longitude));
            }

#elif WINDOWS_PHONE_APP

            MapIcon mapIcon = new MapIcon();
            mapIcon.Image = pushpinViewModel.PushpinImage;
            mapIcon.Location = new Geopoint(pushpinViewModel.Position);
            mapIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
            mapIcon.Title = pushpinViewModel.Title == null ? string.Empty : pushpinViewModel.Title;
            map.MapElements.Add(mapIcon);

            MapPolygon precision = null;

            if (pushpinViewModel.IsMyLocation && pushpinViewModel.Accuracy > 0)
            {
                precision = GenerateMapAccuracyCircle(pushpinViewModel.Position, pushpinViewModel.Accuracy);
                map.MapElements.Add(precision);
            }

            //make a new source
            pushpinViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("Title"))
                {
                    mapIcon.Title = ((PushpinViewModel)sender).Title;
                }
                else if (args.PropertyName.Equals("PushpinImage"))
                {
                    mapIcon.Image = pushpinViewModel.PushpinImage;
                }
                else if (args.PropertyName.Equals("Position"))
                {
                    mapIcon.Location = new Geopoint(pushpinViewModel.Position);
                }
                else if (args.PropertyName.Equals("Accuracy") && precision != null)
                {
                    GenerateMapAccuracyCircle(pushpinViewModel.Position, pushpinViewModel.Accuracy, precision);
                }
            };

#endif
        }

#if WINDOWS_APP

        public void UpdatePushpins()
        {
            foreach (UIElement element in map.Children)
            {
                if (element is Pushpin)
                {
                    Pushpin pushpin = element as Pushpin; 
                    ScaleTransform scaleTrans = element.RenderTransform as ScaleTransform;
                    if (pushpin.RenderTransform == null || !(pushpin.RenderTransform is ScaleTransform))
                    {
                        scaleTrans = new ScaleTransform();
                        pushpin.RenderTransform = scaleTrans;
                    }
                    
                    double delta = Math.Floor(Math.Abs(currentZoom - map.ZoomLevel));
                    
                    scaleTrans.ScaleX *= delta == 0 ? 1 : delta*0.1;
                    scaleTrans.ScaleY = scaleTrans.ScaleX;
                    double marginLeft = (-1) * (pushpin.Width / 2) * scaleTrans.ScaleX; 
                    double marginTop = (-1) * pushpin.Height * scaleTrans.ScaleX;                    
                    pushpin.Margin = new Windows.UI.Xaml.Thickness(marginLeft, marginTop, 0, 0);
                }
                else if (element is AccuracyCircle)
                {
                    AccuracyCircle precision = element as AccuracyCircle;
                    PushpinViewModel pushpinViewModel = (PushpinViewModel)precision.DataContext;
                    GenerateMapAccuracyCircle(pushpinViewModel.Position, pushpinViewModel.Accuracy, precision);
                }
            }
        }

        public AccuracyCircle GenerateMapAccuracyCircle(BasicGeoposition position, double accuracy, AccuracyCircle accuracyCircle = null)
        {
            //Calculate the ground resolution in meters/pixel
            //Math based on http://msdn.microsoft.com/en-us/library/bb259689.aspx
            double groundResolution = Math.Cos(position.Latitude * Math.PI / 180) *
                2 * Math.PI * earthRadius / (256 * Math.Pow(2, this.map.ZoomLevel));

            //Calculate the radius of the accuracy circle in pixels
            double pixelRadius = accuracy / groundResolution;

            //Update the accuracy circle dimensions
            if (accuracyCircle == null)
            {
                accuracyCircle = new AccuracyCircle();
            }
            
            accuracyCircle.Width = pixelRadius;
            accuracyCircle.Height = pixelRadius;

            //Use the margin property to center the accuracy circle
            accuracyCircle.Margin = new Windows.UI.Xaml.Thickness(-pixelRadius / 2, -pixelRadius / 2, 0, 0);

            return accuracyCircle;
        }

#elif WINDOWS_PHONE_APP

        public MapPolygon GenerateMapAccuracyCircle(BasicGeoposition position, double accuracy, MapPolygon precision = null)
        {
            Color FillColor = Colors.Purple;
            Color StrokeColor = Colors.Red;
            FillColor.A = 80;
            StrokeColor.A = 80;
            precision = new MapPolygon
            {
                StrokeThickness = 2,
                FillColor = FillColor,
                StrokeColor = StrokeColor,
                Path = new Geopath(CalculateCircle(position, accuracy))
            };

            return precision;
        }

        // Constants and helper functions:

        
        public static List<BasicGeoposition> CalculateCircle(BasicGeoposition Position, double Radius)
        {
            List<BasicGeoposition> GeoPositions = new List<BasicGeoposition>();
            for (int i = 0; i <= 360; i++)
            {
                double Bearing = ToRad(i);
                double CircumferenceLatitudeCorrected = 2D * Math.PI * Math.Cos(ToRad(Position.Latitude)) * earthRadius;
                double lat1 = Circumference / 360D * Position.Latitude;
                double lon1 = CircumferenceLatitudeCorrected / 360D * Position.Longitude;
                double lat2 = lat1 + Math.Sin(Bearing) * Radius;
                double lon2 = lon1 + Math.Cos(Bearing) * Radius;
                BasicGeoposition NewBasicPosition = new BasicGeoposition();
                NewBasicPosition.Latitude = lat2 / (Circumference / 360D);
                NewBasicPosition.Longitude = lon2 / (CircumferenceLatitudeCorrected / 360D);
                GeoPositions.Add(NewBasicPosition);
            }
            return GeoPositions;
        }

        private static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180D);
        }

#endif

        public List<PushpinViewModel> GetPushpinList()
        {
            return this.pushpinViewModelList;
        }

        #endregion Public Methods

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                var eventArgs = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, eventArgs);
            }
        }

        #endregion INotifyPropertyChanged Members
    }
}