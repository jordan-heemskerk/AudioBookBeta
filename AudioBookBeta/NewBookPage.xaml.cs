using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel; 

namespace AudioBookBeta
{
    public partial class NewBookPage : PhoneApplicationPage
    {
        public NewBookPage()
        {
            InitializeComponent();
        }

        private void CancelApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void SaveApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            string book_title = TitleTextBox.Text;
            string book_author = AuthorTextBox.Text;

            if (book_title == "Title")
            {
                //@FIXME: add error
            }

            if (book_author == "Author")
            {
                //@FIXME: add error
            }

            App.player.books.Add(new Book(book_title, book_author, 0, true));

            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}