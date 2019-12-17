using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using FS = Google.Cloud.Firestore;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for wndFireabase.xaml
    /// </summary>
    public partial class wndFireabase : Window
    {
        public wndFireabase()
        {
            InitializeComponent();
            InitializeConnection();
        }

        private async void InitializeConnection()
        {
            await Task.Factory.StartNew(async () => {
                FirestoreDb db = FirestoreDb.Create("wingbeat-888ed");
                dgCollections.ItemsSource = (List<CollectionReference>) db.ListRootCollectionsAsync();

                // Create a document with a random ID in the "users" collection.
                CollectionReference collection = db.Collection("windows");
                ObservableCollection<FS.DocumentReference> documents = (ObservableCollection<FS.DocumentReference>)collection.ListDocumentsAsync();
                dgDocuments.ItemsSource = documents;
                if (dgDocuments.Items.Count > 0)
                {
                    Dictionary<int, List<FirestoreField>> fields = new Dictionary<int, List<FirestoreField>>();
                    var dictionaryIndex = 0;

                    Query query = collection.WhereLessThan("Version", 1.3);
                    QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
                    foreach (DocumentSnapshot queryResult in querySnapshot.Documents)
                    {
                        string deviceID = queryResult.GetValue<string>("DeviceID");
                        double version = queryResult.GetValue<double>("Version");
                        fields.Add(dictionaryIndex++, new List<FirestoreField> {
                        new FirestoreField("DeviceID", FirestoreType.@string, deviceID),
                        new FirestoreField("Version", FirestoreType.number, version)
                    });
                    }

                    dgFields.ItemsSource = fields[0];
                }
            });
        }

        #region Menu Item Click Events
        private void MnuiNewCollection_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Make a New Collection
        }

        private void MnuiNewDocument_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Make a New Document

        }

        private void MnuiNewField_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Make a New Field
        }
        #endregion
    }

    [FirestoreData]
    [DataContract]
    public class FirestoreField {
        [DataMember]
        public string Field { get; set; }
        [DataMember]
        public FirestoreType Type { get; set; }
        [DataMember]
        public dynamic Value { get; set; }

        public FirestoreField(string Field = "", FirestoreType? Type = null, dynamic Value = null) {
            if(!string.IsNullOrWhiteSpace(Field))
                this.Field = Field;

            if (Type != null && Type.HasValue)
                this.Type = Type.Value;

            if (Value != null)
                this.Value = Value;
        }
    }

    public enum FirestoreType {
        @string = 0,
        number,
        boolean,
        map,
        array,
        @null,
        timestamp,
        geopoint,
        reference
    }
}
