using BlubLib.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebZen.Util;

namespace MU.Tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async Task ServerList(string path)
        {
            var content = await File.ReadAllBytesAsync(path);

            byte[] block, block2;
            string additionalInfo;
            for(var i = 0; i < content.Length; )
            {
                block = content.Skip(i).ToArray();
                block2 = block.Take(39).ToArray();
                BuxDecode.Decode(block2);
                using (var br = new MemoryStream(block))
                {
                    br.Seek(0, SeekOrigin.Begin);
                    br.Write(block2);
                    br.Seek(0, SeekOrigin.Begin);

                    var result = Serializer.Deserialize<ServerList>(br);
                    if(result.descriptionLength > 0)
                    {
                        block = new byte[result.descriptionLength];
                        br.Read(block);
                        BuxDecode.Decode(block);
                        additionalInfo = block.MakeString();
                    }
                    i += (int)br.Position;
                }                
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();

            if (result != DialogResult.OK)
                return;

            switch(dialog.SafeFileName.ToLower())
            {
                case "serverlist.bmd":
                    await ServerList(dialog.FileName);
                    break;
            }
        }
    }
}
