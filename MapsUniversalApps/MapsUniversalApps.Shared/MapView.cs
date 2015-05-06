//-----------------------------------------------------------------------
// <author>Instituto de Pesquisas Eldorado</author>
// <copyright file="MapView" company=Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MapsUniversalApps.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Geolocation;

#if WINDOWS_APP

using Bing.Maps;

#elif WINDOWS_PHONE_APP

using System;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Foundation;

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
#elif WINDOWS_PHONE_APP
            map = new MapControl();
#endif

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

        public void SetPosition(BasicGeoposition center, double zoom)
        {
#if WINDOWS_APP
            map.SetView(new Location(center.Latitude, center.Longitude), zoom);
            RaisePropertyChanged("Center");
            RaisePropertyChanged("Zoom");
#elif WINDOWS_PHONE_APP
            map.Center = new Geopoint(center);
            map.ZoomLevel = zoom;
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
            map.Children.Add(pushpin);
            MapLayer.SetPosition(pushpin, new Location(pushpinViewModel.Position.Latitude, pushpinViewModel.Position.Longitude));

            // MapPolygon is not supported by Windows 8.1 Bing Map SDK

#elif WINDOWS_PHONE_APP

            MapIcon mapIcon = new MapIcon();
            mapIcon.Image = pushpinViewModel.PushpinImage;
            mapIcon.Location = new Geopoint(pushpinViewModel.Position);
            mapIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
            mapIcon.Title = pushpinViewModel.Title == null ? string.Empty : pushpinViewModel.Title;
            map.MapElements.Add(mapIcon);

            MapPolygon precision = null;

            if (pushpinViewModel.Accuracy > 0)
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

#if WINDOWS_PHONE_APP

        /// <summary>
        /// The MapPolygon is only supported on Windows Phone.
        /// So, this method is only implemented for Windows Phone
        /// </summary>
        /// <param name="position">The position on map</param>
        /// <param name="accuracy">The accuracy obtained from position on map</param>
        /// <param name="precision">Specify the polygon if it already exist</param>
        /// <returns>Updated MapPolygon</returns>
        public MapPolygon GenerateMapAccuracyCircle(BasicGeoposition position, double accuracy, MapPolygon precision = null)
        {
            if (precision == null)
            {
                precision = new MapPolygon();
                precision.StrokeThickness = 1;
                precision.FillColor = Windows.UI.Color.FromArgb(80, 255, 0, 0);
            }

            var earthRadius = 6371;
            var lat = position.Latitude * Math.PI / 180.0; //radians
            var lon = position.Longitude * Math.PI / 180.0; //radians
            var d = accuracy / 1000 / earthRadius; // d = angular distance covered on earths surface

            List<BasicGeoposition> precisionPath = new List<BasicGeoposition>();
            for (int x = 0; x <= 360; x++)
            {
                var brng = x * Math.PI / 180.0; //radians
                var latRadians = Math.Asin(Math.Sin(lat) * Math.Cos(d) + Math.Cos(lat) * Math.Sin(d) * Math.Cos(brng));
                var lngRadians = lon + Math.Atan2(Math.Sin(brng) * Math.Sin(d) * Math.Cos(lat), Math.Cos(d) - Math.Sin(lat) * Math.Sin(latRadians));

                var pt = new BasicGeoposition()
                {
                    Latitude = 180.0 * (latRadians / Math.PI),
                    Longitude = 180.0 * (lngRadians / Math.PI)
                };

                precisionPath.Add(pt);
            }

            precision.Path = new Geopath(precisionPath);

            return precision;
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