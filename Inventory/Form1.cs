using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using com.caen.RFIDLibrary;
using MySql.Data.MySqlClient;

namespace WindowsFormsApplication1
{

      public partial class Form1 : Form 
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
           // davidreader.InventoryAbort();
           // davidreader.Disconnect();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void iniciar_Click(object sender, EventArgs e)
        {
            DBConnect DB = new DBConnect();
            CAENRFIDReader davidreader = new CAENRFIDReader();
            davidreader.CAENRFIDEvent += new CAENRFIDEventHandler(davidreader_EventHandler);
            CAENRFIDLogicalSource LS0;
            byte[] Mask = new byte[4];




            //public event CAENRFIDEventHandler davidreader_EventHandler;
            davidreader.Connect(CAENRFIDPort.CAENRFID_TCP, "192.168.10.1");

            LS0 = davidreader.GetSource("Source_0"); // punto de lectura

            LS0.SetReadCycle(0);
            //LS0.InventoryTag(Mask, 0x0, 0x0, 0x06);
            LS0.EventInventoryTag(Mask, 0x0, 0x0, 0x06); //ciclos de lectura continuos el metodo arroja un bool 

            Console.WriteLine("Inicia inventario");
            DB.Insert("ABCD", "", "", "position");

        }
  

       static void davidreader_EventHandler(object Sender, CAENRFIDEventArgs Event)
        {
            CAENRFIDNotify[] Tags = Event.Data; // Class that return a struct like { byte ID[MAX_ID_LENGTH]; short Length; char LogicalSource[MAX_LOGICAL_SOURCE_NAME]; char ReadPoint[MAX_READPOINT_NAME]; CAENRFIDProtocol Type; short RSSI; byte TID[MAX_TID_SIZE]; short TIDLen; byte XPC[XPC_LENGTH]; byte PC[PC_LENGTH];}
            
            DBConnect DB = new DBConnect();
             

            if (Tags.Length > 0)
            {

                for (int i = 0; i < Tags.Length; i++)
                {
                    String tag_id = BitConverter.ToString(Tags[i].getTagID());
                    DateTime tag_time = Tags[i].getDate();
                    string tag_source = Tags[i].getTagSource();

                    

                    if (DB.Select(tag_id) != null)// si el tag esta en la tabla
                    {

                        Console.WriteLine("entre");
                    }
                    else {

                        //Escribiendo un tag nuevo en la tabla tags 
                        DB.Insert(tag_id, tag_time.ToString(), tag_source, "position");
                    }

                    
                    
                  //hacer select para saber si el tag se encuentra guardado o no en la tabla
                   
                    //Luego hacer un insert si el return da empty  

                    Console.WriteLine(tag_id);            
                                                           
                }
            }



        }
      
      

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
        
        }

     }
 

    class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public DBConnect()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            server = "localhost";
            database = "test";
            uid = "root";
            password = "";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {

                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("No se logro conexión con el servidor");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }

        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        //Insert statement
        public void Insert(string id, string time, string source, string position)
        {
            string query = "INSERT INTO tags (id, time, position, source) VALUES ('" + id + "' , '" + time + "', '" + position + "', '" + source + "')";

            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }
        //Select statement
        public string Select(string id)
        {
            string tagreader = null;
            string query = "SELECT id FROM tags WHERE id = '" + id + "'";

            //open connection
            if (this.OpenConnection() == true)
            {

                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tagreader = reader.GetString(0);
                    //Console.WriteLine("Aqui");
                }

                reader.Close();

                //close connection
                this.CloseConnection();

            }
            return tagreader;
        }


    }



}
