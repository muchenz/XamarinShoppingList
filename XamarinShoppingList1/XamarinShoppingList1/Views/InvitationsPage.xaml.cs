﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinShoppingList1.ViewModels;

namespace XamarinShoppingList1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InvitationsPage : ContentPage
    {
        public InvitationsPage(InvitationsViewModel invitationsViewModel)
        {
            InitializeComponent();
            BindingContext = invitationsViewModel;
            invitationsViewModel.Navigation = Navigation;
        }
    }
}