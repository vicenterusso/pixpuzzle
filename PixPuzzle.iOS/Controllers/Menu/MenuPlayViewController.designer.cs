// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace PixPuzzle
{
	[Register ("MenuPlayViewController")]
	partial class MenuPlayViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIView ViewPuzzleDetail { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView ViewPuzzleList { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ViewPuzzleList != null) {
				ViewPuzzleList.Dispose ();
				ViewPuzzleList = null;
			}

			if (ViewPuzzleDetail != null) {
				ViewPuzzleDetail.Dispose ();
				ViewPuzzleDetail = null;
			}
		}
	}
}
