using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.App.Helpers;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace QuickPad.App.Commands.Clipboard
{
    public class PasteCommand : SimpleCommand<DocumentViewModel<StorageFile, IRandomAccessStream>>, IPasteCommand<StorageFile, IRandomAccessStream>
    {
        private readonly WindowsSettingsViewModel _settings;
        private bool _canPasteText;

        public PasteCommand(WindowsSettingsViewModel settingsViewModel)
        {
            _settings = settingsViewModel;

            Task.Run(CheckClipboardStatus);

            CanExecuteEvaluator = viewModel => CanPasteText;

            Executioner = async viewModel =>
            {
                var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                    //if there is nothing to paste then don't paste anything since it will crash
                    if (!string.IsNullOrEmpty(await dataPackageView.GetTextAsync()))
                        viewModel.Document.Paste?.Invoke(await dataPackageView.GetTextAsync(), _settings.PasteTextOnly); //paste the text from the clipboard

                viewModel.OnPropertyChanged(nameof(viewModel.Text));
            };
        }

        private bool CanPasteText
        {
            get => _canPasteText;
            set
            {
                _canPasteText = value;

                InvokeCanExecuteChanged(this);
            }
        }

        private async void ClipboardStatusUpdate(object sender, object e) => await CheckClipboardStatus();

        private async Task CheckClipboardStatus()
        {
            try
            {
                var clipboardContent = await _settings.Dispatch(Windows.ApplicationModel.DataTransfer.Clipboard.GetContent);
                Windows.ApplicationModel.DataTransfer.Clipboard.ContentChanged -= ClipboardStatusUpdate;

                string GetText()
                {
                    Task<IAsyncOperation<string>> task;
                    IAsyncOperation<string> asyncOp;

                    try
                    {
                        if (clipboardContent.Contains(StandardDataFormats.Text))
                        {
                            task = Task.Run(clipboardContent.GetTextAsync);
                            task.Wait(TimeSpan.FromMinutes(1));
                            asyncOp = task.Result;

                            if (asyncOp.ErrorCode != null) throw asyncOp.ErrorCode;

                            if (asyncOp.Status == AsyncStatus.Completed)
                            {
                                var text = asyncOp?.GetResults();

                                return text;
                            }

                            if (asyncOp.Status == AsyncStatus.Canceled)
                            {
                                throw new OperationCanceledException();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _settings.Status(e.Message, TimeSpan.FromSeconds(30), Verbosity.Error);
                    }

                    return null;
                }

                string GetRtf()
                {
                    Task<IAsyncOperation<string>> task;
                    IAsyncOperation<string> asyncOp;

                    try
                    {
                        if (clipboardContent.Contains(StandardDataFormats.Rtf))
                        {
                            task = Task.Run(clipboardContent.GetRtfAsync);
                            task.Wait(TimeSpan.FromMinutes(1));
                            asyncOp = task.Result;

                            if (asyncOp.ErrorCode != null) throw asyncOp.ErrorCode;

                            if (asyncOp.Status == AsyncStatus.Completed)
                            {
                                var text = asyncOp?.GetResults();

                                return text;
                            }

                            if (asyncOp.Status == AsyncStatus.Canceled)
                            {
                                throw new OperationCanceledException();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _settings.Status(e.Message, TimeSpan.FromSeconds(30), Verbosity.Error);
                    }

                    return null;
                }

                void UpdateClipboard()
                {
                    var dataPackage = new DataPackage();
                    string text = null;
                    if (_settings.PasteTextOnly)
                    {
                        text = GetText();
                        if (text != null) dataPackage.SetText(text);
                    }
                    else
                    {
                        dataPackage = new DataPackage();
                        if (clipboardContent.Contains(StandardDataFormats.Rtf))
                        {
                            text = GetRtf();
                            if (text != null) dataPackage.SetRtf(text);
                        }
                        else if (clipboardContent.Contains(StandardDataFormats.Text))
                        {
                            text = GetText();
                            if (text != null) dataPackage.SetText(text);
                        }
                    }

                    var canPasteText = false;
                    try
                    {
                        if (text == null) return;
                        Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                        Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
                        canPasteText = clipboardContent.Contains(StandardDataFormats.Text);
                    }
                    catch (Exception e)
                    {
                        _settings.Status(e.Message, TimeSpan.FromSeconds(30), Verbosity.Error);
                    }
                    finally
                    {
                        Windows.ApplicationModel.DataTransfer.Clipboard.ContentChanged += ClipboardStatusUpdate;
                        CanPasteText = canPasteText;
                    }
                }

                _settings.Dispatch(UpdateClipboard);
            }
            catch (Exception)
            {
                CanPasteText = false;
            }
        }
    }
}