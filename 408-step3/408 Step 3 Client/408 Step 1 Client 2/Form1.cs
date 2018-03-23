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


namespace _408_Step_1_Client_2
{
    public partial class Form1 : Form
    {


        static Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Form1()
        {
            InitializeComponent();
            button3.Enabled = false;
            button4.Enabled = false;
            textBox4.Enabled = false;
            button6.Enabled = false;
            button1.Enabled = false;
            button5.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            textBox5.Enabled = false;
            richTextBox1.ReadOnly = true;
            richTextBox2.ReadOnly = true;
            richTextBox3.ReadOnly = true;
            button9.Enabled = false;
            textBox6.Enabled = false;

        }
        static List<String> senderList = new List<string>();
        static List<String> winloseList = new List<string>();
        bool isGameOn = false;

        int serverguess = 0;
        int enemyguess = 0;
        int point = 0;
        bool isget = false;


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void Receive()
        {
            bool connected = true;


            while (connected)
            {

                try
                {

                    byte[] buffer = new byte[1024];
                    int option = 0;


                    int rec = client.Receive(buffer);

                    option = BitConverter.ToInt32(buffer, 0);  // option info 
                    int len = BitConverter.ToInt32(buffer, 4);

                    if (option == 300) //incoming 300 from server =  get user list from server 
                    {

                        if (rec <= 0)
                        {
                            throw new SocketException();
                        }
                        string newmessage = Encoding.Default.GetString(buffer, 8, len);
                        richTextBox2.AppendText(newmessage);
                    }


                    else if (option == 900) // incoming 900 from server = broadcast
                    {

                        if (rec <= 0)
                        {
                            throw new SocketException();
                        }

                        string newmessage = Encoding.Default.GetString(buffer, 8, len);
                        //string newmessage = Encoding.Default.GetString(messageBuffer);
                        //newmessage = newmessage.Substring(0, newmessage.IndexOf("\0"));
                        richTextBox1.AppendText("Server: " + newmessage + "\r\n");

                    }

                    else if (option == 500) // socket close
                    {


                        if (!isGameOn)
                        {
                            isGameOn = false;
                            string name = "";
                            if (winloseList.Count > 0)
                            {
                                name = winloseList[0];
                                winloseList.Clear();
                            }
                            else
                            {
                                name = textBox5.Text;
                            }


                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                            connected = false;
                            this.Close();
                        }
                        else
                        {
                            isGameOn = false;
                            string name = "";
                            if (winloseList.Count > 0)
                            {
                                name = winloseList[0];
                                winloseList.Clear();
                            }
                            else
                            {
                                name = textBox5.Text;
                            }

                            byte[] code = BitConverter.GetBytes(540);
                            byte[] buff = new byte[25];

                            code.CopyTo(buff, 0);
                            byte[] lenbuffer = BitConverter.GetBytes(name.Length);
                            lenbuffer.CopyTo(buff, 4);
                            byte[] listbuffer = Encoding.Default.GetBytes(name);
                            listbuffer.CopyTo(buff, 8);
                            client.Send(buff);
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                            connected = false;
                            this.Close();

                        }
                    }

                    else if (option == 600) // server close 
                    {

                        richTextBox1.AppendText("Server Closed, dukkan kapandi.. \n ");
                        button3.Enabled = false;
                        button4.Enabled = false;
                        textBox4.Enabled = false;
                        button6.Enabled = false;


                        //client.Shutdown(SocketShutdown.Both);
                        // client.Close();
                        //connected = false;

                    }
                    else if (option == 445) // xx invited xx to play 
                    {

                        isGameOn = true;
                        string enemyname = Encoding.Default.GetString(buffer, 8, len);
                        senderList.Add(enemyname);
                        richTextBox3.AppendText(enemyname + "Invited you to play a game\n");
                        winloseList.Add(enemyname);
                        button7.Enabled = true;
                        button8.Enabled = true;
                        textBox5.Enabled = false;
                        button1.Enabled = false;
                    }
                    else if (option == 490) // xx declined inv
                    {
                        isGameOn = false;
                        string enemyname = Encoding.Default.GetString(buffer, 8, len);
                        richTextBox3.AppendText(enemyname + " Declined your request \n");
                        textBox5.Enabled = true;
                        button1.Enabled = true;
                    }
                    else if (option == 525) // xx accepted req
                    {
                        isGameOn = true;
                        string enemyname = Encoding.Default.GetString(buffer, 8, len);
                        richTextBox3.AppendText(enemyname + " Accepted your request \n");
                        richTextBox3.AppendText("GAME STARTED\n");
                        button1.Enabled = false;
                        textBox5.Enabled = false;
                        button5.Enabled = true;
                        textBox3.Enabled = false;
                        button9.Enabled = true;
                        textBox6.Enabled = true;


                    }
                    else if (option == 545) // xx surrendered
                    {
                        isGameOn = false;
                        string enemyname = Encoding.Default.GetString(buffer, 8, len);
                        richTextBox3.AppendText(enemyname + "Surrendered \n");
                        richTextBox3.AppendText("GAME ENDED\n");
                        richTextBox3.AppendText("YOU WON\n");
                        button1.Enabled = true;
                        textBox5.Enabled = true;
                        button5.Enabled = false;
                        point = 0;
                        button9.Enabled = false;
                        textBox6.Enabled = false;


                    }
                    else if (option == 442)
                    {
                        richTextBox3.AppendText("Name not exist please try again\n");
                        textBox5.Enabled = true;
                        button1.Enabled = true;

                    }
                    else if (option == 432)
                    {
                        richTextBox3.AppendText("Player is in game\n");
                        textBox5.Enabled = true;
                        button1.Enabled = true;

                    }
                    else if (option == 800)
                    {

                        string enemyname = Encoding.Default.GetString(buffer, 8, len);
                        if (isGameOn)
                        {
                            richTextBox3.AppendText("Player is busy\n");
                            byte[] code = BitConverter.GetBytes(800);
                            byte[] buff = new byte[25];

                            code.CopyTo(buff, 0);
                            byte[] lenbuffer = BitConverter.GetBytes(enemyname.Length);
                            lenbuffer.CopyTo(buff, 4);
                            byte[] listbuffer = Encoding.Default.GetBytes(enemyname);
                            listbuffer.CopyTo(buff, 8);
                            client.Send(buff);

                        }
                        else
                        {
                            richTextBox3.AppendText("Player is not busy invitation successfully send \n");
                            byte[] code = BitConverter.GetBytes(700);
                            byte[] buff = new byte[25];

                            code.CopyTo(buff, 0);
                            byte[] lenbuffer = BitConverter.GetBytes(enemyname.Length);
                            lenbuffer.CopyTo(buff, 4);
                            byte[] listbuffer = Encoding.Default.GetBytes(enemyname);
                            listbuffer.CopyTo(buff, 8);
                            client.Send(buff);



                        }
                    }
                    else if (option == 801)
                    {

                        isGameOn = false;
                        richTextBox3.AppendText("Player is in busy\n");
                        textBox5.Enabled = true;
                        button1.Enabled = true;

                    }
                    else if (option == 799)
                    {
                        richTextBox3.AppendText("Enemy guessed a number your turn");
                        string enemyname = Encoding.Default.GetString(buffer, 8, len);
                        enemyguess = BitConverter.ToInt32(buffer, 8 + enemyname.Length);
                        serverguess = BitConverter.ToInt32(buffer, 12 + enemyname.Length);
                        isget = true;
                        isGameOn = true;
                        button9.Enabled = true;
                        textBox6.Enabled = true;

                    }
                    else if (option == 810)
                    {
                        point++;
                        if (point == 2)
                        {
                            byte[] code = BitConverter.GetBytes(814);
                            byte[] buff = new byte[25];
                            string name = textBox5.Text;
                            code.CopyTo(buff, 0);
                            byte[] lenbuffer = BitConverter.GetBytes(name.Length);
                            lenbuffer.CopyTo(buff, 4);
                            byte[] listbuffer = Encoding.Default.GetBytes(name);
                            listbuffer.CopyTo(buff, 8);
                            client.Send(buff);
                            richTextBox3.AppendText("You won game\n");
                            point = 0;
                            button9.Enabled = false;
                            textBox6.Enabled = false;
                            isget = false;
                            isGameOn = false;
                            button1.Enabled = true;
                            textBox5.Enabled = true;
                            button5.Enabled = false;

                        }
                        else
                        {
                            richTextBox3.AppendText("You won round\n");
                            isget = false;
                            button9.Enabled = true;
                            textBox6.Enabled = true;
                        }



                    }
                    else if (option == 811)
                    {
                        richTextBox3.AppendText("Round lost \n");
                        isget = false;
                        button9.Enabled = true;
                        textBox6.Enabled = true;
                    }
                    else if (option == 812)
                    {

                        richTextBox3.AppendText("Draw round\n");
                        isget = false;
                        button9.Enabled = true;
                        textBox6.Enabled = true;

                    }
                    else if (option == 813)
                    {
                        richTextBox3.AppendText("You lost game\n");
                        isget = false;
                        point = 0;
                        button9.Enabled = false;
                        textBox6.Enabled = false;
                        isGameOn = false;
                        button1.Enabled = true;
                        textBox5.Enabled = true;
                        button5.Enabled = false;
                        button9.Enabled = false;
                        textBox6.Enabled = false;
                    }
                    else if (option == 815)
                    {
                        isget = false;
                        richTextBox3.AppendText("You lost game\n");
                        point = 0;
                        button9.Enabled = false;
                        textBox6.Enabled = false;
                        isGameOn = false;
                        button1.Enabled = true;
                        textBox5.Enabled = true;
                        button5.Enabled = false;
                        button9.Enabled = false;
                        textBox6.Enabled = false;
                    }

                }
                catch
                {
                    richTextBox1.AppendText("Server is offline!\n");
                    client.Close();
                    connected = false;

                }
            }

        }


