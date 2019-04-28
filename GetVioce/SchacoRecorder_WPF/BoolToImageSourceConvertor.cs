using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SchacoRecorder_WPF
{
   public class BoolToImageSourceConvertor: IValueConverter
    {
        ImageSource _playImage;
        private ImageSource playImage
        {
            get
            {
                if (_playImage == null)
                {
                    _playImage = new BitmapImage(new Uri("pack://application:,,,/SchacoRecorder_WPF;component/Resource/Play.png", UriKind.Absolute));
                }
                return _playImage;
            }
        }

        ImageSource _pauseImage;
        private ImageSource pauseImage
        {
            get
            {
                if (_pauseImage == null)
                {
                    _pauseImage = new BitmapImage(new Uri("pack://application:,,,/SchacoRecorder_WPF;component/Resource/Pause.png", UriKind.Absolute));
                }
                return _pauseImage;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is bool) && (bool)value.Equals(true))
            {
                return pauseImage;
            }
            else
            {
                return playImage;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
