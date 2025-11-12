using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms;

public static class TicketStatus
{
    public const string Aberto = "Aberto";
    public const string EmAtendimento = "EmAtendimento";
    public const string Encerrado = "Encerrado";

    public static void Bind(ComboBox combo, bool includeEmpty = true)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Items.Clear();
        if (includeEmpty) combo.Items.Add("");
        combo.Items.Add(Aberto);
        combo.Items.Add(EmAtendimento);
        combo.Items.Add(Encerrado);
        combo.SelectedIndex = 0;
    }
}