        private void button4_Click(object sender, EventArgs e)
        {

            richTextBox1.AppendText("Me :");
            richTextBox1.AppendText(textBox4.Text);
            richTextBox1.AppendText("\n");

            string message = textBox4.Text;

            byte[] code = BitConverter.GetBytes(200);
            byte[] buffer = new byte[1024];

            code.CopyTo(buffer, 0);

            byte[] len = BitConverter.GetBytes(message.Length);
            len.CopyTo(buffer, 4);
            byte[] buffer2 = Encoding.Default.GetBytes(message);
            buffer2.CopyTo(buffer, 8);

            client.Send(buffer);


        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e) // connect
        {
            string serverIP;
            int serverPort;

            try
            {


                serverIP = textBox2.Text;
                //note: if you ar1e testing on the same PC, you may try 127.0.0.1 as server's IP. It should work.
                serverPort = Convert.ToInt32(textBox3.Text);




                if (!client.Connected)
                    client.Connect(serverIP, serverPort);
                string lowername = textBox1.Text.ToLower();
                client.Send(Encoding.ASCII.GetBytes(lowername));
                if (Check_UserName())
                {
                    richTextBox1.AppendText("Connected!\n");
                    Thread thrReceive;
                    thrReceive = new Thread(new ThreadStart(Receive));
                    thrReceive.IsBackground = true;
                    thrReceive.Start();
                    button2.BackColor = Color.Green;
                    button2.Enabled = false;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    textBox3.Enabled = false;
                    textBox2.Enabled = false;
                    textBox1.Enabled = false;
                    textBox4.Enabled = true;
                    button6.Enabled = true;
                    button1.Enabled = true;
                    textBox5.Enabled = true;
                }
                else
                {
                    richTextBox1.AppendText(textBox1.Text + " already exists!\n");
                    button2.BackColor = Color.Red;
                }

                // a function to ask for user input until the exit condition is reached.

            }
            catch
            {
                return;
            }
        } // connect


