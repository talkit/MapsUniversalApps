using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#if WINDOWS_APP

using Windows.UI.Xaml.Media;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Media.Imaging;

#elif WINDOWS_PHONE_APP

using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Devices.Geolocation;

#endif

namespace MapsUniversalApps.ViewModels
{
    public class PushpinViewModel : INotifyPropertyChanged
    {
        #region Fields and enums

        public enum PushpinEnum
        {
            Pushpin1,
            Pushpin2,
            Pushpin3
        };

        private bool isMyLocation;
        private string title;
        private PushpinEnum imageSourceKey = PushpinEnum.Pushpin1;
        private BasicGeoposition position;
        private double accuracy = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Fields and enums

        #region Constructors

        public PushpinViewModel(bool isMyLocation = false)
        {
            this.isMyLocation = isMyLocation;
        }

        #endregion Constructors

        #region properties

        public bool IsMyLocation
        {
            get
            {
                return this.isMyLocation;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.title = value;
                RaisePropertyChanged();
            }
        }

        public PushpinEnum ImageSourceKey
        {
            get
            {
                return imageSourceKey;
            }

            set
            {
                this.imageSourceKey = value;
                RaisePropertyChanged();
            }
        }

        public BasicGeoposition Position
        {
            get
            {
                return this.position;
            }

            set
            {
                this.position = value;
                RaisePropertyChanged();
            }
        }

        public double Accuracy
        {
            get
            {
                return this.accuracy;
            }

            set
            {
                this.accuracy = value;
                RaisePropertyChanged();
            }
        }

#if WINDOWS_APP

        public ImageSource PushpinImage
        {
            get
            {
                BitmapImage bitmapImage = (BitmapImage)App.Current.Resources[this.ImageSourceKey.ToString()];
                return bitmapImage;
            }
        }

#elif WINDOWS_PHONE_APP

        public RandomAccessStreamReference PushpinImage
        {
            get
            {
                BitmapImage bitmapImage = (BitmapImage)App.Current.Resources[this.ImageSourceKey.ToString()];
                return RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx://" + bitmapImage.UriSource.AbsolutePath, UriKind.Absolute));
            }
        }

#endif

        #endregion properties

        #region methods

        public void ChangePushpinImage()
        {
            PushpinEnum currentPushpin = this.ImageSourceKey;

            int currentPushpinValue = (int)this.ImageSourceKey;
            int sizeOfOPushpins = Enum.GetNames(typeof(PushpinEnum)).Length;

            this.ImageSourceKey = (PushpinEnum)((currentPushpinValue + 1) % sizeOfOPushpins);
            RaisePropertyChanged("PushpinImage");
        }

        // Create the OnPropertyChanged method to raise the event
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                var eventArgs = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, eventArgs);
            }
        }

        #endregion methods
    }
}