using System.Windows;
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class TeamCommentWindow : Window
    {
        public TeamCommentWindow()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                if (DataContext is TeamCommentViewModel vm)
                {
                    vm.CloseAction = () => this.Close();
                }
            };
        }
    }
}