        private bool Check_UserName()
        {
            try
            {
                byte[] buffer = new byte[5];

                int val = 0; ;

                client.Receive(buffer);

                val = BitConverter.ToInt32(buffer, 0);

                return val == 404 ? true : false;
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("Connection has been terminated...\n" + ex.ToString());
                return false;
            }
        }




        private void textBox2_TextChanged(object sender, EventArgs e) // ip
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e) // port 
        {

        }



        private void button6_Click(object sender, EventArgs e)//exit
        {


            byte[] disconnectbuffer = BitConverter.GetBytes(500);
            client.Send(disconnectbuffer);
            richTextBox2.AppendText("Disconnected \n ");

        }

        private void button3_Click(object sender, EventArgs e)  // get user list 
        {

            richTextBox2.ResetText();
            byte[] getListBit = BitConverter.GetBytes(300);
            client.Send(getListBit);

            richTextBox1.AppendText("User List requested: \n ");
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e) //listbox
        {

        }

        private void richTextBox3_TextChanged(object sender, EventArgs e)
        {

        }


        private void button1_Click(object sender, EventArgs e) // send
        {


            isGameOn = true;
            textBox5.Enabled = false;
            button1.Enabled = false;

            string message = textBox5.Text;
            richTextBox3.AppendText("Invitation sent\n");

            byte[] code = BitConverter.GetBytes(440);
            byte[] buffer = new byte[50];

            code.CopyTo(buffer, 0);

            byte[] len = BitConverter.GetBytes(message.Length);
            len.CopyTo(buffer, 4);
            byte[] buffer2 = Encoding.Default.GetBytes(message);
            buffer2.CopyTo(buffer, 8);
            client.Send(buffer);
        }

