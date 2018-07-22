﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using C1.Xaml.Excel;
using Windows.Storage.Pickers;
using C1.Xaml.Bitmap;
using C1.Xaml.BarCode;
using C1.BarCode;
using Windows.Storage.Streams;
using C1.Xaml.FlexReport;
using C1.Xaml.Document.Export;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace IsbnUwp
{
  /// <summary>
  /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
  /// </summary>
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);

      this.flexgrid1.ItemsSource = Books.GetData();
    }

    private async void ExcelButton_Click(object sender, RoutedEventArgs e)
    {
      // 現在、FlexGrid に表示されている順のデータ
      var currentData = this.flexgrid1.Rows.Select(r => r.DataItem).Cast<Book>();

      // Excel データの作成
      // https://docs.grapecity.com/help/c1/uwp/uwp_excel/#Step_2_of_4-_Adding_Content_to_a_C1XLBook.html

      // 新しい Excel ワークブックを作成します
      var _book = new C1XLBook();
      // デフォルトで作成されたシートを取得
      XLSheet sheet = _book.Sheets[0];
      // シートの中身を書き込み、セルの書式を設定します
      int rowIndex = 0;
      // ヘッダー行
      sheet[rowIndex, 0].Value = "書名";
      sheet[rowIndex, 1].Value = "ISBN";
      sheet[rowIndex, 2].Value = "バーコード";
      sheet.Columns[2].Width
        = C1XLBook.PixelsToTwips(this.HiddenBarCode.ActualWidth);
      sheet[rowIndex, 3].Value = "価格";
      rowIndex++;
      // データ行
      foreach (var book in currentData)
      {
        // バーコードの画像を作る
        this.HiddenBarCode.Text = book.IsbnWithoutCheckDigit;
        C1Bitmap bitmap = new C1Bitmap();
        using (var ms = new InMemoryRandomAccessStream().AsStream())
        {
          await this.HiddenBarCode.SaveAsync(ms, ImageFormat.Png);
          bitmap.Load(ms);
        }

        // 行の高さをバーコードの画像に合わせる
        sheet.Rows[rowIndex].Height 
          = C1XLBook.PixelsToTwips(this.HiddenBarCode.ActualHeight);

        // 1行分のデータとバーコード画像をセット
        sheet[rowIndex, 0].Value = book.Title;
        sheet[rowIndex, 1].Value = book.Isbn;
        sheet[rowIndex, 2].Value = bitmap;
        sheet[rowIndex, 3].Value = book.Price;
        rowIndex++;
      }

      // Excel ファイルへの書き出し
      // https://docs.grapecity.com/help/c1/uwp/uwp_excel/#Step_3_of_4-_Saving_the_XLSX_File.html
      var picker = new FileSavePicker()
      {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary
      };
      picker.FileTypeChoices.Add("Open XML Excel ファイル", new string[]{".xlsx",});
      picker.FileTypeChoices.Add("BIFF Excel ファイル", new string[]{ ".xls",});
      picker.SuggestedFileName = "BarCodeControlSample";
      var file = await picker.PickSaveFileAsync();
      if (file != null)
      {
          var fileFormat = Path.GetExtension(file.Path).Equals(".xls") ? FileFormat.OpenXmlTemplate : FileFormat.OpenXml;
          await _book.SaveAsync(file, fileFormat);
      }
    }

    private async void PdfButton_Click(object sender, RoutedEventArgs e)
    {
      // 現在、FlexGrid に表示されている順のデータ
      var currentData = this.flexgrid1.Rows.Select(r => r.DataItem).Cast<Book>();


      // FlexReport を読み込む
      C1FlexReport rpt = new C1FlexReport();
      using (var stream = File.OpenRead("Assets/BooksReport.flxr"))
          rpt.Load(stream, "BooksReport");
      // データを連結
      rpt.DataSource.Recordset = currentData.ToList(); // IEnumerable<T>ではダメ
      // レポートを生成
      await rpt.RenderAsync();


      // 印刷する場合
      //await rpt.ShowPrintUIAsync();

      // PDF ファイルに直接保存する場合
      var picker = new FileSavePicker()
      {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary
      };
      picker.FileTypeChoices.Add("PDF ファイル", new string[] { ".pdf", });
      picker.SuggestedFileName = "BarCodeControlSample";
      var file = await picker.PickSaveFileAsync();
      if (file != null)
      {
        // 出力先となる PdfFilter オブジェクトを作成
        var filter = new PdfFilter();
        filter.StorageFile = file;
        // Windows Forms 等では、filter.FileName = file.Path;

        // ファイルへ出力
        await rpt.RenderToFilterAsync(filter);
      }
    }
  }
}
