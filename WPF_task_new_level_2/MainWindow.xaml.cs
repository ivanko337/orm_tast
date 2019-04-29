using System;
using System.Windows;
using ORM;

namespace WPF_task_new_level_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseData<Profile> Data { get; set; }
        private DatabaseManager Manager { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.Manager = new DatabaseManager("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=orcldb11))); User Id=db_task;Password=1234;");
            this.Data = this.Manager.GetData<Profile>();

            Loaded += MainWindow_Loaded;
        }

        private void FillListView()
        {
            this.FillListView("");
        }

        private void FillListView(string condition)
        {
            this.Data = this.Manager.GetData<Profile>();
            this.mainListView.ItemsSource = this.Data.Select(condition);
        }

        private void UpdateListView()
        {
            this.mainListView.ItemsSource = null;
            this.FillListView();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.FillListView();
        }

        private void ErrorMessage(Exception ex)
        {
            MessageBox.Show(ex.Message, "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Select_all_button_Click(object sender, RoutedEventArgs e)
        {
            this.UpdateListView();
        }

        private void Create_button_Click(object sender, RoutedEventArgs e)
        {
            FunctionWindow window = new FunctionWindow(FunctionWindowMode.Create);
            Profile newProfile = new Profile();

            window.DataContext = newProfile;

            try
            {
                window.ShowDialog();

                if(window.DialogResult == false)
                {
                    return;
                }

                newProfile.Id = this.Manager.GetNextSequenceValue(typeof(Profile));
                newProfile = this.Data.Create(newProfile);
                this.Manager.Commit(this.Data, null, true);
                this.UpdateListView();
            }
            catch (Exception ex)
            {
                this.ErrorMessage(ex);
            }
        }

        private void Redact_button_Click(object sender, RoutedEventArgs e)
        {
            if (this.mainListView.SelectedItem == null)
            {
                MessageBox.Show("Выберете элемент для редактирования", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Profile currProfile = this.mainListView.SelectedItem as Profile;
            FunctionWindow window = new FunctionWindow(FunctionWindowMode.Edit);

            try
            {
                window.DataContext = currProfile;
                window.ShowDialog();

                if (window.DialogResult == true)
                {
                    currProfile.Commit();
                    this.Manager.Commit(this.Data, currProfile);
                }
                else
                {
                    currProfile.Rollback();
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage(ex);
            }
        }

        private void Delete_buttun_Click(object sender, RoutedEventArgs e)
        {
            if (this.mainListView.SelectedItem == null)
            {
                MessageBox.Show("Выберете элемент для удаления", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Profile currProfile = this.mainListView.SelectedItem as Profile;

            try
            {
                this.Data.Delete(currProfile);
                this.Manager.Commit(this.Data, currProfile);
                this.UpdateListView();
            }
            catch (Exception ex)
            {
                this.ErrorMessage(ex);
            }
        }

        private void Select_button_Click(object sender, RoutedEventArgs e)
        {
            FunctionWindow window = new FunctionWindow(FunctionWindowMode.Select);
            string condition;

            try
            {
                window.ShowDialog();

                if(window.DialogResult == false)
                {
                    return;
                }

                condition = window.Condition;
                this.FillListView(condition);
            }
            catch (Exception ex)
            {
                this.ErrorMessage(ex);
            }
        }
    }
}
