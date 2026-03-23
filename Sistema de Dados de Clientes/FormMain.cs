using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace SistemaCRM
{
    public partial class FormMain : Form
    {
        private List<Cliente> clientes = new List<Cliente>();
        private readonly string databaseFile = "crm_data.json";

        private DataGridView gridClientes;
        private TextBox txtBusca;
        private Button btnAdicionar, btnEditar, btnExcluir, btnBuscar;

        public FormMain()
        {
            ConfigurarInterface();
            CarregarDados();
            AtualizarGrid();
        }

        private void ConfigurarInterface()
        {
            this.Text = "Sistema de CRM Moderno";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(224, 242, 247);

            Panel panelTop = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };
            Label lblBusca = new Label { Text = "Buscar Cliente:", Location = new Point(15, 25), AutoSize = true, Font = new Font("Arial", 12, FontStyle.Bold) };
            txtBusca = new TextBox { Location = new Point(140, 22), Width = 400, Font = new Font("Arial", 12) };
            btnBuscar = CreateButton("Buscar", new Point(550, 18), Color.FromArgb(126, 3, 167));
            btnBuscar.Click += (s, e) => FiltrarClientes();

            panelTop.Controls.AddRange(new Control[] { lblBusca, txtBusca, btnBuscar });

            gridClientes = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 40
            };
            gridClientes.EnableHeadersVisualStyles = false;
            gridClientes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(208, 224, 227);
            gridClientes.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);

            FlowLayoutPanel panelBottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 70, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(10) };
            btnAdicionar = CreateButton("Adicionar Cliente", Point.Empty, Color.FromArgb(0, 127, 108));
            btnEditar = CreateButton("Editar Cliente", Point.Empty, Color.FromArgb(121, 122, 0));
            btnExcluir = CreateButton("Excluir Cliente", Point.Empty, Color.FromArgb(200, 136, 0));

            btnAdicionar.Click += (s, e) => AbrirFormularioCliente(null);
            btnEditar.Click += (s, e) => EditarSelecionado();
            btnExcluir.Click += (s, e) => ExcluirSelecionado();

            panelBottom.Controls.AddRange(new Control[] { btnAdicionar, btnEditar, btnExcluir });

            this.Controls.Add(gridClientes);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelBottom);
        }

        private Button CreateButton(string text, Point location, Color color)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = new Size(160, 40),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        private void CarregarDados()
        {
            try 
            {
                if (File.Exists(databaseFile))
                {
                    string json = File.ReadAllText(databaseFile);
                    clientes = JsonSerializer.Deserialize<List<Cliente>>(json) ?? new List<Cliente>();
                }
            }
            catch { clientes = new List<Cliente>(); }
        }

        private void SalvarDados()
        {
            string json = JsonSerializer.Serialize(clientes, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(databaseFile, json);
        }

        private void AtualizarGrid(List<Cliente> listaSource = null)
        {
            // Vincula a lista diretamente para evitar erros de índice manual
            gridClientes.DataSource = null;
            gridClientes.DataSource = (listaSource ?? clientes).ToList();
        }

        private void FiltrarClientes()
        {
            var termo = txtBusca.Text.ToLower();
            var filtrados = clientes.Where(c => 
                (c.Nome?.ToLower().Contains(termo) ?? false) || 
                (c.Email?.ToLower().Contains(termo) ?? false)
            ).ToList();
            AtualizarGrid(filtrados);
        }

        private void ExcluirSelecionado()
        {
            // Verificação de segurança para evitar o erro de índice -1
            if (gridClientes.CurrentRow != null && gridClientes.CurrentRow.DataBoundItem is Cliente cliente)
            {
                if (MessageBox.Show($"Deseja excluir {cliente.Nome}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    clientes.Remove(cliente);
                    SalvarDados();
                    AtualizarGrid();
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecione um cliente na lista primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void EditarSelecionado()
        {
            // Verificação de segurança para evitar o erro de índice -1
            if (gridClientes.CurrentRow != null && gridClientes.CurrentRow.DataBoundItem is Cliente cliente)
            {
                AbrirFormularioCliente(cliente);
            }
            else
            {
                MessageBox.Show("Por favor, selecione um cliente na lista primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AbrirFormularioCliente(Cliente clienteExistente)
        {
            Form form = new Form
            {
                Text = clienteExistente == null ? "Novo Cliente" : "Editar Cliente",
                Size = new Size(400, 380),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label CreateLabel(string txt, int y) => new Label { Text = txt, Location = new Point(20, y), AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold) };
            TextBox CreateTxt(string val, int y) => new TextBox { Text = val, Location = new Point(20, y + 20), Width = 340, Font = new Font("Arial", 10) };

            var txtNome = CreateTxt(clienteExistente?.Nome ?? "", 20);
            var txtEnd = CreateTxt(clienteExistente?.Endereco ?? "", 80);
            var txtTel = CreateTxt(clienteExistente?.Telefone ?? "", 140);
            var txtEmail = CreateTxt(clienteExistente?.Email ?? "", 200);

            Button btnSalvar = new Button { 
                Text = "Salvar", 
                Location = new Point(140, 280), 
                Size = new Size(100, 40), 
                BackColor = Color.FromArgb(0, 127, 108), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            btnSalvar.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtNome.Text)) {
                    MessageBox.Show("O campo Nome é obrigatório!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (clienteExistente == null) {
                    clientes.Add(new Cliente { 
                        Nome = txtNome.Text, 
                        Endereco = txtEnd.Text, 
                        Telefone = txtTel.Text, 
                        Email = txtEmail.Text 
                    });
                } else {
                    clienteExistente.Nome = txtNome.Text;
                    clienteExistente.Endereco = txtEnd.Text;
                    clienteExistente.Telefone = txtTel.Text;
                    clienteExistente.Email = txtEmail.Text;
                }

                SalvarDados();
                AtualizarGrid();
                form.Close();
            };

            form.Controls.AddRange(new Control[] { 
                CreateLabel("Nome:", 20), txtNome, 
                CreateLabel("Endereço:", 80), txtEnd, 
                CreateLabel("Telefone:", 140), txtTel, 
                CreateLabel("Email:", 200), txtEmail, 
                btnSalvar 
            });
            form.ShowDialog();
        }
    }
}