        private void button7_Click(object sender, EventArgs e) //accept
        {
            isGameOn = true;
            button1.Enabled = false;
            button5.Enabled = true;
            button7.Enabled = false;
            button8.Enabled = false;
            textBox5.Enabled = false;
            button9.Enabled = true;
            textBox6.Enabled = true;




            string name = senderList[0];
            senderList.Clear();

            byte[] code = BitConverter.GetBytes(520);
            byte[] buff = new byte[25];

            code.CopyTo(buff, 0);
            byte[] lenbuffer = BitConverter.GetBytes(name.Length);
            lenbuffer.CopyTo(buff, 4);
            byte[] listbuffer = Encoding.Default.GetBytes(name);
            listbuffer.CopyTo(buff, 8);
            client.Send(buff);

            richTextBox3.AppendText("GAME STARTED\n");
            textBox5.ResetText();
            textBox5.AppendText(name);


        }

        private void button8_Click(object sender, EventArgs e) //reject
        {
            button7.Enabled = false;
            button8.Enabled = false;
            textBox5.Enabled = true;
            button1.Enabled = true;
            isGameOn = false;

            string name = senderList[0];
            senderList.Clear();


            byte[] code = BitConverter.GetBytes(480);

            byte[] rejectbuffer = new byte[20];

            code.CopyTo(rejectbuffer, 0);
            byte[] lenbuffer = BitConverter.GetBytes(name.Length);
            lenbuffer.CopyTo(rejectbuffer, 4);
            byte[] listbuffer = Encoding.Default.GetBytes(name);
            listbuffer.CopyTo(rejectbuffer, 8);
            client.Send(rejectbuffer);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            isGameOn = false;
            string name = "";
            if (winloseList.Count > 0)
            {
                name = winloseList[0];
                winloseList.Clear();
            }
            else
            {
                name = textBox5.Text;
            }

            byte[] code = BitConverter.GetBytes(540);
            byte[] buff = new byte[25];

            code.CopyTo(buff, 0);
            byte[] lenbuffer = BitConverter.GetBytes(name.Length);
            lenbuffer.CopyTo(buff, 4);
            byte[] listbuffer = Encoding.Default.GetBytes(name);
            listbuffer.CopyTo(buff, 8);
            client.Send(buff);

            richTextBox3.AppendText("GAME OVER \n");
            richTextBox3.AppendText("Winner is: " + name + " \n");

            button1.Enabled = true;
            textBox5.Enabled = true;
            button5.Enabled = false;
            point = 0;
            button9.Enabled = false;
            textBox6.Enabled = false;

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            isGameOn = true;

            if (isget)
            {

                int myguess = Convert.ToInt32(textBox6.Text);

                int myresult = Math.Abs(serverguess - myguess);
                int enemyresult = Math.Abs(serverguess - enemyguess);

                if (myresult > enemyresult) // enemy win
                {
                    byte[] code = BitConverter.GetBytes(810);
                    byte[] buff = new byte[25];
                    string name = textBox5.Text;
                    code.CopyTo(buff, 0);
                    byte[] lenbuffer = BitConverter.GetBytes(name.Length);
                    lenbuffer.CopyTo(buff, 4);
                    byte[] listbuffer = Encoding.Default.GetBytes(name);
                    listbuffer.CopyTo(buff, 8);
                    client.Send(buff);

                    isget = false;
                    richTextBox3.AppendText("Enemy won round \n");
                    button9.Enabled = true;
                    textBox6.Enabled = true;
                }
                else if (enemyresult > myresult) // me win
                {
                    point++;



                    if (point == 2)
                    {
                        byte[] code = BitConverter.GetBytes(816);
                        byte[] buff = new byte[25];
                        string name = textBox5.Text;
                        code.CopyTo(buff, 0);
                        byte[] lenbuffer = BitConverter.GetBytes(name.Length);
                        lenbuffer.CopyTo(buff, 4);
                        byte[] listbuffer = Encoding.Default.GetBytes(name);
                        listbuffer.CopyTo(buff, 8);
                        client.Send(buff);

                        button9.Enabled = false;
                        textBox6.Enabled = false;
                        isget = false;
                        isGameOn = false;
                        richTextBox3.AppendText("You won game \n");
                        button1.Enabled = true;
                        textBox5.Enabled = true;
                        button5.Enabled = false;
                        point = 0;
                    }
                    else
                    {
                        isGameOn = true;
                        byte[] code = BitConverter.GetBytes(811);
                        byte[] buff = new byte[25];
                        string name = textBox5.Text;
                        code.CopyTo(buff, 0);
                        byte[] lenbuffer = BitConverter.GetBytes(name.Length);
                        lenbuffer.CopyTo(buff, 4);
                        byte[] listbuffer = Encoding.Default.GetBytes(name);
                        listbuffer.CopyTo(buff, 8);
                        client.Send(buff);

                        richTextBox3.AppendText("You won round \n");
                        isget = false;
                        button9.Enabled = true;
                        textBox6.Enabled = true;

                    }
                }
                else if (enemyresult == myresult) // equal
                {

                  //point++;
                    isGameOn = true;
                    byte[] code = BitConverter.GetBytes(812);
                    byte[] buff = new byte[25];
                    string name = textBox5.Text;
                    code.CopyTo(buff, 0);
                    byte[] lenbuffer = BitConverter.GetBytes(name.Length);
                    lenbuffer.CopyTo(buff, 4);
                    byte[] listbuffer = Encoding.Default.GetBytes(name);
                    listbuffer.CopyTo(buff, 8);
                    client.Send(buff);

                    richTextBox3.AppendText("Round draw \n");
                    isget = false;
                    button9.Enabled = true;
                    textBox6.Enabled = true;

                }
            }

            else
            {
                isGameOn = true;
                //textBox5.Enabled = false;
                //button1.Enabled = false;
                string enemyname = textBox5.Text;
                string message = textBox6.Text;
                richTextBox3.AppendText("Guess sent \n");
                button9.Enabled = false;
                textBox6.Enabled = false;

                byte[] code = BitConverter.GetBytes(789);
                byte[] buffer = new byte[1024];

                code.CopyTo(buffer, 0);

                byte[] leng = BitConverter.GetBytes(enemyname.Length);
                leng.CopyTo(buffer, 4);
                byte[] buffer3 = Encoding.Default.GetBytes(enemyname);
                buffer3.CopyTo(buffer, 8);
                int integer1 = Convert.ToInt32(message);
                byte[] len = BitConverter.GetBytes(integer1);
                len.CopyTo(buffer, 8 + enemyname.Length);
                client.Send(buffer);

            }


        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }







    }
}
