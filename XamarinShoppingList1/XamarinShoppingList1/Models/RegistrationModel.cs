using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using XamarinShoppingList1.Helpers;

namespace XamarinShoppingList1.Models
{
    public class RegistrationModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
               

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string UserName
        {
            get { return _userName; }
            set
            { 
                _userName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.UserName)));               

            }
        }
        string _userName;
    

        [MinLength(6, ErrorMessage = "Minimal lenght is 6")]
        [MaxLength(50)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Password)));               

            }
        }
        string _password;     


       // [Compare(nameof(RegistrationModel.Password), ErrorMessage = "Passwords are not equall.")]
        [DataType(DataType.Password)]
        [Required]
        [Display(Name = "PasswordConfirm")]

        public string PasswordConfirm
        {
            get { return _passwordConfirm; }
            set
            {
                _passwordConfirm = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.PasswordConfirm)));
             

            }
        }
        string _passwordConfirm;
       

    }
}
