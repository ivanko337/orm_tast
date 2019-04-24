using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using ORM;
using ORM.Attributes;

namespace WPF_task_new_level_2
{
    /// <summary>
    /// Interaction logic for FunctionWindow.xaml
    /// </summary>
    public partial class FunctionWindow : Window
    {
        public Profile Result { get; private set; }
        private FunctionWindowMode Mode { get; set; }
        public string Condition { get; private set; } = "";
        private Dictionary<PropertyInfo, TextBox> TextBoxes { get; set; } = new Dictionary<PropertyInfo, TextBox>();

        public FunctionWindow(FunctionWindowMode mode)
        {
            InitializeComponent();

            this.Mode = mode;
            string info = "";

            switch (mode)
            {
                case FunctionWindowMode.Edit:
                    info = "Редактировать";
                    break;
                case FunctionWindowMode.Create:
                    info = "Создать";
                    break;
                case FunctionWindowMode.Select:
                    info = "Выбрать";
                    break;
            }

            this.Title = info;
            this.button.Content = info;
            this.InicializeTextBoxes();
        }

        private void CreateCondition()
        {
            string res = string.Empty;

            foreach (PropertyInfo property in this.TextBoxes.Keys)
            {
                Type valueType = property.PropertyType;
                FieldNameAttribute attribute = property.GetCustomAttributes(typeof(FieldNameAttribute), true).FirstOrDefault() as FieldNameAttribute;
                string fieldName = attribute.Name;
                string value = this.TextBoxes[property].Text.Replace(" ", "");

                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                string sign = this.GetSign(ref value);

                if (valueType == typeof(DateTime))
                {
                    res += fieldName + " " + sign + "'" + DateTime.Parse(value).ToString("yyyy-MM-dd") + "' ";
                }
                else if (valueType == typeof(string))
                {
                    if (string.IsNullOrEmpty(sign))
                    {
                        res += fieldName + " LIKE '%" + value + "%'";
                    }
                }
                else
                {
                    res += fieldName + sign + value;
                }

                res += " AND ";
            }

            res = res.Remove(res.Length - 4);

            this.Condition = res;
        }

        private string GetSign(ref string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            string res = string.Empty;
            List<char> signs = new List<char>() { '<', '>', '=' };

            // кол-во знаков в строке
            int count = 0;
            foreach (char symbol in str)
            {
                if (signs.IndexOf(symbol) != -1)
                {
                    ++count;
                }
            }

            for (int i = 0; i < count; ++i)
            {
                res += str[i];
                if (signs.IndexOf(res[i]) == -1)
                {
                    res = res.Remove(i, 1);
                    continue;
                }
            }
            str = str.Remove(0, res.Length);

            return res;
        }

        private void InicializeTextBoxes()
        {
            this.TextBoxes.Add(typeof(Profile).GetProperty("Url"), this.url_textBox);
            this.TextBoxes.Add(typeof(Profile).GetProperty("RegisterDate"), this.date_textBox);
            this.TextBoxes.Add(typeof(Profile).GetProperty("Name"), this.name_textBox);
            this.TextBoxes.Add(typeof(Profile).GetProperty("Surname"), this.surname_textBox);
            this.TextBoxes.Add(typeof(Profile).GetProperty("FriendsCount"), this.friends_count_textBox);
        }

        #region CheckingMethods
        private bool CheckDate()
        {
            string date = this.date_textBox.Text;
            if (this.Mode == FunctionWindowMode.Select)
            {
                GetSign(ref date);
                if (string.IsNullOrEmpty(date))
                {
                    return true;
                }
            }
            try
            {
                DateTime.Parse(date);
                return true;
            }
            catch
            {
                MessageBox.Show("Корректно введите дату", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private bool CheckFriendsCount()
        {
            string count = this.friends_count_textBox.Text;
            if (this.Mode == FunctionWindowMode.Select)
            {
                this.GetSign(ref count);
                if (string.IsNullOrEmpty(count))
                {
                    return true;
                }
            }
            try
            {
                if (string.IsNullOrEmpty(count))
                {
                    return true;
                }

                decimal.Parse(count);
                return true;
            }
            catch
            {
                MessageBox.Show("Корректно введите число друзей", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private bool CheckProfileUrl()
        {
            if (this.Mode == FunctionWindowMode.Select)
            {
                return true;
            }

            try
            {
                Uri uri = new Uri(this.url_textBox.Text);
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Введите корректную ссылку\n" + "Формат ссылки: https://vk.com/123", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool CheckText()
        {
            if (this.Mode == FunctionWindowMode.Select)
            {
                return true;
            }

            if (string.IsNullOrEmpty(this.name_textBox.Text) || string.IsNullOrEmpty(this.surname_textBox.Text))
            {
                MessageBox.Show("Вы не ввели имя и фамилию", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool CheckAllText()
        {
            var fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            bool res = false;

            foreach (var field in fields)
            {
                string name = field.Name;
                try
                {
                    name = name.Substring(name.Length - 7);
                }
                catch
                { }
                if (name == "textBox")
                {
                    res = res || !string.IsNullOrEmpty((field.GetValue(this) as TextBox).Text);
                }
            }

            return res;
        }

        private bool CheckValues()
        {
            return this.CheckDate() && this.CheckFriendsCount() && this.CheckProfileUrl() && this.CheckText();
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!this.CheckAllText() || !this.CheckValues())
            {
                return;
            }
            this.DialogResult = true;
        }
    }
}
