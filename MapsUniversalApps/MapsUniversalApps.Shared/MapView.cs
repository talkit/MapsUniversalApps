//-----------------------------------------------------------------------
// <author>Instituto de Pesquisas Eldorado</author>
// <copyright file="MapView" company=Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MapsUniversalApps.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;

#if WINDOWS_APP
using Bing.Maps;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;
#elif WINDOWS_PHONE_APP
using System;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Data;
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

        #endregion

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

        #endregion

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

        #endregion

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
            LocationCollection locationCollection = new LocationCollection ();
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

#elif WINDOWS_PHONE_APP

            MapIcon mapIcon = new MapIcon();
            mapIcon.Image = pushpinViewModel.PushpinImage;
            mapIcon.Location = new Geopoint(pushpinViewModel.Position);
            mapIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
            mapIcon.Title = pushpinViewModel.Title == null ? string.Empty : pushpinViewModel.Title;
            map.MapElements.Add(mapIcon);

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
            else if (args.PropertyName.Equals("position"))
            {
                mapIcon.Location = new Geopoint(pushpinViewModel.Position);
            }
        };

#endif      
        }

        public List<PushpinViewModel> GetPushpinList()
        {
            return this.pushpinViewModelList;
        }

        #endregion

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

        #endregion


    }
}
