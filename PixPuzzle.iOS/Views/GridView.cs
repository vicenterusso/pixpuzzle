using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using PixPuzzle.Data;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreText;
using MonoTouch.Foundation;

namespace PixPuzzle
{
	internal class GridViewInternal : UIView
	{
		private GridView parent;

		public GridViewInternal (GridView parent, RectangleF frame) 
			: base(frame)
		{
			this.parent = parent;

			// -- Gestures
			// ---- Double tap to delete
			UITapGestureRecognizer tapGesture = new UITapGestureRecognizer (doubleTapEvent);
			tapGesture.NumberOfTapsRequired = 2;
			this.AddGestureRecognizer (tapGesture);
		}
		#region Grid tools

		private Cell getCellFromViewCoordinates (PointF viewLocation)
		{
			int x = (int)(viewLocation.X / (float)parent.CellSize);
			int y = (int)(viewLocation.Y / (float)parent.CellSize);

			return parent.GetCell (x, y);
		}
		#endregion

		#region Events

		private void doubleTapEvent (UIGestureRecognizer sender)
		{
			if (sender.NumberOfTouches > 0) {

				PointF touchLocation = sender.LocationInView (this);

				Cell cell = getCellFromViewCoordinates (touchLocation);

				Cell[] cellToRefresh = cell.Path.Cells.ToArray ();
				parent.RemovePath (cell);

				OrderRefresh (cellToRefresh);
			}
		}

		public override void TouchesBegan (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			// Touch began: find the cell under the finger and register it as a path start
			if (touches.Count == 1) {
				UITouch touch = (UITouch)touches.AnyObject;

				PointF fingerLocation = touch.LocationInView (this);

				Console.WriteLine (fingerLocation);

				Cell cell = getCellFromViewCoordinates (fingerLocation);

				bool pathStarted = parent.StartPathCreation (cell);

				if (pathStarted) {
					this.BringSubviewToFront (((CellView)cell).View);
				}

				Cell[] cellToRefresh = cell.Path.Cells.ToArray ();
				OrderRefresh (cellToRefresh);
			}
			base.TouchesBegan (touches, evt);
		}

		public override void TouchesMoved (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			// Create a path under the finger following the grid
			if (touches.Count == 1) {

				// Valid movement
				if (parent.IsCreatingPath) {
					UITouch touch = (UITouch)touches.AnyObject;

					PointF fingerLocation = touch.LocationInView (this);

					Cell cell = getCellFromViewCoordinates (fingerLocation);

					parent.CreatePath (cell);

					Cell[] cellToRefresh = cell.Path.Cells.ToArray ();
					OrderRefresh (cellToRefresh);
				}
			}
			base.TouchesMoved (touches, evt);
		}

		public override void TouchesEnded (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			parent.EndPathCreation (false);

			base.TouchesEnded (touches, evt);
		}
		#endregion

		protected override void Dispose (bool disposing)
		{
			parent = null;
			base.Dispose (disposing);
		}
		#region Drawing

		public void OrderRefresh (Cell[] cellsToRefresh)
		{
			foreach (Cell cell in cellsToRefresh) {
				int borderStartX = GridLocationX + (BorderWidth / 2);
				int borderStartY = GridLocationY + (BorderWidth / 2);

				int x = borderStartX + cell.X * parent.CellSize;
				int y = borderStartY + cell.Y * parent.CellSize;

				SetNeedsDisplayInRect (new RectangleF(x,y,parent.CellSize,parent.CellSize));
			}
		}

		private const int BorderWidth = 4;
		private const int GridLocationX = 0;
		private const int GridLocationY = 0;
		private UIColor defaultBackgroundColor;

		public void InitializeViewForDrawing (int x, int y)
		{
			// Find the real final size and location of the frame
			this.Frame = new RectangleF (x, y
			                            , (parent.CellSize * parent.Width) + GridLocationX + BorderWidth
			                            , (parent.CellSize * parent.Height) + GridLocationY + BorderWidth
			);

			defaultBackgroundColor = UIColor.FromRGB (230, 230, 230);
			this.BackgroundColor = defaultBackgroundColor;
		}

