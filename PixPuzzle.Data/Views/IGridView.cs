using System;
#if IOS
using System.Drawing;
#elif WINDOWS_PHONE
using Microsoft.Xna.Framework;
#endif

namespace PixPuzzle.Data
{
	/// <summary>
	/// Cross platform puzzle render interface
	/// </summary>
	public interface IGridView
	{
		void InitializeViewForDrawing ();

		void Refresh (Rectangle rect);

		void StartDraw ();

		void EndDraw ();

		void DrawGrid ();

		void DrawCellBase (Cell cell);

		void DrawPath (Cell cell,Rectangle pathRect, Point direction, CellColor color);

		void DrawLastCellIncompletePath (Cell cell,Rectangle rect, string pathValue, CellColor color);

		void DrawCellText (Cell cell, Rectangle location, string text, CellColor color);

		void SelectCell (Cell cell);

		void UnselectCell (Cell cell, bool complete, bool cancel);
	}
}

