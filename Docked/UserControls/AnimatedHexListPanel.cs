using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Docked.UserControls
{
   public class AnimatedHexListPanel : Panel
   {
      #region DependencyProperties
      public static readonly DependencyProperty ColumnsProperty =
         DependencyProperty.Register("Columns", typeof(int), typeof(AnimatedHexListPanel),
            new PropertyMetadata(5));

      public int Columns
      {
         get => (int)GetValue(ColumnsProperty);
         set => SetValue(ColumnsProperty, value);
      }
      #endregion

      private readonly double duration = 200;
      private Dictionary<int, Point> previousPositions = new Dictionary<int, Point>();
      public Storyboard ArrangeStoryboard;

      public AnimatedHexListPanel()
      {
         Background = Brushes.Transparent;
         ArrangeStoryboard = new Storyboard();
         ArrangeStoryboard.Completed += ResetStoryboard;
      }

      protected override Size MeasureOverride(Size availableSize)
      {
         if (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height))
         {
            var idealSize = new Size(
              double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
              double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);

            foreach (UIElement child in InternalChildren)
            {
               child.Measure(new Size(availableSize.Width / (Columns + 0.5), availableSize.Height));
               child.Measure(new Size(availableSize.Width, child.DesiredSize.Height));
            }

            var exclusiveIndices = Enumerable.Range(0, InternalChildren.Count)
               .Where(x => RequiresOwnRow(InternalChildren[x], availableSize))
               .ToList();

            int index = 0;
            int lastGroupIndex = 0;
            foreach (int exclusiveIndex in exclusiveIndices)
            {
               var groupSize = new Size(
                  double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
                  double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);

               for (; index < exclusiveIndex; index++)
               {
                  var child = InternalChildren[index];
                  groupSize.Width = Math.Max(groupSize.Width, child.DesiredSize.Width * (Columns + 0.5));
                  groupSize.Height = Math.Max(groupSize.Height, child.DesiredSize.Height * ((exclusiveIndex - lastGroupIndex - 1) / Columns + 1));// Not quite right, need to adjust for 0.75
               }
               idealSize.Width = Math.Max(idealSize.Width, Math.Max(groupSize.Width, InternalChildren[index].DesiredSize.Width));
               idealSize.Height += groupSize.Height + InternalChildren[index].DesiredSize.Height;
               index++;
               lastGroupIndex = index;
            }

            if (index < InternalChildren.Count)
            {
               var groupSize = new Size(
                  double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
                  double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);

               // finish off remaining elements
               for (; index < InternalChildren.Count; index++)
               {
                  var child = InternalChildren[index];
                  groupSize.Width = Math.Max(groupSize.Width, child.DesiredSize.Width * (Columns + 0.5));
                  groupSize.Height = Math.Max(groupSize.Height, child.DesiredSize.Height * ((InternalChildren.Count - lastGroupIndex - 1) / Columns + 1)); // Not quite right, need to adjust for 0.75
               }

               idealSize.Width = Math.Max(idealSize.Width, groupSize.Width);
               idealSize.Height += groupSize.Height;
            }

            return idealSize;
         }

         return availableSize;
      }

      protected override Size ArrangeOverride(Size finalSize)
      {
         if (Children == null || Children.Count == 0)
            return finalSize;

         foreach (UIElement child in InternalChildren)
         {
            var group = new TransformGroup();
            group.Children.Add(new TranslateTransform());

            child.RenderTransform = group;
            child.RenderTransformOrigin = new Point(10, 10);
            child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));
         }

         ComplexAnimate(finalSize);

         return finalSize;
      }

      private void ComplexAnimate(Size availableSize)
      {
         ArrangeStoryboard.Stop();
         ArrangeStoryboard.Children.Clear();
         NameScope.SetNameScope(this, new NameScope());

         Dictionary<int,Point> newPositions = new Dictionary<int,Point>();

         int groupIndex = 0;
         Point groupStartingPoint = new Point(0, 0);
         Point nextGroupStartingPoint = new Point(0, 0);

         for(int i = 0; i < InternalChildren.Count; i++)
         {
            var child = InternalChildren[i];
            int hash = child is ContentPresenter ? (child as ContentPresenter).Content.GetHashCode() : child.GetHashCode();
            Point nextPoint, prevPoint = new Point(0, 0);

            if (previousPositions.ContainsKey(hash))
               prevPoint = previousPositions[hash];

            if (RequiresOwnRow(child, availableSize))
            {
               groupStartingPoint.Y += nextGroupStartingPoint.Y;
               nextGroupStartingPoint.Y = 0;
               nextPoint = new Point()
               {
                  X = groupStartingPoint.X,
                  Y = groupStartingPoint.Y
               };
               groupStartingPoint.Y += child.DesiredSize.Height;
               groupIndex = 0;
            } 
            else
            {
               int x = groupIndex % Columns, y = groupIndex / Columns;
               nextPoint = new Point()
               {
                  X = child.DesiredSize.Width * (x + (y % 2 /2.0)) + groupStartingPoint.X,
                  Y = 0.75 * y * child.DesiredSize.Height + groupStartingPoint.Y
               };
               nextGroupStartingPoint.Y = ((groupIndex/ Columns + 1) * 0.75 + 0.25) * child.DesiredSize.Height;
               groupIndex++;
            }

            newPositions[hash] = nextPoint;
            Animate(child, prevPoint.X , prevPoint.Y, nextPoint.X, nextPoint.Y, duration, hash);
         }

         previousPositions = newPositions;
         ArrangeStoryboard.Begin(this);
      }

      private void Animate(UIElement element, double fromX, double fromY, double toX, double toY, double duration, int hash)
      {
         var trans = (TranslateTransform)((TransformGroup)element.RenderTransform).Children[0];
         RegisterName($"TranslateTransform{hash}", trans);

         if (duration == 0)
         {
            trans.X = toX;
            trans.Y = toY;
         }
         else
         {
            var animX = CreateAnimation(fromX, toX, duration);
            var animY = CreateAnimation(fromY, toY, duration);

            Storyboard.SetTargetName(animX, $"TranslateTransform{hash}");
            Storyboard.SetTargetName(animY, $"TranslateTransform{hash}");
            Storyboard.SetTargetProperty(animX, new PropertyPath(TranslateTransform.XProperty));
            Storyboard.SetTargetProperty(animY, new PropertyPath(TranslateTransform.YProperty));

            ArrangeStoryboard.Children.Add(animX);
            ArrangeStoryboard.Children.Add(animY);
         }
      }

      private void ResetStoryboard(object sender, EventArgs e)
      {
         ArrangeStoryboard.Children.Clear();
      }

      private DoubleAnimation CreateAnimation(double from, double to, double duration)
      {
         return CreateAnimation(from, to, duration, null);
      }

      private DoubleAnimation CreateAnimation(double from, double to, double duration, EventHandler endEvent)
      {
         DoubleAnimation anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(duration))
         {
            AccelerationRatio = 0.2,
            DecelerationRatio = 0.7,
            EasingFunction = new SineEase()
         };
         if (endEvent != null)
            anim.Completed += endEvent;
         return anim;
      }

      private bool RequiresOwnRow(UIElement child, Size availableSize)
      {
         return child.DesiredSize.Width * (Columns + 0.5) > availableSize.Width;
      }

      #region Basic Measure/Arrange functions
      private Size SimpleMeasure(Size availableSize)
      {
         if (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height))
         {
            var idealSize = new Size(
              double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
              double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);

            for (int i = 0; i < InternalChildren.Count; i++)
            {
               var child = InternalChildren[i];
               child.Measure(availableSize);
               idealSize.Width = Math.Max(idealSize.Width, child.DesiredSize.Width * (Columns + 0.5));
               idealSize.Height = Math.Max(idealSize.Height, child.DesiredSize.Height * ((InternalChildren.Count - 1) / Columns + 1));
            }

            return idealSize;
         }

         return availableSize;
      }

      private void SimpleAnimate()
      {
         for (int i = 0; i < InternalChildren.Count; i++)
         {
            var child = InternalChildren[i];
            int x = i % Columns, y = i / Columns;
            Point nextPos = new Point()
            {
               X = child.DesiredSize.Width * (x + (y % 2 / 2.0)),
               Y = 0.75 * y * child.DesiredSize.Height
            };
            Animate(child, 0, 0, nextPos.X, nextPos.Y, duration, 0);
         }
      }
      #endregion
   }
}
