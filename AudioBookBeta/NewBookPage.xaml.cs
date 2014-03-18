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
using Windows.UI.Popups;

namespace AudioBookBeta
{
    public partial class NewBookPage : PhoneApplicationPage
    {
        public NewBookPage()
        {
            InitializeComponent();
        }

        private void showMessage(string msg)
        {
            MessageBox.Show(msg, "Clarible", MessageBoxButton.OK);
        }

        private Boolean validBookTitle(string title)
        {
            IEnumerable<Book> books = App.player.books;

            for (int i = 0; i < books.Count(); i++ )
            {
                if (books.ElementAt(i).BookTitle == title) return false;
            }
            return true;
        }

        private void CancelApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void SaveApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            string book_title = TitleTextBox.Text;
            string book_author = AuthorTextBox.Text;

            if (!validBookTitle(book_title))
            {
                showMessage("You already have a book with that title. Please choose a unique title.");
                return;
            }

            if (book_title == "Title")
            {
                showMessage("Please enter a title");
                return;
            }

            if (book_author == "Author")
            {
                showMessage("Please enter an author");
                return;
            }

            App.player.books.Add(new Book(book_title, book_author, 0, true));

            NavigationService.Navigate(new Uri("/MainPage.xaml?reloadFromXml=true", UriKind.Relative));
        }

    }
}