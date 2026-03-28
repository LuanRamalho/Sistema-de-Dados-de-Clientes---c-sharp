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
        
        private FlowLayoutPanel panelCards = null!; // '!' avisa o compilador que será inicializado
        private TextBox txtBusca = null!;
        private Button btnAdicionar = null!, btnEditar = null!, btnExcluir = null!, btnBuscar = null!;
        private Cliente? clienteSelecionado = null;

        public FormMain()
        {
            ConfigurarInterface();
            CarregarDados();
            AtualizarCards();
        }

        private void ConfigurarInterface()
        {
            this.Text = "Sistema de CRM Moderno - Visual em Cards";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 245, 247);
            this.Font = new Font("Segoe UI", 10);

            // --- Painel Superior (Busca) ---
            Panel panelTop = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10), BackColor = Color.White };
            
            Label lblBusca = new Label { Text = "Buscar Cliente:", Location = new Point(20, 25), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            txtBusca = new TextBox { Location = new Point(150, 22), Width = 400, Font = new Font("Segoe UI", 12) };
            
            btnBuscar = CreateButton("Buscar", new Point(560, 18), Color.FromArgb(126, 3, 167));
            btnBuscar.Click += (s, e) => FiltrarClientes();

            panelTop.Controls.AddRange(new Control[] { lblBusca, txtBusca, btnBuscar });

            // --- Container de Cards ---
            panelCards = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(245, 245, 250)
            };

            // --- Painel Inferior (Ações) ---
            FlowLayoutPanel panelBottom = new FlowLayoutPanel { 
                Dock = DockStyle.Bottom, 
                Height = 80, 
                Padding = new Padding(15), 
                BackColor = Color.White,
                FlowDirection = FlowDirection.LeftToRight
            };

            btnAdicionar = CreateButton("Adicionar Cliente", Point.Empty, Color.FromArgb(0, 127, 108));
            btnEditar = CreateButton("Editar Selecionado", Point.Empty, Color.FromArgb(121, 122, 0));
            btnExcluir = CreateButton("Excluir Selecionado", Point.Empty, Color.FromArgb(200, 136, 0));

            btnAdicionar.Click += (s, e) => AbrirFormularioCliente(null);
            btnEditar.Click += (s, e) => EditarSelecionado();
            btnExcluir.Click += (s, e) => ExcluirSelecionado();

            panelBottom.Controls.AddRange(new Control[] { btnAdicionar, btnEditar, btnExcluir });

            this.Controls.Add(panelCards);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelBottom);
        }

        private Panel CriarCard(Cliente cliente)
        {
            Panel card = new Panel {
                Width = 280, Height = 160,
                BackColor = Color.White,
                Margin = new Padding(12),
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = cliente
            };

            Label lblNome = new Label { Text = cliente.Nome, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(15, 15), Width = 250, AutoSize = false };
            Label lblEmail = new Label { Text = "📧 " + cliente.Email, Location = new Point(15, 50), Width = 250, ForeColor = Color.DimGray };
            Label lblTel = new Label { Text = "📞 " + cliente.Telefone, Location = new Point(15, 75), Width = 250, ForeColor = Color.DimGray };
            Label lblEnd = new Label { Text = "📍 " + cliente.Endereco, Location = new Point(15, 100), Width = 250, Height = 50, ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8.5f) };

            Control[] innerControls = { lblNome, lblEmail, lblTel, lblEnd };
            foreach (var ctrl in innerControls) {
                ctrl.Click += (s, e) => SelecionarCard(card, cliente);
                card.Controls.Add(ctrl);
            }
            card.Click += (s, e) => SelecionarCard(card, cliente);

            return card;
        }

        private void SelecionarCard(Panel card, Cliente cliente)
        {
            clienteSelecionado = cliente;
            foreach (Control c in panelCards.Controls) {
                if (c is Panel p) p.BackColor = Color.White;
            }
            card.BackColor = Color.FromArgb(220, 235, 255);
        }

        private void AtualizarCards(List<Cliente>? listaSource = null)
        {
            panelCards.Controls.Clear();
            clienteSelecionado = null;
            var lista = listaSource ?? clientes;
            foreach (var c in lista) panelCards.Controls.Add(CriarCard(c));
        }

        private void CarregarDados()
        {
            if (File.Exists(databaseFile)) {
                try {
                    string json = File.ReadAllText(databaseFile);
                    clientes = JsonSerializer.Deserialize<List<Cliente>>(json) ?? new List<Cliente>();
                } catch { clientes = new List<Cliente>(); }
            }
        }

        private void SalvarDados()
        {
            string json = JsonSerializer.Serialize(clientes, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(databaseFile, json);
        }

        private void FiltrarClientes()
        {
            string t = txtBusca.Text.ToLower();
            var filtrados = clientes.Where(c => 
                (c.Nome?.ToLower().Contains(t) ?? false) || 
                (c.Email?.ToLower().Contains(t) ?? false)
            ).ToList();
            AtualizarCards(filtrados);
        }

        private void ExcluirSelecionado()
        {
            if (clienteSelecionado == null) {
                MessageBox.Show("Selecione um cliente clicando no card.", "Aviso");
                return;
            }

            if (MessageBox.Show($"Excluir {clienteSelecionado.Nome}?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                clientes.Remove(clienteSelecionado);
                SalvarDados();
                AtualizarCards();
            }
        }

        private void EditarSelecionado()
        {
            if (clienteSelecionado == null) {
                MessageBox.Show("Selecione um cliente primeiro.", "Aviso");
                return;
            }
            AbrirFormularioCliente(clienteSelecionado);
        }

        private void AbrirFormularioCliente(Cliente? cliente)
        {
            using (var form = new FormEdicaoCliente(cliente)) {
                if (form.ShowDialog() == DialogResult.OK) {
                    if (cliente == null && form.ClienteResult != null) 
                        clientes.Add(form.ClienteResult);
                    
                    SalvarDados();
                    AtualizarCards();
                }
            }
        }

        private Button CreateButton(string text, Point loc, Color color)
        {
            return new Button {
                Text = text, Location = loc, Size = new Size(170, 45),
                BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand,
                Margin = new Padding(5)
            };
        }
    }

    public class FormEdicaoCliente : Form {
        public Cliente? ClienteResult { get; private set; }
        private TextBox tNome, tEnd, tTel, tEmail;

        public FormEdicaoCliente(Cliente? c) {
            this.Text = c == null ? "Novo Cliente" : "Editar Cliente";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            Label l1 = new Label { Text = "Nome:", Location = new Point(20, 20), AutoSize = true };
            tNome = new TextBox { Location = new Point(20, 40), Width = 340, Text = c?.Nome };

            Label l2 = new Label { Text = "Endereço:", Location = new Point(20, 90), AutoSize = true };
            tEnd = new TextBox { Location = new Point(20, 110), Width = 340, Text = c?.Endereco };

            Label l3 = new Label { Text = "Telefone:", Location = new Point(20, 160), AutoSize = true };
            tTel = new TextBox { Location = new Point(20, 180), Width = 340, Text = c?.Telefone };

            Label l4 = new Label { Text = "Email:", Location = new Point(20, 230), AutoSize = true };
            tEmail = new TextBox { Location = new Point(20, 250), Width = 340, Text = c?.Email };

            Button btnSalvar = new Button { Text = "Salvar", Location = new Point(130, 330), Size = new Size(120, 40), BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSalvar.Click += (s, e) => {
                ClienteResult = c ?? new Cliente();
                ClienteResult.Nome = tNome.Text;
                ClienteResult.Endereco = tEnd.Text;
                ClienteResult.Telefone = tTel.Text;
                ClienteResult.Email = tEmail.Text;
                this.DialogResult = DialogResult.OK;
            };

            this.Controls.AddRange(new Control[] { l1, tNome, l2, tEnd, l3, tTel, l4, tEmail, btnSalvar });
        }
    }
}
