using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Carrier_storage_system
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private string lbl_Code;
        private string x_value;
        private string y_value;
        private string z_value;

        private int work_qty = 0;

        private bool empty_check = false;

        string con = "DATA SOURCE = orcl; USER ID = scott; PASSWORD = 1234";
        OracleConnection conn;
        OracleCommand cmd;
        OracleDataReader reader;
        OracleDataAdapter ad;
        DataSet ds;
        DataTable dt;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            conn = new OracleConnection(con);
            cmd = new OracleCommand();
            ds = new DataSet();
            Weight_Check();
        }

        private void Insert_btn_Click(object sender, RoutedEventArgs e)
        {
            if (PSurName.Text == "" || PGivenName.Text == "" || PPassport.Text == "" || PNationality.Text == "" || PSex.Text == ""
                || PIssue.Text == "" || PExpiry.Text == "" || PBirth.Text == "" || ProductWeight.Text == "" || ProductWidth.Text == ""
                || ProductVertical.Text == "" || ProductHorizon.Text == "")
                MessageBox.Show("필수 입력 사항\r\n여권번호, 성, 이름, 국적, 성별,\r\n발급일, 만료일, 생년월일, 무게, 가로,\r\n세로, 폭 을 입력해주세요.");
            else
            {
                try
                {
                    Size_Check();
                    if (empty_check == true)
                    {
                        Search_Empty();
                        Lbl_code_make();
                        Insert_work();
                        PLC_work();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Close_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void Size_Check()
        {
            if (Int32.Parse(ProductWeight.Text) > 60)
            {
                MessageBox.Show("무게 초과");
                empty_check = false;
                return;
            }
            else if (Int32.Parse(ProductWidth.Text) > 40)
            {
                MessageBox.Show("가로길이 초과");
                empty_check = false;
                return;
            }
            else if (Int32.Parse(ProductVertical.Text) > 80)
            {
                MessageBox.Show("세로길이 초과");
                empty_check = false;
                return;
            }
            else if (Int32.Parse(ProductHorizon.Text) > 30)
            {
                MessageBox.Show("폭 길이 초과");
                empty_check = false;
                return;
            }
            else
            {
                empty_check = true;
            }
        }

        private void Weight_Check()
        {
            conn.Open();
            try
            {

                for (int i = 1; i < 6; i++)
                {
                    for (int j = 1; j < 5; j++)
                    {
                        dt = new DataTable();
                        ds = new DataSet();

                        string num;
                        int qty;
                        int text_num;

                        string sql = $"Select count(*), sum(weight), y, z " +
                            $"From WORK_TB " +
                            $"WHERE Y = {i} " +
                            $"And Z = {j}" +
                            $"Group By y, z";

                        ad = new OracleDataAdapter(sql, conn);
                        ad.Fill(ds, "list_tb");
                        dt = ds.Tables["list_tb"];

                        if (dt.Rows.Count > 0)
                        {
                            qty = 20 - Int32.Parse(dt.Rows[0]["COUNT(*)"].ToString());
                            num = dt.Rows[0]["SUM(WEIGHT)"].ToString();
                            text_num = 1200 - Int32.Parse(num);
                            switch (i)
                            {
                                case 1:
                                    OneFloor.Text = text_num.ToString() + "KG / " + qty.ToString();
                                    break;
                                case 2:
                                    TwoFloor.Text = text_num.ToString() + "KG / " + qty.ToString();
                                    break;
                                case 3:
                                    ThreeFloor.Text = text_num.ToString() + "KG / " + qty.ToString();
                                    break;
                                case 4:
                                    FourFloor.Text = text_num.ToString() + "KG / " + qty.ToString();
                                    break;
                                case 5:
                                    FiveFloor.Text = text_num.ToString() + "KG / " + qty.ToString();
                                    break;
                            }
                            work_qty += Int32.Parse(dt.Rows[0]["COUNT(*)"].ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            EmptyQty_txt.Text = (100 - work_qty).ToString();
            InputQty_txt.Text = work_qty.ToString();
            conn.Close();
        }

        private void Search_Empty()
        {
            conn.Open();
            try
            {
                for (int i = 1; i < 6; i++)
                {
                    for (int j = 1; j < 5; j++)
                    {
                        for (int k = 1; k < 6; k++)
                        {
                            string sql = $"Select PASS_NUM From WORK_TB Where x={k} And z={j} And y={i}";
                            cmd = new OracleCommand(sql, conn);

                            reader = cmd.ExecuteReader();
                            if (!reader.Read())
                            {
                                sql = $"Select SUM(WEIGHT) From WORK_TB Where y={i} And z={j}";
                                ad = new OracleDataAdapter(sql, conn);
                                ad.Fill(ds, "weight_tb");
                                dt = ds.Tables["weight_tb"];

                                if (dt.Rows.Count > 0)
                                {
                                    string sum = dt.Rows[0]["SUM(WEIGHT)"].ToString();
                                    if (Int32.Parse(ProductWeight.Text) + Int32.Parse(sum) < 300)
                                    {
                                        x_value = k.ToString();
                                        y_value = i.ToString();
                                        z_value = j.ToString();

                                        conn.Close();
                                        return;
                                    }
                                }
                                else
                                {
                                    x_value = k.ToString();
                                    y_value = i.ToString();
                                    z_value = j.ToString();

                                    conn.Close();
                                    return;
                                }
                            }
                        }
                    }

                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            conn.Close();
        }

        private void Lbl_code_make()
        {
            try
            {
                conn.Open();
                while (true)
                {
                    Random random = new Random();
                    int num = random.Next(1000000000, 2000000000);
                    lbl_Code = "C" + num.ToString(); ;

                    string sql = $"Select LBL_CODE From WORK_TB Where LBL_CODE = '{lbl_Code}' And  WORK_DT = TO_CHAR(SYSDATE, 'yyyymmdd')";
                    cmd = new OracleCommand(sql, conn);

                    OracleDataReader reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        break;
                    }
                }
                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void Insert_work()
        {
            conn.Open();
            try
            {
                cmd.Connection = conn;

                cmd.CommandText = $"Insert Into WORK_TB" +
                    $"(SUR_NAME, GIVEN_NAME, PASS_NUM, PASS_COUNTRY, PASS_SEX," +
                    $"ISSUE_DT, EXPIRY_DT, BIRTH_DT, WEIGHT, WIDTH," +
                    $"VERTICAL, HORIZON, WORK_FLAG, WORK_DT, LBL_CODE," +
                    $"X, Y, Z)" +
                    $"VALUES" +
                    $"('{PSurName.Text}', '{PGivenName.Text}', '{PPassport.Text}', '{PNationality.Text}', '{PSex.Text}'," +
                    $"'{PIssue.Text}', '{PExpiry.Text}', '{PBirth.Text}', {ProductWeight.Text}, {ProductWidth.Text}," +
                    $"{ProductVertical.Text}, {ProductHorizon.Text}, 'Y', TO_CHAR(SYSDATE, 'yyyyMMdd'), '{lbl_Code}'," +
                    $"{x_value}, {y_value}, {z_value})";
                cmd.ExecuteNonQuery();
                WindowClear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            conn.Close();
        }

        private void PLC_work()
        {
            //System.Threading.Thread.Sleep(3000);
            empty_check = false;
        }

        private void WindowClear()
        {
            if (empty_check == false)
                return;
            PSurName.Text = "";
            PGivenName.Text = "";
            PPassport.Text = "";
            PNationality.Text = "";
            PSex.Text = "";
            PIssue.Text = "";
            PExpiry.Text = "";
            PBirth.Text = "";
            ProductWeight.Text = "";
            ProductWidth.Text = "";
            ProductVertical.Text = "";
            ProductHorizon.Text = "";
            PType.Text = "";
            PCountry.Text = "";
            PPersonal.Text = "";
            PAuthority.Text = "";
            PFullname.Text = "";
        }
    }
}
