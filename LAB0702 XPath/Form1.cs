using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace LAB0702_XPath
{
    public partial class Form1 : Form
    {
        APD66_64011212155Entities context = new APD66_64011212155Entities();
        List<ProductJIB> productjiblist;
        int count;
        int imagcount;
        private ProductJIB[] productJIBs; // หรือ private List<ProductJIB> productJIBs;


        public Form1()
        {
            InitializeComponent();
            textBox5.Text = "https://www.jib.co.th/web/product/product_list/2/46";
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox7.Clear();
            pictureBox1.Image = null;

            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();


            string url = textBox5.Text;
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(url);

            var listproduct = doc.DocumentNode.Descendants("div");
            var products = listproduct.Where(m => m.GetAttributeValue("data-qty", "")== "1").ToList();

            //จำนวนหน้า
            var page = doc.DocumentNode.Descendants("ul");
            try
            {
                var pageproduct = page.Where(m => m.GetAttributeValue("class", "") == "pagination").First();
                count = pageproduct.Descendants("a").Count() - 1;
                textBox6.Text = (count).ToString();
            }
            catch {
                count = 1;
                textBox6.Text = 1.ToString();
            }

            //ประเภท
            var type = doc.DocumentNode.Descendants("div");
            var title = type.Where(m => m.GetAttributeValue("data-qty", "") == "1")
                .First().GetAttributeValue("data-name", "").ToString();
            textBox7.Text = GetIndextype(title);
        }

        string GetIndextype(string input)
        {
            var index = input.Split('(');
            index = index[1].Split(')');

            return index[0];
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            Console.WriteLine(5678);
            productjiblist = new List<ProductJIB>();

            for (int i = 0; i < count; i++)
            {
                string url = textBox5.Text + "/" + (i * 100).ToString();
                Console.WriteLine(url);

                // Load page asynchronously
                HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = await web.LoadFromWebAsync(url);

                var listproduct = doc.DocumentNode.Descendants("div");
                var products = listproduct.Where(m => m.GetAttributeValue("data-qty", "") == "1").ToList();
                foreach (var product in products)
                {
                    string urlInfo = "https://www.jib.co.th/web/product/readProduct/" + product.GetAttributeValue("data-id", "");

                    // Load product info asynchronously
                    HtmlWeb web2 = new HtmlWeb();
                    HtmlAgilityPack.HtmlDocument doc2 = await web2.LoadFromWebAsync(urlInfo);
                    var htmlMeta = doc2.DocumentNode.Descendants("meta");
                    int Pid = int.Parse(product.GetAttributeValue("data-id", ""));
                    string ProductName = htmlMeta.FirstOrDefault(m => m.GetAttributeValue("property", "") == "og:title")?.GetAttributeValue("content", "");
                    string Detail = htmlMeta.FirstOrDefault(m => m.GetAttributeValue("property", "") == "og:description")?.GetAttributeValue("content", "");

                    var htmlDiv = doc2.DocumentNode.Descendants("div");
                    int Price = 0;
                    var priceNode = htmlDiv.FirstOrDefault(m => m.GetAttributeValue("class", "") == "col-lg-12 col-md-12 col-sm-12 col-xs-12 text-center price_block");
                    if (priceNode != null)
                    {
                        string priceText = new string(priceNode.InnerText.Where(char.IsDigit).ToArray());
                        int.TryParse(priceText, out Price);
                    }

                    string Photo = htmlMeta.FirstOrDefault(m => m.GetAttributeValue("property", "") == "og:image")?.GetAttributeValue("content", "");

                    productjiblist.Add(new ProductJIB { Pid = Pid, ProductName = ProductName, Detail = Detail, Photo = Photo, Price = Price });
                    Console.WriteLine(1234);

                    var row = new string[] { product.GetAttributeValue("data-id", ""), product.GetAttributeValue("data-name", "") };
                    dataGridView1.Rows.Add(row);
                }
            }
        }




        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowindex = e.RowIndex;
            DataGridViewRow row = dataGridView1.Rows[rowindex];
            string id = row.Cells[0].Value.ToString();

            //loop หาจำนวนหน้า
            foreach (DataGridViewCell cell in row.Cells)
            {
                Console.WriteLine(cell.Value.ToString());
            }

            //แสดงข้อมูล
            showInfo(id);
        }

        private void showInfo(string id)
        {
            string urlInfo = "https://www.jib.co.th/web/product/readProduct/" + id;
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(urlInfo);
            var htmlMeta = doc.DocumentNode.Descendants("meta");
            textBox1.Text = id;

            var title = htmlMeta.Where(m => m.GetAttributeValue("property", "") == "og:title").First();
            textBox2.Text = title.GetAttributeValue("content", "");

            var description = htmlMeta.Where(m => m.GetAttributeValue("property", "") == "og:description").First();
            textBox3.Text = description.GetAttributeValue("content", "");

            var htmlDiv = doc.DocumentNode.Descendants("div");
            var price = htmlDiv.Where(m => m.GetAttributeValue("class", "") == "col-lg-12 col-md-12 col-sm-12 col-xs-12 text-center price_block").First().InnerText;
            price = new string(price.Where(p => char.IsDigit(p)).ToArray());
            textBox4.Text = price;

            var image = htmlMeta.Where(m => m.GetAttributeValue("property", "") == "og:image").First();
            pictureBox1.Image = loadImage(image.GetAttributeValue("content", ""));
        }
        private Image loadImage(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "GET";
            httpWebRequest.UserAgent = "Chome/105.0.0.0";
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream stream = httpWebResponse.GetResponseStream();
            Bitmap bitmap = new Bitmap(stream);
            stream.Close();
            httpWebResponse.Close();
            return bitmap;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ProductJIB productJIB in productjiblist)
            {
                // ตรวจสอบว่ามีข้อมูลที่มี Pid ซ้ำกันใน context.ProductJIBs หรือไม่
                if (!context.ProductJIBs.Any(p => p.Pid == productJIB.Pid))
                {
                    // ถ้าไม่มี Pid ซ้ำกัน ก็ทำการเพิ่มข้อมูลใหม่
                    context.ProductJIBs.Add(new ProductJIB
                    {
                        Pid = productJIB.Pid,
                        ProductName = productJIB.ProductName,
                        Detail = productJIB.Detail,
                        Photo = productJIB.Photo,
                        Price = productJIB.Price
                    });
                }
            }

            // จากนั้นเรียก SaveChanges() เพื่อบันทึกการเปลี่ยนแปลง
            int change = context.SaveChanges();
            MessageBox.Show("Changed " + change + " records");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Clear existing items in the listView1
            listView1.Clear();
            textBox14.Clear();
            textBox13.Clear();
            textBox12.Clear();
            textBox11.Clear();
            pictureBox2.Image = null;

            // Retrieve products from the database
            productJIBs = context.ProductJIBs.ToArray();


            // Create an ImageList for storing images
            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(100, 100);

            // Get the desired number of items from the numericUpDown1 control
            int count = (int)numericUpDown1.Value;

            // Limit count to the number of items available
            count = Math.Min(count, productJIBs.Length);

            // Randomly select products from the database
            Random random = new Random();
            HashSet<int> selectedIndices = new HashSet<int>();
            while (selectedIndices.Count < count)
            {
                selectedIndices.Add(random.Next(0, productJIBs.Length));
            }

            // Assign the ImageList to the LargeImageList property of the listView1
            listView1.LargeImageList = imageList;

            // Loop through each randomly selected product and add it to the listView1
            foreach (int index in selectedIndices)
            {
                // Load the image for the product
                Image image = loadImage(productJIBs[index].Photo); // Assuming Photo contains the URL of the image

                // Add the image to the ImageList
                imageList.Images.Add(productJIBs[index].Pid.ToString(), image);

                // Create a new ListViewItem with the product name and associate it with the corresponding image
                ListViewItem item = new ListViewItem(productJIBs[index].ProductName);
                item.ImageKey = productJIBs[index].Pid.ToString(); // Use the product ID as the key to retrieve the image from the ImageList

                // Add the ListViewItem to the listView1
                listView1.Items.Add(item);
            }
  
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("listviewclick");
            if (listView1.SelectedItems.Count > 0)
            {
                // หากมีรายการที่ถูกเลือกใน ListView
                ListViewItem selectedItem = listView1.SelectedItems[0];
                string productId = selectedItem.ImageKey; // รับ product ID จาก ImageKey

                // ค้นหาข้อมูลสินค้าจาก ProductJIBs
                ProductJIB selectedProduct = productJIBs.FirstOrDefault(p => p.Pid.ToString() == productId);

                // แสดงข้อมูลของสินค้าที่เลือกใน TextBoxes
                if (selectedProduct != null)
                {
                    textBox14.Text = $"{selectedProduct.Pid}\r\n";
                    textBox13.Text = $"{selectedProduct.ProductName}\r\n";
                    textBox12.Text = $"{selectedProduct.Detail}\r\n";
                    textBox11.Text = $"{selectedProduct.Price}\r\n";

                    // สร้าง QR Code จาก Product ID และแสดงใน PictureBox2
                    QrCodeEncodingOptions options = new QrCodeEncodingOptions();
                    options.CharacterSet = "UTF-8";
                    options.Width = 200;
                    options.Height = 130;

                    BarcodeWriter writer = new BarcodeWriter();
                    writer.Format = BarcodeFormat.QR_CODE; // กำหนดให้สร้าง QR Code
                    writer.Options = options;

                    // สร้าง QR Code จาก Product ID และชื่อ Product
                    var result = writer.Write($"{selectedProduct.Pid}\n {selectedProduct.ProductName}");

                    // แสดง QR Code ใน PictureBox2
                    pictureBox2.Image = result;
                }
            }
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
