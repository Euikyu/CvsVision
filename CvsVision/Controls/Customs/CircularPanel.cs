using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CvsVision.Controls.Customs
{
    //public class CircularPanel : Panel
    //{

    //    /// <summary>
    //    /// CircularPanel.StartAngle 에 대한 종속성 속성을 식별합니다.
    //    /// </summary>
    //    public static readonly DependencyProperty StartAngleProperty =
    //        DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(CircularPanel),
    //            new PropertyMetadata(0.0, StartAngle_PropertyChanged, StartAngle_CoerceValue));

    //    private static object StartAngle_CoerceValue(DependencyObject o, object baseValue)
    //    {
    //        CircularPanel panel = (CircularPanel)o;
    //        if (baseValue is double angle)
    //        {
    //            return (angle + 360) % 360;
    //        }
    //        else return panel.StartAngle;
    //    }

    //    private static void StartAngle_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    //    {
    //        CircularPanel panel = (CircularPanel)o;
    //        panel.ArrangeOverride(panel.RenderSize);
    //    }

    //    /// <summary>
    //    /// CircularPanel.IntervalAngle 에 대한 종속성 속성을 식별합니다.
    //    /// </summary>
    //    public static readonly DependencyProperty IntervalAngleProperty =
    //        DependencyProperty.Register(nameof(IntervalAngle), typeof(double), typeof(CircularPanel),
    //            new PropertyMetadata(IntervalAngle_PropertyChanged));

    //    private static void IntervalAngle_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    //    {
    //        CircularPanel panel = (CircularPanel)o;
    //        panel.ArrangeOverride(panel.RenderSize);
    //    }

    //    /// <summary>
    //    /// CircularPanel.Radius 에 대한 종속성 속성을 식별합니다.
    //    /// </summary>
    //    public static readonly DependencyProperty RadiusProperty =
    //        DependencyProperty.Register(nameof(Radius), typeof(double), typeof(CircularPanel),
    //            new PropertyMetadata(double.NaN, Radius_PropertyChanged));

    //    private static void Radius_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    //    {
    //        if (e.NewValue is double)
    //        {
    //            CircularPanel panel = (CircularPanel)o;
    //            panel.Width = (double)e.NewValue * 2;
    //            panel.Height = (double)e.NewValue * 2;
    //        }
    //    }

    //    /// <summary>
    //    /// CircularPanel.Radius 에 대한 종속성 속성을 식별합니다.
    //    /// </summary>
    //    public static readonly DependencyProperty ItemsSourceProperty =
    //        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(CircularPanel),
    //            new PropertyMetadata(null, ItemsSource_PropertyChanged));

    //    private static void ItemsSource_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    //    {
    //        CircularPanel panel = (CircularPanel)o;
    //        if(panel.ItemTemplate != null)
    //        {

    //        }
    //    }

    //    public double StartAngle
    //    {
    //        get { return (double)GetValue(StartAngleProperty); }
    //        set { SetValue(StartAngleProperty, value); }
    //    }

    //    public double IntervalAngle
    //    {
    //        get { return (double)GetValue(IntervalAngleProperty); }
    //        set { SetValue(IntervalAngleProperty, value); }
    //    }
    //    public double Radius
    //    {
    //        get { return (double)GetValue(RadiusProperty); }
    //        set { SetValue(RadiusProperty, value); }
    //    }

    //    /// <summary>
    //    /// 지정된 중심축을 기준으로 회전 변환한 점을 반환하기.
    //    /// </summary>
    //    /// <param name="rotatePoint">회전 변환할 점.</param>
    //    /// <param name="rad">회전 라디안 값.</param>
    //    /// <param name="center">중심축 점.</param>
    //    /// <returns></returns>
    //    private Point GetPointByRotation(Point rotatePoint, double rad, Point center)
    //    {
    //        return new Point(Math.Cos(rad) * rotatePoint.X - Math.Sin(rad) * rotatePoint.Y - center.X, Math.Sin(rad) * rotatePoint.X + Math.Cos(rad) * rotatePoint.Y - center.Y);
    //    }

    //    protected override Size MeasureOverride(Size availableSize)
    //    {
    //        foreach(UIElement child in InternalChildren)
    //        {
    //            child.Measure(availableSize);
    //        }
    //        return availableSize;
    //    }
    //    protected override Size ArrangeOverride(Size finalSize)
    //    {
    //        double radius = double.IsNaN(this.Radius) ? (this.ActualWidth > this.ActualHeight ? this.ActualHeight / 2 : this.ActualWidth / 2) : this.Radius;
    //        int count = this.Children.Count;
            
    //        for(int i = 0; i < count; i++)
    //        {
    //            if(Children[i] is FrameworkElement element)
    //            {
    //                var angle = this.StartAngle + i * this.IntervalAngle;
    //                RotateTransform r = new RotateTransform
    //                {
    //                    Angle = angle + 90,
    //                    CenterX = element.Width / 2,
    //                    CenterY = element.Height / 2
    //                };

    //                element.RenderTransform = r;

    //                var v = this.GetPointByRotation(new Point(radius, 0), angle * Math.PI / 180, new Point()) - new Point(-radius, -radius);

    //                element.Arrange(new Rect(v.X - r.CenterX, v.Y - r.CenterY, element.Width, element.Height));
    //            }
    //        }
            
    //        return finalSize;
    //    }
    //}
}
