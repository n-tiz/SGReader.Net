using System;

namespace SGReader.Helpers
{
    internal static class PopupHelper
    {
        public static async void WaitUntil(Action action, string message)
        {
            action();

            ////let's set up a little MVVM, cos that's what the cool kids are doing:
            //var view = new SampleDialog
            //{
            //    DataContext = new SampleDialogViewModel()
            //};

            ////show the dialog
            //var result = await DialogHost.Show(view, "RootDialog", ExtendedOpenedEventHandler, ExtendedClosingEventHandler);

            ////check the result...
            //Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));

        }
    }
}