using System;
using System.Collections;
using System.Collections.Generic;
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

namespace IPinfo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			byte[] addressbytes = new byte[4];
			string[] addressString = new string[4];

			//haal de verschillende delen van het IP adres uit de textbox
			addressString = address.Text.Split('.');
			/* legacy debug
			Console.WriteLine("addressString:");
			Console.WriteLine(addressString[0]);
			*/

			//verander de delen naar bytes
			for(byte a = 0; a < addressString.Length; a++)
			{
				Byte.TryParse(addressString[a], out addressbytes[a]);
			}

			//-------------------------------------
			//Dit deel behandelt het netwerkgedeelte
			//-------------------------------------

			//zet netwerkgedeelte om naar binair (32-bits lang)
			bool[] networkbits = new bool[32];
			byte networkpart;
			Byte.TryParse(network.Text, out networkpart);

			//zet alle 32 bits op "0"
			for(byte a = 0; a <= 31; a++)
			{
				networkbits[a] = false;
			}

			//bereken hoveel bits logisch "1" moeten worden gezet
			//
			//times is hoeveel bytes "1" moeten worden
			//rectimes is hoeveel bijkomende bits "1" moeten worden
			byte times = Convert.ToByte((networkpart) / 8);
			byte rectimes = Convert.ToByte((networkpart) % 8);

			//zet de nodige bytes op "1"
			for (byte a = 0; a < times; a++)
			{
				for(byte b = 8; b > 0; b--)
				{
					networkbits[a*8+(b-1)] = true;
				}
			}

			//zet alle nodige bits op "1"
			if(rectimes > 0)
			{
				for(byte a = 7; a > 7-rectimes; a--)
				{
					networkbits[times * 8 + a] = true;
				}
			}

			//we maken het netwerkgedeelte (bin)
			BitArray networkbin = new BitArray(networkbits);
			netmask.Content = IpBinToString(networkbin);
			/* legacy debug
			Console.WriteLine("networkbin:");
			PrintValues(networkbin, 8);
			*/

			//verander de IPv4 delen naar binair
			BitArray addressbin = new BitArray(addressbytes);
			//Aanmaken van Broadcast Address (BAD)
			//
			//Het BAD is het netwerkgedeelte overgenomen en alle hostbits op "1"
			bool[] bad = new bool[32];
			for(byte a = 0; a <= 31; a++)
			{
				bad[a] = addressbin[a];
			}
			Console.WriteLine("bad:");
			PrintValues(bad, 8);
			times = Convert.ToByte((32 - networkpart) / 8);
			rectimes = Convert.ToByte((32 - networkpart) % 8);

			if(times > 0)
			{
				for(byte a = 1; a <= times; a++)
				{
					for(byte b = 1; b <= 8; b++)
					{
						bad[31 - a*8 + b] = true;
					}
				}
			}
			if (rectimes > 0)
			{
				for (byte a = 1; a <= rectimes; a++)
				{
					bad[31 - (times + 1) * 8 + a] = true;
				}
			}
			BitArray badbin = new BitArray(bad);
			/* legacy debug
			Console.WriteLine("addressbin:");
			PrintValues(addressbin, 8);
			Console.WriteLine("badbin:");
			PrintValues(badbin, 8);
			*/

			//laat waarden zien op het scherm
			string broadcastadstr = IpBinToString(badbin);
			broadcastad.Content = broadcastadstr;

			//Aanmaken van het Network Address (NAD)
			//
			//Het NAD is het netwerkgedeelte overgenomen en alle hostbits op "0"
			bool[] nad = new bool[32];
			for(byte a = 0; a <= 31; a++)
			{
				nad[a] = addressbin[a];
			}

			times = Convert.ToByte((32 - networkpart) / 8);
			rectimes = Convert.ToByte((32 - networkpart) % 8);

			if (times > 0)
			{
				for (byte a = 1; a <= times; a++)
				{
					for (byte b = 1; b <= 8; b++)
					{
						nad[31 - a * 8 + b] = false;
					}
				}
			}
			if (rectimes > 0)
			{
				for (byte a = 1; a <= rectimes; a++)
				{
					nad[31 - (times + 1) * 8 + a] = false;
				}
			}

			BitArray nadbin = new BitArray(nad);
			/* legacy debug
			Console.WriteLine("nadbin:");
			PrintValues(nadbin, 8);
			*/
			string networkadstr = IpBinToString(nadbin);
			networkad.Content = networkadstr;

			//Aanmaken van beschikbare IP range
			//
			//Lower limit is 1 boven netwerkadres, Upper limit is 1 onder broadcast adres

			//Lower Limit
			string[] ladrstr = new string[4];
			ladrstr = networkadstr.Split('.');
			byte[] ladrbyte = new byte[4];
			for(byte a = 0; a <= 3; a++)
			{
				Byte.TryParse(ladrstr[a], out ladrbyte[a]);
			}
			ladrbyte[3]++;
			rangeL.Content = String.Join(".", ladrbyte);

			//Upper Limit
			string[] hadrstr = new string[4];
			hadrstr = broadcastadstr.Split('.');
			byte[] hadrbyte = new byte[4];
			for(byte a = 0; a <= 3; a++)
			{
				Byte.TryParse(hadrstr[a], out hadrbyte[a]);
			}
			hadrbyte[3]--;

			rangeH.Content = String.Join(".", hadrbyte);
		}

		//functie om BitArray om te zetten in een IP-adres (string)
		//
		//Waardes gescheiden via '.'
		public static string IpBinToString(BitArray ipbit)
		{
			byte IPvalue;
			byte[] retvals = new byte[4];
			for(byte a = 0; a <= 3; a++)
			{
				IPvalue = 0;
				for(byte b = 0; b <= 7; b++)
				{
					if(ipbit[a*8+b])
					{
						IPvalue += Convert.ToByte(Math.Pow(2, b));
						Console.Write("IPvalue");
						Console.WriteLine(IPvalue);
					}
					retvals[a] = IPvalue;
				}
			}
			return String.Join(".", retvals);
		}

		//Functie om bitarrays te kunnen printen
		//gebruik myWidth = 8 om 8 bits per lijn te printen (1 byte)
		//
		//Code hergebruikt van https://docs.microsoft.com/en-us/dotnet/api/system.collections.bitarray?view=netframework-4.8 (Examples)
		public static void PrintValues(IEnumerable myList, int myWidth)
		{
			int i = myWidth;
			foreach (Object obj in myList)
			{
				if (i <= 0)
				{
					i = myWidth;
					Console.WriteLine();
				}
				i--;
				Console.Write("{0,8}", obj);
			}
			Console.WriteLine();
		}
	}
}
