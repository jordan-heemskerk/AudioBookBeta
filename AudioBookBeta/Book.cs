using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO.IsolatedStorage;
using System.IO;

namespace AudioBookBeta
{
    public class Book : INotifyPropertyChanged
    {
        private String _BookTitle;
        private String _Author;

        public List<string> files;

        private string InternalName { get; set; }

        


        // Declare the PropertyChanged event.
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the property that will be the source of the binding.
        public String BookTitle
        {
            get { return _BookTitle; }
            set
            {
                _BookTitle = value;
                // Call NotifyPropertyChanged when the source property 
                // is updated.
                NotifyPropertyChanged("BookTitle");
            }
        }

        public String Author
        {
            get { return _Author; }
            set
            {
                _Author = value;
                // Call NotifyPropertyChanged when the source property 
                // is updated.
                NotifyPropertyChanged("Author");
            }
        }

      

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }

        public Book(string message)
        {
            BookTitle = message;
            Author = "";
        }

        public Book(string title, string author) {
            Author = author;
            BookTitle = title;
            files = new List<string>();

            XDocument xml = new XDocument();
            var root = new XElement("Book");
            root.SetAttributeValue("current_position", "0");
            xml.Add(root);

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isoStream =
                    new IsolatedStorageFileStream("manifest.xml", FileMode.Create, isoStore))
                {
                    xml.Save(isoStream);
                }
            }
        }

        public void addFile (string filename) 
        {
            if (files == null)
            {
                
                System.Diagnostics.Debug.WriteLine("File list for a Book is null");
                return;
            }
            if (filename == null)
            {
                System.Diagnostics.Debug.WriteLine("Filename not provided to Book$addFile");
                return;
            }
            files.Add(filename);
            XDocument xml;
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("manifest.xml", FileMode.Open, isoStore))
                {
                    xml = XDocument.Load(isoStream);
                    xml.Element("Book").Add(new XElement("File", filename));
                }
            }
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isoStream =
                    new IsolatedStorageFileStream("manifest.xml", FileMode.Create, isoStore))
                {
                    xml.Save(isoStream);
                }
            }
        }
        

    }
}
