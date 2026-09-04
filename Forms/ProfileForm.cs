using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Scan-Profile verwalten (Button neben der Profil-Combo): die aktuellen Einstellungen
/// des Scan-Panels unter einem Namen speichern oder gespeicherte Profile löschen. Der Dialog
/// arbeitet auf einer Kopie der Liste — erst OK übernimmt die Änderungen.</summary>
internal sealed partial class ProfileForm : Form
{
    /// <summary>Die (bei OK zu übernehmende) Profilliste.</summary>
    public List<ScanProfile> Profiles { get; }

    private readonly ScanProfile current; // die aktuellen Panel-Einstellungen als Vorlage fürs Hinzufügen

    public ProfileForm(List<ScanProfile> profiles, ScanProfile current)
    {
        InitializeComponent();
        Lng.Apply(this);
        this.current = current;
        Profiles = profiles.Select(p => new ScanProfile // Kopie — Abbrechen lässt die Originale unberührt
        {
            Name = p.Name, DpiIndex = p.DpiIndex, ColorIndex = p.ColorIndex, AreaIndex = p.AreaIndex,
            FeedIndex = p.FeedIndex, Brightness = p.Brightness,
        }).ToList();
        RefreshList(null);
    }

    private void RefreshList(string selectName)
    {
        listProfiles.BeginUpdate();
        listProfiles.Items.Clear();
        foreach (var profile in Profiles) { listProfiles.Items.Add(profile.Name); }
        listProfiles.EndUpdate();
        if (selectName != null) { listProfiles.SelectedIndex = Profiles.FindIndex(p => p.Name == selectName); }
    }

    /// <summary>Speichert die aktuellen Scan-Einstellungen unter dem eingegebenen Namen;
    /// ein vorhandenes Profil gleichen Namens wird nach Rückfrage ersetzt.</summary>
    private void BtnAdd_Click(object sender, EventArgs e)
    {
        var name = textName.Text.Trim();
        if (name.Length == 0)
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Bitte gib einen Profilnamen an."), string.Empty, TaskDialogIcon.Warning);
            return;
        }
        var existing = Profiles.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            if (!TaskDlg.ConfirmTaskDlg(Handle, string.Format(Lng.T("Profil „{0}“ ersetzen?"), existing.Name),
                Lng.T("Die gespeicherten Einstellungen werden überschrieben."), defaultNo: true))
            {
                return;
            }
            Profiles.Remove(existing);
        }
        Profiles.Add(new ScanProfile
        {
            Name = name, DpiIndex = current.DpiIndex, ColorIndex = current.ColorIndex, AreaIndex = current.AreaIndex,
            FeedIndex = current.FeedIndex, Brightness = current.Brightness,
        });
        RefreshList(name);
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (listProfiles.SelectedIndex < 0) { return; }
        Profiles.RemoveAt(listProfiles.SelectedIndex);
        RefreshList(null);
        textName.Text = string.Empty;
    }

    private void ListProfiles_SelectedIndexChanged(object sender, EventArgs e)
    {
        btnDelete.Enabled = listProfiles.SelectedIndex >= 0;
        if (listProfiles.SelectedIndex >= 0) { textName.Text = (string)listProfiles.SelectedItem; }
    }
}
