using System;
using System.Windows;
using System.Windows.Input;
using Top5.Utils;

namespace Top5.ViewModels
{
    public class TeamCommentViewModel : ViewModelBase
    {
        private string _comment;
        public string Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(); }
        }

        public string ShiftName { get; }
        public bool IsReadOnly { get; }
        public Visibility SaveButtonVisibility => IsReadOnly ? Visibility.Collapsed : Visibility.Visible;
        public bool IsSaved { get; private set; } = false;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public Action? CloseAction { get; set; }

        public TeamCommentViewModel(string shiftName, string initialComment, bool isReadOnly)
        {
            ShiftName = shiftName;
            _comment = initialComment;
            IsReadOnly = isReadOnly;

            SaveCommand = new RelayCommand(_ => { IsSaved = true; CloseAction?.Invoke(); });
            CancelCommand = new RelayCommand(_ => { IsSaved = false; CloseAction?.Invoke(); });
        }
    }
}