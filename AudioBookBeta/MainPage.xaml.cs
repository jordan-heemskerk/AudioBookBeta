using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using AudioBookBeta.Resources;
using System.Collections.ObjectModel;
using Microsoft.Phone.BackgroundAudio;

namespace AudioBookBeta
{
    public partial class MainPage : PhoneApplicationPage
    {
        
      
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Boolean found = false;
            foreach (var i in App.player.books)
            {
                if (i.BookTitle == "Select a book...") found = true;
            }
            if (!found && App.player.books.Count == 0)
            {
                App.player.books.Insert(0, new Book("Select a book..."));
            }
            this.bookPicker.ItemsSource = App.player.books;

            

            updateUI();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void AddNewBookApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewBookPage.xaml", UriKind.Relative));
        }

        private void EditBookApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/LoadFile.xaml", UriKind.Relative));
        }

        private void bookPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.player.selectedBookIndex = bookPicker.SelectedIndex;
            updateUI();
        }


        private void updateUI()
        {
            if (bookPicker.SelectedIndex > -1)
            {
                this.AuthorText.Text = App.player.books.ElementAt(bookPicker.SelectedIndex).Author;
            }
        }

       

        


        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}