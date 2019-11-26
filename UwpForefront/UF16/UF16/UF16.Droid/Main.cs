using Android.Runtime;
using Com.Nostra13.Universalimageloader.Core;
using System;
using Windows.UI.Xaml.Media;

namespace UF16.Droid
{
  [global::Android.App.ApplicationAttribute(
    Label = "@string/ApplicationName",
    LargeHeap = true,
    HardwareAccelerated = true,
    Theme = "@style/AppTheme"
  )]
  public class Application : Windows.UI.Xaml.NativeApplication
  {
    public Application(IntPtr javaReference, JniHandleOwnership transfer)
      : base(new App(), javaReference, transfer)
    {
      SQLiteSample.MovieDatabase.InitForAndroid();

      ConfigureUniversalImageLoader();
    }

    private void ConfigureUniversalImageLoader()
    {
      // Create global configuration and initialize ImageLoader with this config
      ImageLoaderConfiguration config = new ImageLoaderConfiguration
        .Builder(Context)
        .Build();

      ImageLoader.Instance.Init(config);

      ImageSource.DefaultImageLoader = ImageLoader.Instance.LoadImageAsync;
    }
  }
}
