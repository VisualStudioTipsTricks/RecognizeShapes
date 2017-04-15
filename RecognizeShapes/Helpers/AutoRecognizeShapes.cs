using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace RecognizeShapes.Helpers
{
    public class AutoRecognizeShapes
    {
        static DispatcherTimer timer;
        static InkCanvas canvas;
        static InkAnalyzer inkAnalyzer = new InkAnalyzer();

        public static event EventHandler<RecognitionEventArgs> RecognitionOccured;

        #region Timeout property
        public static int GetTimeout(DependencyObject obj)
        {
            return (int)obj.GetValue(TimeoutProperty);
        }

        public static void SetTimeout(DependencyObject obj, int value)
        {
            obj.SetValue(TimeoutProperty, value);
        }

        public static readonly DependencyProperty TimeoutProperty =
            DependencyProperty.RegisterAttached("Timeout",
            typeof(int),
            typeof(AutoRecognizeShapes), new PropertyMetadata(0, new PropertyChangedCallback(SetupTimer)));
        #endregion

        #region Canvas
        public static Canvas GetTargetCanvas(DependencyObject obj)
        {
            return (Canvas)obj.GetValue(TargetCanvasProperty);
        }

        public static void SetTargetCanvas(DependencyObject obj, Canvas value)
        {
            obj.SetValue(TargetCanvasProperty, value);
        }

        public static readonly DependencyProperty TargetCanvasProperty =
            DependencyProperty.RegisterAttached("TargetCanvas",
            typeof(Canvas),
            typeof(AutoRecognizeShapes), new PropertyMetadata(null));
        #endregion

        private static void SetupTimer(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool result = Int32.TryParse(e.NewValue.ToString(), out int milliseconds);

            if (result == true && milliseconds > 0)
            {
                canvas = d as InkCanvas;

                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(milliseconds);
                timer.Tick += Timer_Tick;

                canvas.InkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;
                canvas.InkPresenter.StrokeInput.StrokeEnded += StrokeInput_StrokeEnded;
            }
            else
            {
                throw new InvalidOperationException("Invalid value for internal timer!");
            }
        }

        private static void Timer_Tick(object sender, object e)
        {
            recognize(canvas.InkPresenter);
            timer.Stop();
        }

        private static void StrokeInput_StrokeEnded(InkStrokeInput sender, PointerEventArgs args)
        {
            timer.Start();
        }

        private static void StrokeInput_StrokeStarted(InkStrokeInput sender, PointerEventArgs args)
        {
            timer.Stop();
        }

        private async static void recognize(InkPresenter presenter)
        {
            IReadOnlyList<InkStroke> strokes = presenter.StrokeContainer.GetStrokes();

            if (strokes.Any())
            {
                inkAnalyzer.AddDataForStrokes(strokes);
                InkAnalysisResult results = await inkAnalyzer.AnalyzeAsync();

                if (results.Status == InkAnalysisStatus.Updated)
                {
                    ConvertShapes(presenter, inkAnalyzer.AnalysisRoot);
                }
            }

            inkAnalyzer.ClearDataForAllStrokes();
        }

        private static void ConvertShapes(InkPresenter presenter, InkAnalysisRoot analysisRoot)
        {
            var drawingSurface = GetTargetCanvas(canvas);
            if (drawingSurface == null) return;

            IReadOnlyList<IInkAnalysisNode> nodes = inkAnalyzer.AnalysisRoot.FindNodes(InkAnalysisNodeKind.WritingRegion);
            foreach (var item in nodes)
            {
                AddTextToCanvas(analysisRoot, drawingSurface);
            }

            IReadOnlyList<IInkAnalysisNode> drawings = inkAnalyzer.AnalysisRoot.FindNodes(InkAnalysisNodeKind.InkDrawing);
            
            if (drawings.Any())
            {
                foreach (IInkAnalysisNode drawing in drawings)
                {
                    var shape = (InkAnalysisInkDrawing)drawing;

                    if (shape.DrawingKind == InkAnalysisDrawingKind.Drawing)
                    {
                        // Omit unsupported shape
                        continue;
                    }

                    if (shape.DrawingKind == InkAnalysisDrawingKind.Circle || shape.DrawingKind == InkAnalysisDrawingKind.Ellipse)
                    {
                        // Create a Circle or Ellipse on the canvas.
                        AddEllipseToCanvas(shape, drawingSurface);
                    }
                    else
                    {
                        // Create a Polygon on the canvas.
                        AddPolygonToCanvas(shape, drawingSurface);
                    }

                    // Select the strokes that were recognized, so we can delete them.
                    // The effect is that the shape added to the canvas replaces the strokes.
                    foreach (var strokeId in shape.GetStrokeIds())
                    {
                        InkStroke stroke = presenter.StrokeContainer.GetStrokeById(strokeId);
                        stroke.Selected = true;
                    }

                    // Remove the recognized strokes from the analyzer
                    // so it won't re-analyze them.
                    // inkAnalyzer.RemoveDataForStrokes(shape.GetStrokeIds());
                }
            }

            // presenter.StrokeContainer.DeleteSelected();
            presenter.StrokeContainer.Clear();
        }

        private static void AddTextToCanvas(InkAnalysisRoot analysisRoot, Canvas drawingSurface)
        {
            //var rect = analysisRoot.BoundingRect;
            //Rectangle r = new Rectangle();
            //r.IsHitTestVisible = false;
            //r.SetValue(Canvas.LeftProperty, rect.Left);
            //r.SetValue(Canvas.TopProperty, rect.Top);
            //r.SetValue(Canvas.WidthProperty, rect.Width);
            //r.SetValue(Canvas.HeightProperty, rect.Height);
            //r.Stroke = new SolidColorBrush(canvas.InkPresenter.CopyDefaultDrawingAttributes().Color);
            //r.StrokeThickness = 1;
            //drawingSurface.Children.Add(r);

            //double rotationAngle = Math.Atan2(points[2].Y - points[0].Y, points[2].X - points[0].X);
            //double rotationAngle = Math.Atan2(analysisRoot.BoundingRect.Height, analysisRoot.BoundingRect.Width);
            //rotationAngle = -(rotationAngle * 180.0 / Math.PI);
            var attributes = canvas.InkPresenter.CopyDefaultDrawingAttributes();

            var container = new Viewbox();
            container.RenderTransformOrigin = new Point(0.5, 0.5);
            container.IsHitTestVisible = false;
            container.SetValue(Canvas.LeftProperty, analysisRoot.BoundingRect.Left);
            container.SetValue(Canvas.TopProperty, analysisRoot.BoundingRect.Top + analysisRoot.BoundingRect.Height);
            container.Width = analysisRoot.BoundingRect.Width;
            container.Height = analysisRoot.BoundingRect.Height;
            //container.RenderTransform = new RotateTransform() { Angle = rotationAngle };
            container.Child = new Border() {
                Child = new TextBlock() {
                    Text = analysisRoot.RecognizedText,
                    IsHitTestVisible = false,
                    Foreground = new SolidColorBrush(attributes.Color)
                }
            };
            drawingSurface.Children.Add(container);

            if (RecognitionOccured != null)
            {
                RecognitionOccured(null, new RecognitionEventArgs(container)
                {
                    Description = $"Text '{analysisRoot.RecognizedText}'"
                } );
            }
        }

        private static void AddEllipseToCanvas(InkAnalysisInkDrawing shape, Canvas drawingSurface)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.IsHitTestVisible = false;

            // Ellipses and circles are reported as four points
            // in clockwise orientation.
            // Points 0 and 2 are the extrema of one axis,
            // and points 1 and 3 are the extrema of the other axis.
            // See Ellipse.svg for a diagram.
            IReadOnlyList<Point> points = shape.Points;

            // Calculate the geometric center of the ellipse.
            var center = new Point((points[0].X + points[2].X) / 2.0, (points[0].Y + points[2].Y) / 2.0);

            // Calculate the length of one axis.
            ellipse.Width = Distance(points[0], points[2]);

            var compositeTransform = new CompositeTransform();
            if (shape.DrawingKind == InkAnalysisDrawingKind.Circle)
            {
                ellipse.Height = ellipse.Width;
            }
            else
            {
                // Calculate the length of the other axis.
                ellipse.Height = Distance(points[1], points[3]);

                // Calculate the amount by which the ellipse has been rotated
                // by looking at the angle our "width" axis has been rotated.
                // Since the Y coordinate is inverted, this calculates the amount
                // by which the ellipse has been rotated clockwise.
                double rotationAngle = Math.Atan2(points[2].Y - points[0].Y, points[2].X - points[0].X);

                // Convert radians to degrees.
                compositeTransform.Rotation = rotationAngle * 180.0 / Math.PI;
                compositeTransform.CenterX = ellipse.Width / 2.0;
                compositeTransform.CenterY = ellipse.Height / 2.0;
            }

            compositeTransform.TranslateX = center.X - ellipse.Width / 2.0;
            compositeTransform.TranslateY = center.Y - ellipse.Height / 2.0;

            ellipse.RenderTransform = compositeTransform;

            var attributes = canvas.InkPresenter.CopyDefaultDrawingAttributes();
            ellipse.Stroke = new SolidColorBrush(attributes.Color);
            ellipse.StrokeThickness = attributes.Size.Width;

            drawingSurface.Children.Add(ellipse);

            if (RecognitionOccured != null)
            {
                RecognitionOccured(null, new RecognitionEventArgs(ellipse)
                {
                    Description = $"Ellipse centered at {center}"
                });
            }
        }

        private static void AddPolygonToCanvas(InkAnalysisInkDrawing shape, Canvas drawingSurface)
        {
            Polygon polygon = new Polygon();
            polygon.IsHitTestVisible = false;

            foreach (var point in shape.Points)
            {
                polygon.Points.Add(point);
            }

            var attributes = canvas.InkPresenter.CopyDefaultDrawingAttributes();
            polygon.Stroke = new SolidColorBrush(attributes.Color);
            polygon.StrokeThickness = attributes.Size.Width;

            drawingSurface.Children.Add(polygon);

            if (RecognitionOccured != null)
            {
                RecognitionOccured(null, new RecognitionEventArgs(polygon)
                {
                    Description = $"Polygon with {shape.Points.Count} sides"
                });
            }
        }

        static double Distance(Point p0, Point p1)
        {
            double dX = p1.X - p0.X;
            double dY = p1.Y - p0.Y;
            return Math.Sqrt(dX * dX + dY * dY);
        }
    }
}