using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FileExtensionChanger
{
    public partial class FileExtensionChangerForm : Form
    {
        private List<string> selectedFiles = new List<string>();
        
        public FileExtensionChangerForm()
        {
            InitializeComponent();
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                folderBrowser.Description = "选择包含要修改的文件的文件夹";
                
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    txtFolderPath.Text = folderBrowser.SelectedPath;
                    LoadFilesFromFolder(folderBrowser.SelectedPath);
                }
            }
        }

        private void LoadFilesFromFolder(string folderPath)
        {
            try
            {
                lstFiles.Items.Clear();
                selectedFiles.Clear();
                
                string[] files = Directory.GetFiles(folderPath);
                foreach (string file in files)
                {
                    lstFiles.Items.Add(Path.GetFileName(file));
                }
                
                lblStatus.Text = $"找到 {files.Length} 个文件";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载文件夹内容时出错: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lstFiles.Items.Count; i++)
            {
                lstFiles.SetSelected(i, true);
            }
        }

        private void btnDeselectAll_Click(object sender, EventArgs e)
        {
            lstFiles.ClearSelected();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFolderPath.Text) && Directory.Exists(txtFolderPath.Text))
            {
                LoadFilesFromFolder(txtFolderPath.Text);
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("请至少选择一个文件", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string newExtension = txtNewExtension.Text.Trim();
            if (string.IsNullOrEmpty(newExtension))
            {
                MessageBox.Show("请输入新的扩展名", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 确保扩展名格式正确，不包含点号
            newExtension = newExtension.TrimStart('.');

            string folderPath = txtFolderPath.Text;
            int successCount = 0;
            List<string> failedFiles = new List<string>();

            foreach (var item in lstFiles.SelectedItems)
            {
                string oldFilename = item.ToString();
                string oldFilePath = Path.Combine(folderPath, oldFilename);
                
                // 分离文件名和扩展名
                string baseName = Path.GetFileNameWithoutExtension(oldFilename);
                string newFilename = $"{baseName}.{newExtension}";
                string newFilePath = Path.Combine(folderPath, newFilename);
                
                try
                {
                    // 如果新文件已存在，先询问是否覆盖
                    if (File.Exists(newFilePath) && oldFilename != newFilename)
                    {
                        DialogResult result = MessageBox.Show(
                            $"文件 '{newFilename}' 已存在，是否覆盖?", 
                            "文件已存在",
                            MessageBoxButtons.YesNo, 
                            MessageBoxIcon.Question);
                            
                        if (result == DialogResult.No)
                        {
                            failedFiles.Add($"{oldFilename} (用户取消)");
                            continue;
                        }
                        
                        // 如果用户同意覆盖，先删除目标文件
                        File.Delete(newFilePath);
                    }
                    
                    // 重命名文件
                    File.Move(oldFilePath, newFilePath);
                    successCount++;
                }
                catch (Exception ex)
                {
                    failedFiles.Add($"{oldFilename} ({ex.Message})");
                }
            }

            // 刷新文件列表
            LoadFilesFromFolder(folderPath);
            
            // 显示结果
            string resultMessage = $"成功更改了 {successCount} 个文件的扩展名";
            if (failedFiles.Count > 0)
            {
                resultMessage += $"\n\n以下文件处理失败:\n{string.Join("\n", failedFiles)}";
            }
            
            MessageBox.Show(resultMessage, "处理结果", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            lblStatus.Text = $"成功更改了 {successCount} 个文件的扩展名";
        }
    }
}