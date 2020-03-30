using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Encrypted_Module_Generator {
	public partial class MainWindow : Window {
		readonly Brush defaultFG;
		public MainWindow() {
			InitializeComponent();
			defaultFG = pb.Foreground;
			cfg = new EncryptedServerConfig();
			localFiles = new List<string>();
		}
		[Import]
		IServerCrypto crypto;
		readonly EncryptedServerConfig cfg;
		EncryptedServerConfig.EncryptionType encryptionType;
		readonly List<string> localFiles;
		string serverFilePath, cryptoPath;
		readonly bool[] status = new[] { false, false, false, false, false, false };
		void ShowStatus() {
			pb.Maximum = status.Length;
			var v = 0;
			foreach (var s in status)
				if (s) v++;
			pb.Value = v;
			pb.Foreground = !status[0] || !status[4] || (status[2] && !status[3]) ? new SolidColorBrush(Colors.Red) : defaultFG;
		}
		private void Button_Click(object sender, RoutedEventArgs e) {
			var dlg = new Microsoft.Win32.OpenFileDialog {
				DefaultExt = "*.dll",
				CheckFileExists = true,
				CheckPathExists = true,
				Multiselect = false,
				RestoreDirectory = true,
				DereferenceLinks = true,
				Title = "Please select a Server DLL to encrypt",
				Filter = "Server Files *.dll|*.dll"
			};
			if (dlg.ShowDialog() == true) {
				serverFilePath = dlg.FileName;
				cfg.ServerFileName = Path.GetFileName(dlg.FileName);
				(sender as Button).Content = cfg.ServerFileName;
				status[0] = true;
				ShowStatus();
			}
		}
		private void Button_Click_1(object sender, RoutedEventArgs e) {
			var dlg = new Microsoft.Win32.OpenFileDialog {
				DefaultExt = "*.*",
				CheckFileExists = true,
				CheckPathExists = true,
				Multiselect = true,
				RestoreDirectory = true,
				DereferenceLinks = true,
				Title = "Please select Additional Files to encrypt",
				Filter = "Any File *.*|*.*"
			};
			if (dlg.ShowDialog() == true) {
				foreach (var f in dlg.FileNames) {
					localFiles.Add(f);
					var b = new Button() {
						Height = 15,
						Width = 15,
						Content = "-",
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Center,
						FontSize = 21,
						Padding = new Thickness(0, -10, 0, 0),
						Style = FindResource("BTStyle") as Style
					};
					var g = new StackPanel() {
						HorizontalAlignment = HorizontalAlignment.Stretch,
						Orientation = Orientation.Horizontal,
						Children ={
							b,
							new Label(){
								Content = Path.GetFileName(f),
								VerticalAlignment = VerticalAlignment.Stretch,
								VerticalContentAlignment = VerticalAlignment.Center,
								Foreground=new SolidColorBrush(Colors.White)
							}
						}
					};
					b.Click += (a, c) => {
						lbAdditional.Items.Remove(g);
						localFiles.Remove(f);
					};
					lbAdditional.Items.Add(g);
				}
				status[1] = true;
				ShowStatus();
			}
		}
		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (btEnc == null) return;
			switch ((sender as ComboBox).SelectedIndex) {
				case (0):
					encryptionType = EncryptedServerConfig.EncryptionType.AES;
					btEnc.Visibility = Visibility.Collapsed;
					status[2] = true;
					status[3] = true;
					ShowStatus();
					break;
				case (1):
					encryptionType = EncryptedServerConfig.EncryptionType.CUSTOM;
					btEnc.Visibility = Visibility.Visible;
					status[2] = true;
					if (crypto == null)
						status[3] = false;
					ShowStatus();
					break;
			}
		}
		private void Button_Click_2(object sender, RoutedEventArgs e) {
			var dlg = new Microsoft.Win32.OpenFileDialog {
				DefaultExt = "*.dll",
				CheckFileExists = true,
				CheckPathExists = true,
				Multiselect = false,
				RestoreDirectory = true,
				DereferenceLinks = true,
				Title = "Please select a DLL that Implements IServerCrypto",
				Filter = "Crypto Files *.dll|*.dll"
			};
			if (dlg.ShowDialog() == true) {
				crypto = null;
				status[3] = false;
				try {
					using (var cat = new AggregateCatalog()) {
						cat.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(dlg.FileName), Path.GetFileName(dlg.FileName)));
						var loader = new CompositionContainer(cat);
						loader.ComposeParts(this);
					}
					cryptoPath = dlg.FileName;
					status[3] = true;
					(sender as Button).Content = Path.GetFileName(dlg.FileName);
				}
				catch (Exception ex) {
					MessageBox.Show(ex.ToString(), "Error Loading Crypto");
					return;
				}
				finally {
					ShowStatus();
				}
			}
		}
		private void Button_Click_3(object sender, RoutedEventArgs e) {
			if (pwA.Password != pwB.Password) {
				MessageBox.Show("Passwords unequal", "Error");
				return;
			}
			if (pwA.Password.Length == 0) {
				MessageBox.Show("No Password given", "Error");
				return;
			}
			if (string.IsNullOrWhiteSpace(serverFilePath)) {
				MessageBox.Show("No Server DLL Specified", "Error");
				return;
			}
			status[1] = true;
			pb.IsIndeterminate = true;
			var dlg = new Microsoft.Win32.SaveFileDialog() {
				RestoreDirectory = true,
				AddExtension = true,
				DefaultExt = "*.edll",
				FileName = Path.GetFileNameWithoutExtension(serverFilePath),
				DereferenceLinks = true,
				Title = "Save as",
				ValidateNames = true,
				Filter = "Encrypted Server *.edll|*.edll"
			};
			if (dlg.ShowDialog() == false) {
				pb.IsIndeterminate = false;
				return;
			}
			switch (encryptionType) {
				case EncryptedServerConfig.EncryptionType.AES:
					crypto = new ServerCryptoAES();
					break;
			}
			cfg.Encryption = encryptionType;
			var enc = new CryptoWrapper<IServerCrypto>(crypto);
			var mutated = enc.KeyMutation(pwA.SecurePassword);
			var dat = new Dictionary<string, byte[]>();
			try {
				foreach (var f in localFiles) {
					var sh = Path.GetFileName(f);
					dat.Add(sh, enc.Encrypt(File.ReadAllBytes(f), mutated));
					cfg.EncryptedFiles.Add(sh);
				}
				dat.Add(cfg.ServerFileName, enc.Encrypt(File.ReadAllBytes(serverFilePath), mutated));
				if (encryptionType == EncryptedServerConfig.EncryptionType.CUSTOM) {
					cfg.CryptoFileName = Path.GetFileName(cryptoPath);
					dat.Add(cfg.CryptoFileName, File.ReadAllBytes(cryptoPath));
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.ToString(), "ERROR reading and encrypting files");
				pb.IsIndeterminate = false;
				return;
			}
			try {
				if (File.Exists(dlg.FileName)) File.Delete(dlg.FileName);
				using (var fs = File.Open(dlg.FileName, FileMode.CreateNew)) {
					using (var a = new ZipArchive(fs, ZipArchiveMode.Create)) {
						foreach (var k in dat.Keys)
							using (var eStream = a.CreateEntry(k).Open())
								eStream.Write(dat[k], 0, dat[k].Length);
						using (var configStream = a.CreateEntry(EncryptedServerConfig.ConfigFileName).Open())
							cfg.Save(configStream);
					}
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.ToString(), "ERROR compressing files");
				pb.IsIndeterminate = false;
				return;
			}
			status[5] = true;
			pb.IsIndeterminate = false;
			ShowStatus();
		}
		private void PasswordChanged(object sender, RoutedEventArgs e) {
			var eq = pwA.Password == pwB.Password;
			if (status[4] != eq) {
				status[4] = eq;
				ShowStatus();
			}
		}
	}
}
