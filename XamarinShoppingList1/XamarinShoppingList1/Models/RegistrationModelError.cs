using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace XamarinShoppingList1.Models
{
    public class RegistrationModelError : INotifyPropertyChanged
    {
        private readonly RegistrationModel _registrationModel;

        public RegistrationModelError(RegistrationModel registrationModel)
        {
            _registrationModel = registrationModel;
            _registrationModel.PropertyChanged += _registrationModel_PropertyChanged;
                ValidateEventRise(nameof(this.IsValid));

        }

        private void _registrationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName==nameof(_registrationModel.PasswordConfirm) || e.PropertyName ==nameof(_registrationModel.Password))
            {
                _passwordError= OnValidate2(nameof(_registrationModel.Password));
                _passwordConfirmError = ValidatePassworsConfirm();

                ValidateEventRise(nameof(_registrationModel.PasswordConfirm) + "Error"); 
                ValidateEventRise(nameof(_registrationModel.Password) + "Error");
                ValidateEventRise(nameof(this.IsValid));
                return;
            }

            _userNameError = OnValidate2(e.PropertyName);

            ValidateEventRise(e.PropertyName+"Error");
            ValidateEventRise(nameof(this.IsValid));
        }


        string  ValidatePassworsConfirm()
        {

            var val = OnValidate2(nameof(_registrationModel.PasswordConfirm));
            if (string.IsNullOrEmpty(val))
                if (_registrationModel.PasswordConfirm != _registrationModel.Password)
                    return "Passwords are not equall.";


            return val;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual string OnValidate2(string propertyName)
        {
            var context = new ValidationContext(_registrationModel);
            string resultReturn = "";
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(_registrationModel, context, results, true);

            if (!isValid)
            {
                ValidationResult result = results.Where(p => p.MemberNames.First()
                                                == propertyName).FirstOrDefault();

                //ValidationResult result = results.SingleOrDefault(p =>
                //                                                p.MemberNames.Any(memberName =>
                //                                                                  memberName == propertyName));
                resultReturn = result == null ? "" : result.ErrorMessage;
            }

            return resultReturn;
        }


      

        public bool IsValid => string.IsNullOrEmpty(UserNameError) &&
            string.IsNullOrEmpty(PasswordError) && 
            string.IsNullOrEmpty(PasswordConfirmError);

        protected virtual void ValidateEventRise(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
           // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsValid)));
        }

        string _userNameError = "Novalidated";
        public string UserNameError => _userNameError;

        string _passwordError = "Novalidated";
        public string PasswordError => _passwordError;

        string _passwordConfirmError = "Novalidated";
        public string PasswordConfirmError => _passwordConfirmError;
       

    }
}
