using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;
using System.Xml.Linq;

namespace u16
{
    public struct Rate
    {
        public int Day;
        public int Month;
        public int Year;

        public double Value;
    }

    public partial class Form1 : Form
    {
        List<Rate> rates = new List<Rate>();
        List<Rate> cur_rates = new List<Rate>();
        string file;
        bool isDataInitialised;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog1.FilterIndex = 0;

            isDataInitialised = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                file = openFileDialog1.FileName;
            }

            isDataInitialised = GetRates();

            if (isDataInitialised)
            {
                int day1 = dateTimePicker1.Value.Day;
                int month1 = dateTimePicker1.Value.Month;
                int year1 = dateTimePicker1.Value.Year;
                int day2 = dateTimePicker2.Value.Day;
                int month2 = dateTimePicker2.Value.Month;
                int year2 = dateTimePicker2.Value.Year;

                Show(day1, month1, year1, day2, month2, year2);
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (isDataInitialised)
            {
                int day1 = dateTimePicker1.Value.Day;
                int month1 = dateTimePicker1.Value.Month;
                int year1 = dateTimePicker1.Value.Year;
                int day2 = dateTimePicker2.Value.Day;
                int month2 = dateTimePicker2.Value.Month;
                int year2 = dateTimePicker2.Value.Year;

                Show(day1, month1, year1, day2, month2, year2);
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (isDataInitialised)
            {
                int day1 = dateTimePicker1.Value.Day;
                int month1 = dateTimePicker1.Value.Month;
                int year1 = dateTimePicker1.Value.Year;
                int day2 = dateTimePicker2.Value.Day;
                int month2 = dateTimePicker2.Value.Month;
                int year2 = dateTimePicker2.Value.Year;

                Show(day1, month1, year1, day2, month2, year2);
            }
        }

        private bool GetRates()
        {
            XDocument xDoc = XDocument.Load(file);
            XElement root = xDoc.Element("ValCurs");
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "," };

            label2.Text = root.Attribute("name").Value;
            label4.Text = label6.Text = label5.Text = "";

            rates.Clear();
            foreach (XElement element in root.Elements("Record"))
            {
                string[] date_str = element.Attribute("Date").Value.Split('.');
                string value_str = element.Element("Value").Value.Replace(".", ",").Trim();
                int day, month, year;
                double value;

                try
                {
                    day = Convert.ToInt32(date_str[0]);
                    month = Convert.ToInt32(date_str[1]);
                    year = Convert.ToInt32(date_str[2]);

                    value = Convert.ToDouble(value_str, formatter);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("Ошибка при получении данных: неверно указано значение. ({0}).", ex.Message));
                    return false;
                }

                Rate rate = new Rate()
                {
                    Day = day,
                    Month = month,
                    Year = year,
                    Value = value
                };

                rates.Add(rate);
            }

            return true;
        }

        private void Show(int day1, int month1, int year1, int day2, int month2, int year2)
        {
            List<string> date_arr = new List<string>();
            List<double> value_arr = new List<double>();

            DateTime date1 = new DateTime(year1, month1, day1);
            DateTime date2 = new DateTime(year2, month2, day2);
            DateTime tempDate;

            cur_rates.Clear();
            chart1.Series[0].Points.Clear();
            foreach (Rate rate in rates)
            {
                tempDate = new DateTime(rate.Year, rate.Month, rate.Day);

                if (tempDate >= date1 && tempDate <= date2)
                {
                    string temp = rate.Year + "-" + rate.Month + "-" + rate.Day;
                    date_arr.Add(temp);
                    value_arr.Add(rate.Value);
                    cur_rates.Add(rate);
                }
            }

            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

            try
            {
                chart1.Series[0].Points.DataBindXY(date_arr, value_arr);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            Scale();
        }

        private void Scale()
        {
            double max = Double.MinValue;
            double min = Double.MaxValue;

            double leftLimit = chart1.ChartAreas[0].AxisX.Minimum;
            double rightLimit = chart1.ChartAreas[0].AxisX.Maximum;

            for (int s = 0; s < chart1.Series.Count(); s++)
            {
                foreach (DataPoint dp in chart1.Series[s].Points)
                {
                    if (dp.XValue >= leftLimit && dp.XValue <= rightLimit)
                    {
                        min = Math.Min(min, dp.YValues[0]);
                        max = Math.Max(max, dp.YValues[0]);
                    }
                }
            }
            chart1.ChartAreas[0].AxisY.Maximum = (Math.Ceiling((max / 10)) * 10);
            chart1.ChartAreas[0].AxisY.Minimum = (Math.Floor((min / 10)) * 10);
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePoint = new Point(e.X, e.Y);

            chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(mousePoint, true);
            chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(mousePoint, true);

            double positionX = chart1.ChartAreas[0].CursorX.Position - 1;
            double positionY = chart1.ChartAreas[0].CursorY.Position;

            if (cur_rates.Count > 0 && positionX >= 0 && cur_rates.Count > positionX)
            {
                Rate rate = cur_rates[(int)positionX];

                label4.Text = rate.Year + "-" + rate.Month + "-" + rate.Day;
                label6.Text = rate.Value.ToString();
                label5.Text = positionY.ToString();
            }
            else
            {
                label4.Text = label6.Text = label5.Text = "#";
            }
        }
    }
}
