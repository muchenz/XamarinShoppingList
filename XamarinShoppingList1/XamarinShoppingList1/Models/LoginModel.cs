using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace XamarinShoppingList1.Models
{
    public class LoginModel:INotifyPropertyChanged
    {
        string _userName;
        public string UserName { get { return _userName; } set {
                _userName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.UserName)));
            } }
        string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Password)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
