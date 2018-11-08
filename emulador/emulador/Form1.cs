﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyClasses;
using System.IO.Ports;
using System.Threading;

namespace emulador
{
    public partial class Emulador : Form
    {
        ParameterizedThreadStart delSerial1 = new ParameterizedThreadStart(serialSensor);
        ParameterizedThreadStart delSerial2 = new ParameterizedThreadStart(serialAtuador);
        Thread ThreadSerial1;
        Thread ThreadSerial2;
        public static bool sendData1;
        public static bool sendData2;
        public static bool valve;
        public static bool close1;
        public static bool close2;

        public static double level = 10000;

        public Vazao vz;
        public void enable(object ObjSerial)
        {
            SerialPort SP = (SerialPort)ObjSerial;
            try
            {
                SP.Open();
            }
            catch
            {
                MessageBox.Show("Erro ao iniciar a conexão");
                return;
            }
            this.bt_iniciar.Text = "PARAR";
        }

        public void disable(object ObjSerial)
        {
            SerialPort SP = (SerialPort)ObjSerial;
            try
            {
                SP.WriteLine("EOT");
                SP.Close();
                this.bt_iniciar.Text = "INICIAR";
                this.med_lb.Text = "";
            }
            catch
            {
                MessageBox.Show("Falha ao desligar o sensor");
            }
        }

        public Emulador()
        {
            InitializeComponent();
            vz = new Vazao(0);
        }

        private static void serialSensor(object obj)
        {
            SerialPort sp = (SerialPort)obj;

            while (sp.IsOpen)
            {
                if (sendData1)
                {
                    Thread.Sleep(2);
                    sp.WriteLine(level.ToString());
                    sendData1 = false;
                }

                if (close1)
                {
                    sp.Close();
                    close1 = false;
                }
            }
        }

        private static void serialAtuador(object obj)
        {
            SerialPort sp = (SerialPort)obj;

            while (sp.IsOpen)
            {
                if (close2)
                {
                    sp.Close();
                    close2 = false;
                }
                if (sendData2)
                {
                    if (valve)
                        sp.WriteLine("VALVE IS ON");
                    else
                        sp.WriteLine("VALVE IS OFF");

                    sendData2 = false;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.bt_iniciar.Text == "INICIAR")
            {
                try
                {
                    serialPort1.DataReceived += new SerialDataReceivedEventHandler(rxAck1);

                    this.enable(serialPort1);
                    ThreadSerial1 = new Thread(delSerial1);
                    ThreadSerial1.Start(this.serialPort1);
                }
                catch
                {
                    MessageBox.Show("Falha ao conectar o Sensor");
                }

                try
                {
                    serialPort2.DataReceived += new SerialDataReceivedEventHandler(rxAck2);

                    this.enable(serialPort2);
                    ThreadSerial2 = new Thread(delSerial2);
                    ThreadSerial2.Start(this.serialPort2);
                }
                catch
                {
                    MessageBox.Show("Falha ao conectar o Atuador");
                }
            }
            else
            {
                try
                {
                    this.disable(serialPort1);
                    ThreadSerial1.Abort();
                }
                catch
                {
                    MessageBox.Show("Falha ao desconectar o Sensor");
                }

                try
                {
                    this.disable(serialPort2);
                    ThreadSerial2.Abort();
                }
                catch
                {
                    MessageBox.Show("Falha ao desconectar o Atuador");
                }
            }
        }

        private void conexãoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormConfigConex fCC = new FormConfigConex(this.serialPort1);
            fCC.Show();
        }

        private void conexãoAtuadorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormConfigConex fCC = new FormConfigConex(this.serialPort2);
            fCC.Show();
        }

        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private static void rxAck1(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            sp.DiscardInBuffer();

            if (indata == "EOT")
            {
                Emulador.close1 = true;
            }

            if (indata == "REQ")
            {
                Emulador.sendData1 = true;
            }
        }

        private static void rxAck2(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            sp.DiscardInBuffer();

            if (indata == "EOT")
            {
                Emulador.close2 = true;
            }

            if (indata == "REQ")
            {
                Emulador.sendData2 = true;
            }

            if (indata == "ON")
            {
                Emulador.valve = true;
            }

            if (indata == "OFF")
            {
                Emulador.valve = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (valve)
            {
                level = level - 3.14;
            }
            else
            {
                level = level + 2.71;
            }
        }
    }
}
