using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Orbifold.Graphite
{
    /// <summary>
    /// Visualization of an <see cref="Edge"/>.
    /// </summary>
    public class VisualEdge : Shape
    {
        #region Dependency Properties

        public static readonly DependencyProperty X1Property = DependencyProperty.Register("X1", typeof(double), typeof(VisualEdge), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure) );

        


        public static readonly DependencyProperty Y1Property = DependencyProperty.Register("Y1", typeof(double), typeof(VisualEdge), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty X2Property = DependencyProperty.Register("X2", typeof(double), typeof(VisualEdge), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty Y2Property = DependencyProperty.Register("Y2", typeof(double), typeof(VisualEdge), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(VisualEdge), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var edge = obj as VisualEdge;
            if (edge != null && edge.EdgeLabel != null)
                edge.EdgeLabel.Text = args.NewValue.ToString();
        }

        private MultiBinding l1;
        private MultiBinding l2;

        #endregion

        #region CLR Properties

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        [TypeConverter(typeof(LengthConverter))]
        public double X1
        {
            get { return (double)GetValue(X1Property); }
            set { SetValue(X1Property, value); }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double Y1
        {
            get { return (double)GetValue(Y1Property); }
            set { SetValue(Y1Property, value); }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double X2
        {
            get { return (double)GetValue(X2Property); }
            set { SetValue(X2Property, value); }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double Y2
        {
            get { return (double)GetValue(Y2Property); }
            set { SetValue(Y2Property, value); }
        }


        #endregion
        public TextBlock EdgeLabel { get; set; }


        public VisualEdge()
        {
            EdgeLabel = new TextBlock { Text = Label };
        }
        #region Overrides

        protected override Geometry DefiningGeometry
        {
            get
            {
                var pt1 = new Point(X1, Y1);
                var pt2 = new Point(X2, Y2);
                var geometry = new LineGeometry(pt1, pt2);
                geometry.Freeze();
                return geometry;
            }
        }
        #endregion
        public void ShowLabel()
        {
            #region Label

            //if(!Design.ShowLabel)return;

            var l11 = new Binding
            {
                Source = X1,
            };
            var l12 = new Binding
            {
                Source = X2,
            };

             l1 = new MultiBinding
            {
                Converter = new LabelLocationConverter(),
            };

            l1.Bindings.Add(l11);
            l1.Bindings.Add(l12);

            var l21 = new Binding
            {
                Source = Y1,
            };
            var l22 = new Binding
            {
                Source = Y2,
            };

             l2 = new MultiBinding
            {
                Converter = new LabelLocationConverter(),
            };

            l2.Bindings.Add(l21);
            l2.Bindings.Add(l22);
           
            EdgeLabel.SetBinding(Canvas.LeftProperty, l1);
            EdgeLabel.SetBinding(Canvas.TopProperty, l2);

            #endregion
        }

    }
    public sealed class LabelLocationConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Length == 2 && value[0] is double && value[1] is double)
            {
                var a = (double)value[0];
                var b = (double)value[1];
                if (double.IsNaN(a) || double.IsNaN(b))
                {
                    return 0;
                }
                if (a < b)
                    return a + (b - a) / 2;
                return b + (a - b) / 2;

            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class Arrow : VisualEdge
    {
        #region Dependency Properties

        public static readonly DependencyProperty HeadWidthProperty = DependencyProperty.Register("HeadWidth", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty HeadHeightProperty = DependencyProperty.Register("HeadHeight", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region CLR Properties


        [TypeConverter(typeof(LengthConverter))]
        public double HeadWidth
        {
            get { return (double)GetValue(HeadWidthProperty); }
            set { SetValue(HeadWidthProperty, value); }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double HeadHeight
        {
            get { return (double)GetValue(HeadHeightProperty); }
            set { SetValue(HeadHeightProperty, value); }
        }

        #endregion

        #region Overrides

        protected override Geometry DefiningGeometry
        {
            get
            {
                // Create a StreamGeometry for describing the shape
                var geometry = new StreamGeometry {FillRule = FillRule.EvenOdd};

                using (StreamGeometryContext context = geometry.Open())
                {
                    InternalDrawArrowGeometry(context);
                }

                // Freeze the geometry for performance benefits
                geometry.Freeze();

                return geometry;
            }
        }

        #endregion

        #region Privates

        private void InternalDrawArrowGeometry(StreamGeometryContext context)
        {
            double theta = Math.Atan2(Y1 - Y2, X1 - X2);
            double sint = Math.Sin(theta);
            double cost = Math.Cos(theta);

            var pt1 = new Point(X1, Y1);
            var pt2 = new Point(X2, Y2);

            var pt3 = new Point(
                X2 + (HeadWidth * cost - HeadHeight * sint),
                Y2 + (HeadWidth * sint + HeadHeight * cost));

            var pt4 = new Point(
                X2 + (HeadWidth * cost + HeadHeight * sint),
                Y2 - (HeadHeight * cost - HeadWidth * sint));

            context.BeginFigure(pt1, true, false);
            context.LineTo(pt2, true, true);
            context.LineTo(pt3, true, true);
            context.LineTo(pt2, true, true);
            context.LineTo(pt4, true, true);
        }

        #endregion
    }

}