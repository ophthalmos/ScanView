using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Profilverwaltung (Link „Profile verwalten" über der Profil-Combo), zwei GroupBoxen:
/// „Neues Profil" speichert die Panel-Werte (als Klartext aufgelistet) unter dem Namen aus
/// textName. „Gespeicherte Profile" wirkt auf das markierte Listenprofil: Löschen, Umsortieren
/// und das Umbenennen-Feld, das die Markierung vorbefüllt — es ändert nur den Namen, nie die
/// gespeicherten Werte. Der Dialog arbeitet auf einer Kopie der Liste; „Änderungen speichern"
/// (aktiv erst nach der ersten Änderung) übernimmt sie in die MainForm.</summary>
internal sealed partial class ProfileForm : Form
{
    /// <summary>Die (bei OK zu übernehmende) Profilliste.</summary>
    public List<ScanProfile> Profiles { get; }

    /// <summary>Aktueller Name des beim Öffnen in der MainForm gewählten Profils — folgt einem
    /// Umbenennen und Umsortieren, null nach dem Löschen. Damit behält die Profil-Combo
    /// ihre Auswahl, egal was im Dialog markiert wurde.</summary>
    public string TrackedName => tracked != null && Profiles.Contains(tracked) ? tracked.Name : null;

    private readonly ScanProfile tracked; // das beim Öffnen gewählte Profil (Objekt der Dialog-Kopie)

    private readonly ScanProfile current; // die aktuellen Panel-Einstellungen als Vorlage fürs Hinzufügen

    public ProfileForm(List<ScanProfile> profiles, ScanProfile current, string currentSummary, string selectedName)
    {
        InitializeComponent();
        Lng.Apply(this);
        labelSettings.Text = currentSummary; // die Panel-Werte im Klartext — das speichert der Button
        btnOk.Left = btnCancel.Left - 6 - btnOk.Width; // rechtsbündig neben Abbrechen (AutoSize, Textbreite je Sprache)
        this.current = current;
        Profiles = [.. profiles.Select(p => new ScanProfile // Kopie — Abbrechen lässt die Originale unberührt
        {
            Name = p.Name, DpiIndex = p.DpiIndex, ColorIndex = p.ColorIndex, AreaIndex = p.AreaIndex,
            FeedIndex = p.FeedIndex, Brightness = p.Brightness,
        })];
        tracked = Profiles.FirstOrDefault(p => p.Name == selectedName);
        RefreshList(selectedName); // das in der MainForm gewählte Profil startet markiert
        if (listProfiles.SelectedIndex >= 0) { ActiveControl = textRename; } // Umbenennen ist dann der nächstliegende Zweck
    }

    private void RefreshList(string selectName)
    {
        listProfiles.BeginUpdate();
        listProfiles.Items.Clear();
        foreach (var profile in Profiles) { listProfiles.Items.Add(profile.Name); }
        listProfiles.EndUpdate();
        if (selectName != null) { listProfiles.SelectedIndex = Profiles.FindIndex(p => p.Name == selectName); }
    }

    /// <summary>Speichert die aktuellen Scan-Einstellungen unter dem in textName eingegebenen
    /// Namen (ein vorhandenes Profil gleichen Namens wird nach Rückfrage ersetzt).</summary>
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
        btnOk.Enabled = true; // es gibt jetzt etwas zu speichern
    }

    /// <summary>Gibt dem markierten Profil den Namen aus dem Umbenennen-Feld —
    /// die gespeicherten Einstellungen bleiben unangetastet.</summary>
    private void BtnRename_Click(object sender, EventArgs e)
    {
        var selectedIndex = listProfiles.SelectedIndex;
        if (selectedIndex < 0) { return; }
        var name = textRename.Text.Trim();
        if (name.Length == 0)
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Bitte gib einen Profilnamen an."), string.Empty, TaskDialogIcon.Warning);
            return;
        }
        if (Profiles.Where((p, i) => i != selectedIndex).Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Es gibt bereits ein Profil mit diesem Namen."), string.Empty, TaskDialogIcon.Warning);
            return;
        }
        Profiles[selectedIndex].Name = name;
        RefreshList(name);
        btnOk.Enabled = true;
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (listProfiles.SelectedIndex < 0) { return; }
        Profiles.RemoveAt(listProfiles.SelectedIndex);
        RefreshList(null);
        btnOk.Enabled = true;
    }

    /// <summary>Verschiebt das markierte Profil in der Reihenfolge (auch der Profil-Combo).</summary>
    private void MoveSelected(int offset)
    {
        var index = listProfiles.SelectedIndex;
        var target = index + offset;
        if (index < 0 || target < 0 || target >= Profiles.Count) { return; }
        (Profiles[index], Profiles[target]) = (Profiles[target], Profiles[index]);
        RefreshList(Profiles[target].Name);
        btnOk.Enabled = true;
    }

    private void BtnUp_Click(object sender, EventArgs e) => MoveSelected(-1);

    private void BtnDown_Click(object sender, EventArgs e) => MoveSelected(1);

    /// <summary>Die Markierung steuert die untere Zone: Löschen/Verschieben/Umbenennen sind nur
    /// mit Markierung aktiv, das Umbenennen-Feld wird mit dem markierten Namen vorbefüllt.</summary>
    private void ListProfiles_SelectedIndexChanged(object sender, EventArgs e)
    {
        var index = listProfiles.SelectedIndex;
        btnDelete.Enabled = index >= 0;
        btnUp.Enabled = index > 0;
        btnDown.Enabled = index >= 0 && index < listProfiles.Items.Count - 1;
        btnRename.Enabled = index >= 0;
        textRename.Text = index >= 0 ? (string)listProfiles.SelectedItem : string.Empty;
    }

    /// <summary>Solange der Fokus im jeweiligen Namensfeld steht, löst Enter das Speichern
    /// bzw. Umbenennen aus statt den Dialog zu schließen.</summary>
    private void TextName_Enter(object sender, EventArgs e) => AcceptButton = btnAdd;

    private void TextName_Leave(object sender, EventArgs e) => AcceptButton = btnOk;

    private void TextRename_Enter(object sender, EventArgs e) => AcceptButton = btnRename;

    private void TextRename_Leave(object sender, EventArgs e) => AcceptButton = btnOk;

    /// <summary>Klick auf die leere Listenfläche hebt die Markierung auf.</summary>
    private void ListProfiles_MouseDown(object sender, MouseEventArgs e)
    {
        if (listProfiles.IndexFromPoint(e.Location) < 0) { listProfiles.ClearSelected(); }
    }
}
