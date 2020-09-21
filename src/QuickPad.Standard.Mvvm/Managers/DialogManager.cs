using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.Views;
using System;

namespace QuickPad.Mvvm.Managers
{
    public class DialogManager
    {
        private readonly IServiceProvider _serviceProvider;

        public DialogManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IDialogView CurrentDialogView { get; private set; }

        public (bool, TDialogView) RequestDialog<TDialogView>()
            where TDialogView : IDialogView
        {
            if (CurrentDialogView != null)
            {
                if (CurrentDialogView is TDialogView typedDialogView)
                {
                    return (true, typedDialogView);
                }

                return (false, default);
            }

            var dialog = _serviceProvider.GetService<TDialogView>();

            if (dialog == null) return (false, default);

            dialog.Closed += () =>
            {
                DialogClosed?.Invoke(CurrentDialogView);
                CurrentDialogView = null;
            };

            CurrentDialogView = dialog;

            return (true, dialog);
        }

        public event Action<IDialogView> DialogClosed;
    }
}