		public override void Draw (RectangleF rect)
		{
			Console.WriteLine ("draw");
			base.Draw (rect);

			CGContext context = UIGraphics.GetCurrentContext ();

			// Context properties
			// -- Text
			context.SetTextDrawingMode (CGTextDrawingMode.Fill);
			context.TextMatrix = CGAffineTransform.MakeScale (1f, -1f);

			// Draw the bounds of the grid
			// ------------------------------------------------------------
			int borderStartX = GridLocationX + (BorderWidth / 2);
			int borderStartY = GridLocationY + (BorderWidth / 2);
			int borderEndX = borderStartX + (parent.CellSize * parent.Width) + (BorderWidth / 2);
			int borderEndY = borderStartY + (parent.CellSize * parent.Height) + (BorderWidth / 2);

			context.SetStrokeColorWithColor (UIColor.Blue.CGColor);
			context.MoveTo (borderStartX, borderStartY); //start at this point
			context.SetLineWidth (BorderWidth);

			// Draw a square
			context.AddLineToPoint (borderEndX, borderStartY); 
			context.AddLineToPoint (borderEndX, borderEndY); 
			context.AddLineToPoint (borderStartX, borderEndY); 
			context.AddLineToPoint (borderStartX, borderStartY); 

			// Effective draw
			context.StrokePath ();

			// Draw each cell
			// ------------------------------------------------------------
			for (int x=0; x<parent.Width; x++) {
				for (int y=0; y<parent.Height; y++) {

					int cellStartX = borderStartX + (x * parent.CellSize);
					int cellStartY = borderStartY + (y * parent.CellSize);

					RectangleF cellRect = new RectangleF (cellStartX, cellStartY, parent.CellSize, parent.CellSize);

					// The optimization for not redrawing every cell each display
					// ******************************************************************************************
					// Are we in the draw rect?!
					if (rect.Contains (cellRect) == false || rect.IntersectsWith (cellRect) == false) {
						continue;
					}

					// Get properties
					// ******************************************************************************************
					Cell cell = parent.GetCell (x, y);
					if (cell == null)
						continue;

					// Get properties
					bool hasPath = (cell.Path != null);
					bool isStartOrEnd = cell.IsPathStartOrEnd;
					bool isValid = (hasPath && cell.Path.IsValid);
					bool isLastCell = (hasPath && cell.Path.IsLastCell (cell));

					// Game states
					// ******************************************************************************************
					if (hasPath) {

						// Maybe we will draw some text
						bool showText = false;
						string textValue = string.Empty;
						UIColor colorUnderText = defaultBackgroundColor;

						// Path is valid?
						if (isValid) {
							// Change background color
							colorUnderText = UIColor.Blue;
							context.SetFillColor (colorUnderText.CGColor);
							context.FillRect (cellRect);

						}

						// Set the context color to the path one
						context.SetFillColor (cell.Path.Color.UIColor.CGColor);

						// Path and end or start
						if (isStartOrEnd) {

							colorUnderText = cell.Path.Color.UIColor;

							showText = true;
							textValue = cell.Path.ExpectedLength.ToString ();
							context.SelectFont ("Helvetica Neue", 16.0f, CGTextEncoding.MacRoman);

							// Draw a circle of the color
							// But reduce the circle value
							int circleReductionValue = 2;
							context.FillEllipseInRect (new RectangleF(cellStartX + circleReductionValue, cellStartY + circleReductionValue, parent.CellSize-2*circleReductionValue, parent.CellSize-2*circleReductionValue));
						}

						// Draw the path!
						// For this each cell of the path draw the cell just before them
						Cell previousCell = cell.Path.PreviousCell (cell);
						if (previousCell != null) {

							// Get the direction of the previous cell
							int previousDirectionX = previousCell.X - cell.X;
							int previousDirectionY = previousCell.Y - cell.Y;

							// If x or y is set then y or x has to be 0

							// Draw an ellipse between the two cells
							// This code is brutal
							int pathStartX;
							int pathStartY;
							int pathWidth;
							int pathHeight;

							if (previousDirectionX != 0) {
								// Get the middle X of the previous cell
								pathStartX = cellStartX + (previousDirectionX * parent.CellSize / 2);

								// Center Y in the current cell
								pathStartY = cellStartY + Math.Abs (previousDirectionX * parent.CellSize / 4);

								// Horizontal path
								pathWidth = parent.CellSize;
								pathHeight = parent.CellSize / 2;

							} else {
								// Center X in the current cell
								pathStartX = cellStartX + Math.Abs (previousDirectionY * parent.CellSize / 4);

								// Get the middle Y of the previous cell
								pathStartY = cellStartY + (previousDirectionY * parent.CellSize / 2);

								// Vertical path
								pathWidth = parent.CellSize / 2;
								pathHeight = parent.CellSize;
							}

							RectangleF pathRect = new RectangleF (
								pathStartX,
								pathStartY,
								pathWidth,
								pathHeight
							);

							// Draw in 2 parts:
							// First a rect
							context.FillRect (pathRect);

							// Then an arc to the end
							if (previousDirectionX < 0) {
								context.MoveTo (pathRect.Right, pathRect.Top);
								context.AddCurveToPoint (
									pathRect.Right + pathRect.Width / 3,
									pathRect.Top + pathRect.Height / 3,
									pathRect.Right + pathRect.Width / 3,
									pathRect.Top + 2 * pathRect.Height / 3,
									pathRect.Right,
									pathRect.Bottom
								);
							} else if (previousDirectionX > 0) {
								context.MoveTo (pathRect.Left, pathRect.Top);
								context.AddCurveToPoint (
									pathRect.Left - pathRect.Width / 3,
									pathRect.Top + pathRect.Height / 3,
									pathRect.Left - pathRect.Width / 3,
									pathRect.Top + 2 * pathRect.Height / 3,
									pathRect.Left,
									pathRect.Bottom
								);
							}
							if (previousDirectionY < 0) {
								context.MoveTo (pathRect.Left, pathRect.Bottom);
								context.AddCurveToPoint (
									pathRect.Left + pathRect.Width / 3,
									pathRect.Bottom + pathRect.Height / 3,
									pathRect.Left + 2 * pathRect.Width / 3,
									pathRect.Bottom + pathRect.Height / 3,
									pathRect.Right,
									pathRect.Bottom
								);
							} else if (previousDirectionY > 0) {
								context.MoveTo (pathRect.Left, pathRect.Top);
								context.AddCurveToPoint (
									pathRect.Left + pathRect.Width / 3,
									pathRect.Top - pathRect.Height / 3,
									pathRect.Left + 2 * pathRect.Width / 3,
									pathRect.Top - pathRect.Height / 3,
									pathRect.Right,
									pathRect.Top
								);
							}

							context.FillPath ();

							// Last cell of an incomplete path?
							if ((isLastCell == true) && (isValid == false) && (isStartOrEnd == false)) {

								// Text properties
								showText = true;
								colorUnderText = UIColor.LightGray;
								textValue = cell.Path.Length.ToString ();
								context.SelectFont ("Helvetica Neue", 12.0f, CGTextEncoding.MacRoman);

								// Draw a gray circle!
								int circleReductionValue = parent.CellSize / 8;
								context.SetFillColor (colorUnderText.CGColor);
								context.FillEllipseInRect (new RectangleF(cellStartX + circleReductionValue, cellStartY + circleReductionValue, parent.CellSize-2*circleReductionValue, parent.CellSize-2*circleReductionValue));
							}
						}

						if (showText) {

							// Get the reverse color of the background
							float sumOfComponents = 0;
							float r, g, b, a;
							colorUnderText.GetRGBA (out r, out g, out b, out a);
							sumOfComponents = r + g + b;

							UIColor textColor = UIColor.Black;
							if (sumOfComponents < 0.5f) {
								textColor = UIColor.White;
							}

							context.SetFillColor (textColor.CGColor);

							// Draw the text properly

							// Careful with the coordinates!!!
							// Remember it's a real mess because it's inverted
							context.ShowTextAtPoint (cellStartX+ parent.CellSize/3, cellStartY + 2 * parent.CellSize / 3, textValue);
						}

					} // if cell has path

					// The common part: the border
					// ******************************************************************************************
					// Start point
					context.MoveTo (cellStartX, cellStartY);

					context.SetStrokeColorWithColor (UIColor.Black.CGColor);
					context.SetLineWidth (1.0f);

					// Square for borders
					context.StrokeRect (cellRect); 
				}
			}

			context.Dispose ();
		}
		#endregion
	}
	/// <summary>
	/// Grid iOS view.
	/// </summary>
	public class GridView : Grid<CellView>
	{
		// Constants
		public const int CellSizeIphone = 32;
		public const int CellSizeIpad = 48;

		public GridView (int width, int height)
			: base(width, height, AppDelegate.UserInterfaceIdiomIsPhone ? CellSizeIphone : CellSizeIpad)
		{
			// Create the view 
			View = new GridViewInternal (this, new RectangleF (0, 0, width * CellSize, height * CellSize));

			// Create the grid and cells views
			CreateGrid ((x,y) => {
				CellView cell = new CellView(x,y, new RectangleF (x * CellSize, y * CellSize, CellSize, CellSize));

				cell.BuildView();

//				View.AddSubview(cell.View);

				return cell;
			});

		}

		internal GridViewInternal View {
			get;
			set;
		}
	}
}

