using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _408_Step_1_Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            richTextBox1.ReadOnly = true;
            richTextBox2.ReadOnly = true;
        }

        static bool listening;
        static bool terminating = false;
        static bool accept = true;
        static Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static List<Socket> socketList = new List<Socket>();
        static List<String> nameList = new List<string>();
        static List<String> gameList = new List<string>();
        static List<int> pointlist = new List<int>();
        static List<int> guesslist = new List<int>();

        private void Accept()
        {
            while (accept)
            {
                try
                {
                    socketList.Add(server.Accept());
                    //socketList[0].SendTo
                    Thread thrReceive;
                    thrReceive = new Thread(new ThreadStart(Receive));
                    thrReceive.Start();
                }
                catch
                {
                    if (terminating)
                        accept = false;
                }
            }
        }


        private void Redirect(int i, string username, int num)
        {
            Socket a = socketList[i];

            byte[] code = BitConverter.GetBytes(num);
            byte[] requestbuffer = new byte[1024];

            code.CopyTo(requestbuffer, 0);

            byte[] lenbuffer = BitConverter.GetBytes(username.Length);
            lenbuffer.CopyTo(requestbuffer, 4);

            byte[] listbuffer = Encoding.Default.GetBytes(username);
            listbuffer.CopyTo(requestbuffer, 8);
            //richTextBox1.AppendText("redirected"); // do we need to add which player ?
            a.Send(requestbuffer);
        }

        private void Redirect2(int i, string username, int num, int myguess, int b)
        {
            Socket a = socketList[i];

            byte[] code = BitConverter.GetBytes(num);
            byte[] buffer6 = new byte[1024];
            code.CopyTo(buffer6, 0);

            byte[] len2 = BitConverter.GetBytes(username.Length);
            len2.CopyTo(buffer6, 4);

            byte[] buffer2 = Encoding.Default.GetBytes(username);
            buffer2.CopyTo(buffer6, 8);

            byte[] leng = BitConverter.GetBytes(myguess);
            leng.CopyTo(buffer6, 8 + username.Length);

            byte[] lengo = BitConverter.GetBytes(b);
            lengo.CopyTo(buffer6, 12 + username.Length);
            a.Send(buffer6);
        }


        private void Receive()
        {

            bool connected = true;
            Socket n = socketList[socketList.Count - 1];
            bool isNameExist = false;
            string thisClient = "";
            while (connected)
            {

                try
                {


                    if (!isNameExist)
                    {

                        Byte[] nameBuffer = new byte[20];


                        int rec = n.Receive(nameBuffer);

                        if (rec <= 0)
                        {
                            throw new SocketException();
                        }


                        string nick = Encoding.ASCII.GetString(nameBuffer);
                        nick = nick.Substring(0, nick.IndexOf("\0"));

                        if (nameList.Contains(nick))
                        {

                            byte[] exist = BitConverter.GetBytes(505);
                            n.Send(exist);
                            //plog.AppendText("This username already exists\n");

                        }
                        else
                        {
                            byte[] notexist = BitConverter.GetBytes(404);
                            n.Send(notexist);
                            richTextBox1.AppendText(nick + " connected\n");

                            nameList.Add(nick);
                            pointlist.Add(0);

                            isNameExist = true;
                            thisClient = nick;

                        }

                    }
                    else
                    {

                        byte[] buffer = new byte[64];
                        int option = 0;
                        try
                        {

                            int rec = n.Receive(buffer);

                            option = BitConverter.ToInt32(buffer, 0);  // option info 
                            int len = BitConverter.ToInt32(buffer, 4);
                            if (option == 200) // incoming message from a client
                            {
                                if (rec <= 0)
                                {
                                    throw new SocketException();
                                }
                                string newmessage = Encoding.Default.GetString(buffer, 8, len);
                                string username = thisClient;
                                if (richTextBox1.TextLength > 0)
                                {

                                    richTextBox1.AppendText(thisClient + ": " + newmessage + "\n");
                                }
                                else
                                {

                                    richTextBox1.AppendText(thisClient + ": " + newmessage + "\n"); // do we need to keep this ????
                                }

                            }
                            else if (option == 300)
                            {

                                string nameliststr = "";
                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    string s = pointlist[i].ToString();


                                    nameliststr = nameliststr + nameList[i] + " " + s + "\n";

                                }
                                byte[] code = BitConverter.GetBytes(300);
                                byte[] namelistbuffer = new byte[1024];

                                code.CopyTo(namelistbuffer, 0);

                                byte[] lenbuffer = BitConverter.GetBytes(nameliststr.Length);
                                lenbuffer.CopyTo(namelistbuffer, 4);
                                byte[] listbuffer = Encoding.Default.GetBytes(nameliststr);
                                listbuffer.CopyTo(namelistbuffer, 8);
                                richTextBox1.AppendText("A player is requested the user list!\n"); // do we need to add which player ?
                                n.Send(namelistbuffer);


                            }
                            else if (option == 500) //disconnect tag
                            {


                                if (rec <= 0)
                                {
                                    throw new SocketException();
                                }


                                for (int i = 0; i < socketList.Count; i++)
                                {
                                    if (socketList[i] == n)
                                    {
                                        richTextBox1.AppendText(nameList[i] + " has disconnected from server!\n");
                                        socketList.RemoveAt(i);
                                        nameList.RemoveAt(i);
                                        pointlist.RemoveAt(i);
                                        break;
                                    }
                                }


                                byte[] disconnect = BitConverter.GetBytes(500);
                                n.Send(disconnect);



                            }

                            else if (option == 440)  // reciv invite req
                            {
                                string enemyname = Encoding.Default.GetString(buffer, 8, len);


                                if (rec <= 0)
                                {
                                    throw new SocketException();
                                }

                                string username = thisClient;

                                if (richTextBox1.TextLength > 0)
                                {

                                    richTextBox1.AppendText(thisClient + " send a challange to  " + enemyname + "\n");
                                }
                                else
                                {


                                    richTextBox1.AppendText(thisClient + " send a challange to  " + enemyname + "\n"); // do we need to keep this ????
                                }

                                int count = 0;
                                for (int i = 0; i < nameList.Count; i++)
                                {

                                    if (nameList[i] == enemyname)
                                    {
                                        count = 0;
                                        Redirect(i, username, 800);

                                    }
                                    else
                                    {
                                        count++;

                                    }

                                    if (count == nameList.Count)
                                    {
                                        richTextBox1.AppendText(enemyname + " is not exist\n");

                                        byte[] code = BitConverter.GetBytes(442);// not exist co e
                                        n.Send(code);
                                    }
                                }
                            }
                            else if (option == 480)
                            {
                                if (rec <= 0)
                                {
                                    throw new SocketException();
                                }

                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                richTextBox1.AppendText("Invitation from " + enemyname + " rejected\n");
                     
                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == enemyname)
                                    {

                                        Redirect(i, thisClient, 490);
                                        //n = socketList[i];

                                    }
                                }
                            }

                            else if (option == 520)
                            {

                                if (rec <= 0)
                                {
                                    throw new SocketException();
                                }

                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                richTextBox1.AppendText("Invitation from " + enemyname + " accepted\n");


                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == enemyname)
                                    {


                                        Redirect(i, thisClient, 525);
                                        //n = socketList[i];
                                    }
                                }
                            }

                            else if (option == 800)
                            {

                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == enemyname)
                                    {


                                        Redirect(i, thisClient, 801);
                                        //n = socketList[i];

                                    }
                                }
                            }

                            else if (option == 700)
                            {
                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                byte[] code = BitConverter.GetBytes(445);
                                byte[] buffer5 = new byte[1024];

                                code.CopyTo(buffer5, 0);

                                byte[] lenbuffer = BitConverter.GetBytes(enemyname.Length);
                                lenbuffer.CopyTo(buffer5, 4);

                                byte[] listbuffer = Encoding.Default.GetBytes(enemyname);
                                listbuffer.CopyTo(buffer5, 8);
                                n.Send(buffer5);

                            }
                            else if (option == 789)
                            {
                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                int myguess = BitConverter.ToInt32(buffer, 8 + len);
                                Random rnd = new Random();
                                int randint = rnd.Next(1, 101);
                                richTextBox1.AppendText("\n rand= " + randint + "\n");


                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == enemyname)
                                    {
                                        Redirect2(i, thisClient, 799, myguess, randint);
                                        //n = socketList[i];
                                    }
                                }
                            }


                            else if (option == 540) // one surrendered
                            {
                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                richTextBox1.AppendText("winner is " + enemyname + "\n");

                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == enemyname)
                                    {

                                        pointlist[i]++;
                                        Redirect(i, thisClient, 545);
                                        //n = socketList[i];

                                    }


                                }

                            }

                            else if (option == 810)
                            {


                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                richTextBox1.AppendText(enemyname + " won " + thisClient + " lost round\n");
                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == enemyname)
                                    {
                                        Redirect(i, thisClient, 810);
                                        //n = socketList[i];

                                    }
                                }

                            }
                            
                            else if (option == 811)
                            {
                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                richTextBox1.AppendText(thisClient + " won " + enemyname + " round\n");
                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == enemyname)
                                    {

                                        Redirect(i, thisClient, 811);
                                        //n = socketList[i];

                                    }
                                }
                            }

                            else if (option == 812)
                            {

                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == enemyname)
                                    {
                                        richTextBox1.AppendText(thisClient + " vs " + enemyname + " round draw\n");
                                        Redirect(i, thisClient, 812);
                                        //n = socketList[i];

                                    }
                                }

                            }

                            else if (option == 816)
                            {

                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == thisClient)
                                    {
                                        pointlist[i]++;
                                        //n = socketList[i];

                                    }
                                }


                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                richTextBox1.AppendText(thisClient + " won " + enemyname + " lost game\n");
                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == enemyname)
                                    {
                                        Redirect(i, thisClient, 813);
                                        //n = socketList[i];

                                    }
                                }

                            }



                            else if (option == 813)
                            {

                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == enemyname)
                                    {

                                        Redirect(i, thisClient, 816);
                                        //n = socketList[i];

                                    }
                                }

                            }
                            else if (option == 814) // sender win receiver lost
                            {


                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == thisClient)
                                    {
                                        pointlist[i]++;
                                        //n = socketList[i];

                                    }
                                }

                                string enemyname = Encoding.Default.GetString(buffer, 8, len);
                                richTextBox1.AppendText(thisClient+ " won " +enemyname +" lost game\n");
                                for (int i = 0; i < nameList.Count; i++)
                                {
                                    if (nameList[i] == enemyname)
                                    {

                                        Redirect(i, thisClient, 815);
                                        //n = socketList[i];

                                    }
                                }

                            }

                        }
                        catch
                        { } // catchlememiz gerekli mi ??? ne catchlicez 


                    }


                }
                catch
                {
                    if (!terminating)
                        n.Close();
                    socketList.Remove(n);
                    connected = false;
                }
            }

        }


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e) // listen
        {
            int serverPort;
            Thread thrAccept;

            //this port will be used by clients to connect

            serverPort = Convert.ToInt32(textBox1.Text);


            try
            {
                server.Bind(new IPEndPoint(IPAddress.Any, serverPort));
                richTextBox1.AppendText("Now Listening clients from Port : ");
                string strport = serverPort.ToString();
                richTextBox1.AppendText(strport);
                richTextBox1.AppendText("\n");

                server.Listen(3); //the parameter here is maximum length of the pending connections queue
                thrAccept = new Thread(new ThreadStart(Accept));
                thrAccept.Start();
                listening = true;


                textBox2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                Listen.Enabled = false;
                textBox1.Enabled = false;


            }
            catch
            {

                richTextBox1.AppendText("Cannot create a server");

            }


        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }



        private void richTextBox3_TextChanged(object sender, EventArgs e)
        {

        }


        //this function broadcasts the given message via the sockets
        private void BroadCastMessage(string message)
        {
            byte[] buffer = Encoding.Default.GetBytes(message);

            //broadcast the message to all clients
            foreach (Socket s in socketList)
            {
                s.Send(buffer);
            }

            richTextBox1.AppendText("Your message \" " + message + " \" has been broadcasted.\n");
        }


        private void button4_Click(object sender, EventArgs e)
        {
            string message;
            message = textBox2.Text;
            byte[] code = BitConverter.GetBytes(900);
            byte[] buffer = new byte[1024];

            try
            {

                code.CopyTo(buffer, 0);

                byte[] len = BitConverter.GetBytes(message.Length);
                len.CopyTo(buffer, 4);
                byte[] buffer2 = Encoding.Default.GetBytes(message);
                buffer2.CopyTo(buffer, 8);

                //broadcast the message to all clients
                foreach (Socket s in socketList)
                {
                    s.Send(buffer);
                }

                richTextBox1.AppendText(" Broadcasted: \" " + message + " \" \n");


            }
            catch
            {
                richTextBox1.AppendText("could not broadcast");
                terminating = true;
                server.Close();
            }
        }


        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {


            string nameliststr = "";
            for (int i = 0; i < nameList.Count; i++)
            {
                string s = pointlist[i].ToString();
                nameliststr = nameliststr + nameList[i] + " " + s + "\n";
            }

            richTextBox2.AppendText(nameliststr);

        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            byte[] code = BitConverter.GetBytes(600); //client sent terminating
            byte[] buffer = new byte[50];
            code.CopyTo(buffer, 0);


            //broadcast the message to all clients
            foreach (Socket s in socketList)
            {
                s.Send(buffer);
            }

            
            this.Close();
           

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
