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
        private int position;

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

        public int getPosition() { return position; }

        public void setPosition(int pos) {
            Boolean writeBack = false;
            if (Math.Abs(pos - position) > 5) writeBack = true;
            position = pos;
            if (writeBack)
            {
                XDocument xml;
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("manifest.xml", FileMode.Open, isoStore))
                    {
                        xml = XDocument.Load(isoStream);
                        XElement books = xml.Element("Books");
                        for (int i = 0; i < books.Elements("Book").Count(); i++)
                        {
                            if (books.Elements("Book").ElementAt(i).Attribute("title").Value == BookTitle)
                                books.Elements("Book").ElementAt(i).SetAttributeValue("current_position", pos);
                        }
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

        public Book(string title, string author, int pos, Boolean newBook) {
            Author = author;
            BookTitle = title;
            position = pos;
            files = new List<string>();
            if (!newBook) return;

            
            var root = new XElement("Book");
            root.SetAttributeValue("current_position", pos);
            root.SetAttributeValue("title", BookTitle);
            root.SetAttributeValue("author", Author);
      
            XDocument xml;
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("manifest.xml", FileMode.Open, isoStore);

                    xml = XDocument.Load(isoStream);
                    xml.Element("Books").Add(root);
                    isoStream.Close();
                }
                catch (Exception fnf)
                {
                    var true_root = new XDocument();
                    var books = new XElement("Books");
                    books.Add(root);
                    true_root.Add(books);
                        
                    xml = true_root;
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

        public void addFile (string filename, Boolean newFile) 
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
            if (!newFile) return;
            XDocument xml;
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("manifest.xml", FileMode.Open, isoStore))
                {
                    xml = XDocument.Load(isoStream);
                    XElement books = xml.Element("Books");
                    for (int i = 0; i < books.Elements("Book").Count(); i++) {
                        if (books.Elements("Book").ElementAt(i).Attribute("title").Value == BookTitle)
                            books.Elements("Book").ElementAt(i).Add(new XElement("File", filename));
                    }
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
