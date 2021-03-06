using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace PixPuzzle
{
	public partial class MenuCreateViewController : UIViewController
	{
		public MenuCreateViewController (IntPtr handle)
			: base(handle)
		{
			IsFriendMatch = false;
		}

		partial void OnTakePictureButtonPressed (MonoTouch.Foundation.NSObject sender)
		{
			Camera.TakePicture (this, 
			                    (dico) => {

				UIImage img = null;

				// Get camera result
				var selectedImageObject = dico.ObjectForKey (UIImagePickerController.OriginalImage);
					
				if (selectedImageObject != null && selectedImageObject is UIImage) {
					img = selectedImageObject as UIImage;
				}
				GoToStep2 (img);
			}
			);
		}

		partial void OnLibraryButtonPressed (MonoTouch.Foundation.NSObject sender)
		{
			Camera.SelectPicture (this, 
			                      (dico) => {

				UIImage img = null;

				// Get camera result
				var selectedImageObject = dico.ObjectForKey (UIImagePickerController.OriginalImage);

				if (selectedImageObject != null && selectedImageObject is UIImage) {
					img = selectedImageObject as UIImage;
				}

				GoToStep2 (img);
			});
		}

		partial void OnDebugButtonPressed (MonoTouch.Foundation.NSObject sender)
		{
			var img = UIImage.FromFile ("testpathfromphoto3.jpg");

			GoToStep2 (img);
		}

		private void GoToStep2 (UIImage img)
		{
			var vc = this.Storyboard.InstantiateViewController ("MenuCreateStep2ViewController") as MenuCreateStep2ViewController;
			vc.InitializePuzzleCreation (img, IsFriendMatch);

			NavigationController.PushViewController (
				vc,
				true
			);
		}

		public bool IsFriendMatch {
			get;
			set;
		}
	}
}

