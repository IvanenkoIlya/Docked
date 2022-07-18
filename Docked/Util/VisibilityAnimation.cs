using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace Docked.Util
{
   public class VisibilityAnimation : DependencyObject
   {
      private static Dictionary<FrameworkElement, bool> _hookedElements = new Dictionary<FrameworkElement, bool>();

      #region DependencyProperties
      public static readonly DependencyProperty AppearAnimationProperty =
         DependencyProperty.RegisterAttached("AppearAnimation", typeof(AnimationTimeline), typeof(VisibilityAnimation),
            new FrameworkPropertyMetadata(null));

      public static readonly DependencyProperty DisppearAnimationProperty =
         DependencyProperty.RegisterAttached("DisappearAnimation", typeof(AnimationTimeline), typeof(VisibilityAnimation),
            new FrameworkPropertyMetadata(null));

      public static readonly DependencyProperty DurationProperty =
         DependencyProperty.RegisterAttached("Duration", typeof(double), typeof(VisibilityAnimation),
            new FrameworkPropertyMetadata(1000.0));

      public static readonly DependencyProperty AnimatedPropertyProperty =
         DependencyProperty.RegisterAttached("AnimatedProperty", typeof(DependencyProperty), typeof(VisibilityAnimation),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnAnimatedPropertyPropertyChanged)));

      public static AnimationTimeline GetAppearAnimation(DependencyObject obj)
      {
         return (AnimationTimeline)obj.GetValue(AppearAnimationProperty);
      }

      public static void SetAppearAnimation(DependencyObject obj, AnimationTimeline value)
      {
         obj.SetValue(AppearAnimationProperty, value);
      }

      public static AnimationTimeline GetDisappearAnimation(DependencyObject obj)
      {
         return (AnimationTimeline)obj.GetValue(DisppearAnimationProperty);
      }

      public static void SetDisappearAnimation(DependencyObject obj, AnimationTimeline value)
      {
         obj.SetValue(DisppearAnimationProperty, value);
      }

      public static double GetDuration(DependencyObject obj)
      {
         return (double)obj.GetValue(DurationProperty);
      }

      public static void SetDuration(DependencyObject obj, double value)
      {
         obj.SetValue(DurationProperty, value);
      }

      public static DependencyProperty GetAnimatedProperty(DependencyObject obj)
      {
         return (DependencyProperty)obj.GetValue(AnimatedPropertyProperty);
      }

      public static void SetAnimatedProperty(DependencyObject obj, DependencyProperty value)
      {
         obj.SetValue(AnimatedPropertyProperty, value);
      }

      private static void OnAnimatedPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var element = d as FrameworkElement;
         if (element == null)
            return;
         if (GetAnimatedProperty(element) != null)
            HookVisibilityChanged(element);
         else
            UnHookVisibilityChanged(element);
      }

      private static void HookVisibilityChanged(FrameworkElement frameworkElement)
      {
         _hookedElements.Add(frameworkElement, false);
      }

      private static void UnHookVisibilityChanged(FrameworkElement frameworkElement)
      {
         if (_hookedElements.ContainsKey(frameworkElement))
            _hookedElements.Remove(frameworkElement);
      }
      #endregion

      static VisibilityAnimation()
      {
         UIElement.VisibilityProperty.AddOwner(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    Visibility.Visible,
                    VisibilityChanged,
                    CoerceVisibility));
      }

      private static void VisibilityChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
      { }

      private static object CoerceVisibility(DependencyObject d, object baseValue)
      {
         var element = d as FrameworkElement;
         if (element == null)
            return baseValue;

         if (!element.IsLoaded)
            return baseValue;

         Visibility currentVisibility = element.Visibility;
         Visibility targetVisibility = (Visibility)baseValue;
         if (targetVisibility == currentVisibility)
            return baseValue;

         if (!IsHookedElement(element))
            return baseValue;

         if (UpdateAnimation(element))
            return baseValue;

         AnimationTimeline animation = (targetVisibility == Visibility.Visible) ?
            GetAppearAnimation(d) : GetDisappearAnimation(d);

         if (animation == null)
            return baseValue;

         animation.Duration = new Duration(TimeSpan.FromMilliseconds(GetDuration(d)));

         animation.Completed += (sender, args) =>
         {
            if (targetVisibility == Visibility.Visible)
            {
               UpdateAnimation(element);
            }
            else
            {
               if (BindingOperations.IsDataBound(element, UIElement.VisibilityProperty))
               {
                  Binding bindingValue = BindingOperations.GetBinding(element, UIElement.VisibilityProperty);
                  BindingOperations.SetBinding(element, UIElement.VisibilityProperty, bindingValue);
               }
               else
               {
                  element.Visibility = targetVisibility;
               }
            }
         };

         element.BeginAnimation(GetAnimatedProperty(d), animation);
         return baseValue;
      }

      private static bool IsHookedElement(FrameworkElement element)
      {
         return _hookedElements.ContainsKey(element);
      }

      private static bool UpdateAnimation(FrameworkElement element)
      {
         bool val = _hookedElements[element];
         _hookedElements[element] = !val;
         return val;
      }
   }
}