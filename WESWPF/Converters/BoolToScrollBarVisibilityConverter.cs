using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WES.Models;

namespace WES.Converters
{
    public class BoolToScrollBarVisibilityConverter : IValueConverter
    {
        // 当IsScrollDisabled为true时，返回Disabled（禁用滚动）；否则返回Auto（允许滚动）
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isDisabled && isDisabled ? ScrollBarVisibility.Disabled : (object)ScrollBarVisibility.Auto;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // 无需反向转换
        }
    }

    public class HasItemsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 判断子菜单集合是否有元素
            if (value is List<MenuModel> subMenus && subMenus.Count > 0)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 展开状态转图标
    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? IconChar.CaretUp : IconChar.CaretDown;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
