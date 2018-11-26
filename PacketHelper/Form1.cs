using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WebZen.Network;

namespace PacketHelper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SimpleModulus.LoadDecryptionKey("Dec2.dat");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var clear = textBox1.Text.Split(new char[] { ' ', '\t' });
            var ms = new MemoryStream();
            var i = 0;
            foreach(var c in clear)
            {
                if (!string.IsNullOrWhiteSpace(c))
                    ms.WriteByte(byte.Parse(c, System.Globalization.NumberStyles.HexNumber));

                i++;
            }

            var stream = new byte[ms.Length];
            Array.Copy(ms.GetBuffer(), stream, stream.Length);
            ms.Dispose();

            byte[] dec;
            try
            {
                if (stream[0] == 0xC3)
                {
                    var substream = new byte[stream.Length - 2];
                    Array.Copy(stream, 2, substream, 0, substream.Length);
                    dec = SimpleModulus.Decoder(substream);
                }
                else
                {
                    var substream = new byte[stream.Length - 3];
                    Array.Copy(stream, 3, substream, 0, substream.Length);
                    dec = SimpleModulus.Decoder(substream);
                }
            }catch(Exception)
            {
                textBox2.Text = "Invalid Packet";
                return;
            }


            textBox2.Text = string.Join(" ", dec.Select(x => x.ToString("X2")));
        }
    }
}
