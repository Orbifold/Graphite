using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Orbifold.Graphite
{
    /// <summary>
    /// Helpful methods related to WPF animation.
    /// </summary>
    public static class AnimationExtentions
    {
        /// <summary>
        /// This will create a fade-in-out effect on the opacity of the given element.
        /// </summary>
        /// <param name="fe">The framework element you wish to apply the effect on.</param>
        public static void ApplyFade(FrameworkElement fe)
        {
            var da = new DoubleAnimationUsingKeyFrames{Duration = new TimeSpan(0, 0, 0, 4, 0)};
            var kf0 = new SplineDoubleKeyFrame {Value = 0D, KeyTime = TimeSpan.FromSeconds(0)};
            var kf1 = new SplineDoubleKeyFrame {Value = 1D, KeyTime = TimeSpan.FromSeconds(2)};
            var kf3 = new SplineDoubleKeyFrame {Value = 0D, KeyTime = TimeSpan.FromSeconds(4)};
            da.KeyFrames.Add(kf0);
            da.KeyFrames.Add(kf1);
            da.KeyFrames.Add(kf3);

            var sb = new Storyboard();
            Storyboard.SetTarget(da,fe);
            Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));
            sb.Children.Add(da);
            sb.Begin();
        }


        /// <summary>
        /// Animation extension for <see cref="TranslateTransform"/>.
        /// </summary>
        /// <param name="translateTransform">The translate transform.</param>
        /// <param name="milliseconds">The milliseconds.</param>
        /// <param name="x">The x end-position.</param>
        /// <param name="y">The y end-position.</param>
        /// <param name="completed">The enventhandler to be called when the animation is completed.</param>
        public static void AnimateTo(this TranslateTransform translateTransform, int milliseconds, double x, double y, EventHandler completed)
        {

            var sb = new Storyboard();
            var daX = new DoubleAnimation
            {
                Duration = new TimeSpan(0, 0, 0, 0, milliseconds)
            };

            Storyboard.SetTargetProperty(daX, new PropertyPath("(TranslateTransform.X)"));
            Storyboard.SetTarget(daX, translateTransform);
            daX.To = x;
            var daY = new DoubleAnimation
                          {
                              Duration = new TimeSpan(0, 0, 0, 0, milliseconds)
                          };
            Storyboard.SetTargetProperty(daY, new PropertyPath("(TranslateTransform.Y)"));
            Storyboard.SetTarget(daY, translateTransform);
            daY.To = y;
            sb.Children.Add(daX);
            sb.Children.Add(daY);
            //((FrameworkElement)Application.Current.RootVisual).Resources.Add(Guid.NewGuid().ToString(), sb);
            sb.Begin();
            if (completed != null)
                sb.Completed += completed;

        }
        /// <summary>
        /// Animates to.
        /// </summary>
        /// <param name="rotateTransform">The rotate transform.</param>
        /// <param name="milliseconds">The milliseconds.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="relative">if set to <c>true</c> [relative].</param>
        /// <param name="completed">The enventhandler to be called when the animation is completed.</param>
        public static void AnimateTo(this RotateTransform rotateTransform, int milliseconds, double angle, bool relative, EventHandler completed)
        {

            var sb = new Storyboard();
            if (relative)
                angle = rotateTransform.Angle + angle;
            var da = new DoubleAnimation
                         {
                             Duration = new TimeSpan(0, 0, 0, 0, milliseconds)
                         };
            Storyboard.SetTargetProperty(da, new PropertyPath("(RotateTransform.Angle)"));
            Storyboard.SetTarget(da, rotateTransform);
            da.To = angle;
            sb.Children.Add(da);
            //((FrameworkElement)Application.Current.RootVisual).Resources.Add(Guid.NewGuid().ToString(), sb);
            sb.Begin();
            if (completed != null)
                sb.Completed += completed;
        }
        /// <summary>
        ///  Animation extension for <see cref="ScaleTransform"/>.
        /// </summary>
        /// <param name="scaleTransform">The scale transform.</param>
        /// <param name="milliseconds">The milliseconds.</param>
        /// <param name="scaleX">The end-scale in the X direction.</param>
        /// <param name="scaleY">The end-scale in the Y direction.</param>
        /// <param name="completed">The enventhandler to be called when the animation is completed.</param>
        public static void AnimateTo(this ScaleTransform scaleTransform, int milliseconds, double scaleX, double scaleY, EventHandler completed)
        {
            var sb = new Storyboard();
            var daX = new DoubleAnimation
                          {
                              Duration = new TimeSpan(0, 0, 0, 0, milliseconds)
                          };
            Storyboard.SetTargetProperty(daX, new PropertyPath("(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(daX, scaleTransform);
            daX.To = scaleX;
            var daY = new DoubleAnimation
                          {
                              Duration = new TimeSpan(0, 0, 0, 0, milliseconds)
                          };
            Storyboard.SetTargetProperty(daY, new PropertyPath("(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(daY, scaleTransform);

            daY.To = scaleY;
            sb.Children.Add(daX);
            sb.Children.Add(daY);
            //((FrameworkElement)Application.Current.RootVisual).Resources.Add(Guid.NewGuid().ToString(), sb);
            sb.Begin();
            if (completed != null)
                sb.Completed += completed;

        }

    }
}